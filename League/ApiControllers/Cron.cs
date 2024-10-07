using League.BackgroundTasks;
using League.Controllers;
using League.Emailing.Creators;
using League.MultiTenancy;
using League.Routing;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TournamentManager.MultiTenancy;


namespace League.ApiControllers;

/// <summary>
/// Web api service controller executing scheduled tasks.
/// </summary>
/// <remarks>
/// The api may be called by an external scheduled tasks service, or by an internal background task.
/// The reason for designing an <see cref="ApiControllerAttribute"/> is the need for
/// <see cref="Routing"/> and <see cref="HttpContent"/> specific data.
/// </remarks>
[ApiController]
[Route("api/[controller]")]
public class Cron : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Cron> _logger;
    private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
    private readonly SendEmailTask _sendMailTask;
    private readonly TenantStore _tenantStore;
    private readonly IMemoryCache _cache;
    private const int DoNotExecute = 0; // zero would mean a notification on the match day

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="tenantStore"></param>
    /// <param name="tenantContext"></param>
    /// <param name="configuration"></param>
    /// <param name="queue"></param>
    /// <param name="sendMailTask"></param>
    /// <param name="cache"></param>
    /// <param name="logger"></param>
    public Cron(TenantStore tenantStore, ITenantContext tenantContext, IConfiguration configuration,
        Axuno.BackgroundTask.IBackgroundQueue queue,
        SendEmailTask sendMailTask, IMemoryCache cache, ILogger<Cron> logger)
    {
        _tenantContext = tenantContext;
        _configuration = configuration;
        _queue = queue;
        _sendMailTask = sendMailTask;
        _tenantStore = tenantStore;
        _cache = cache;
        _logger = logger;
    }
        
    /// <summary>
    /// Runs all auto mails for the date/time when the method is invoked.
    /// </summary>
    /// <param name="key">The authentication key for the service.</param>
    /// <returns></returns>
    [HttpGet("automail/all/{key}")]
    public async Task<IActionResult> RunAllAutoMails(string key)
    {
        if(!IsAuthorized(key)) return Unauthorized("Incorrect authorization key");
            
        var urlSegments = new List<string>();
        foreach (var (_, tenant) in _tenantStore.GetTenants())
        {
            if (!tenant.IsDefault) urlSegments.Add(tenant.SiteContext.UrlSegmentValue);
        }

        var tasks = new Task<InvocationResult>[urlSegments.Count];
        for (var i = 0; i < tasks.Length; i++)
        {
            tasks[i] = InvokeUrl(urlSegments[i]);
        }

        var results = new List<InvocationResult>();
            
        try
        {
            await Task.WhenAll(tasks);
                
            foreach (var task in tasks)
            {
                results.Add(task.Result);
            }
        }
        catch (AggregateException e)
        {
            _logger.LogCritical(e, "Failure invoking automail Urls.");
        }
            
        return Ok(results);
    }
        
        
    /// <summary>
    /// Queues all auto mail jobs for the given date.
    /// </summary>
    /// <remarks>Note: If a tenant is used, it must be the first route key by definition.</remarks>
    /// <param name="key">The authentication key for the service.</param>
    /// <param name="referenceDate">The reference date which will be taken for queuing mails.</param>
    /// <returns></returns>
    [HttpGet("/" + TenantRouteConstraint.Template + "/api/[controller]/automail/{key}/{referenceDate?}")]
    public IActionResult AutoMail(string key, string? referenceDate)
    {
        if(!IsAuthorized(key)) return Unauthorized("Incorrect authorization key");
            
        var forceDate = referenceDate?.EndsWith('!') ?? false;

        if (referenceDate == null || !DateTime.TryParse(referenceDate.TrimEnd('!'), out var cronDateTime))
        {
            cronDateTime = DateTime.UtcNow;
        }
            
        // Strip hours, minutes, seconds
        cronDateTime = new DateTime(cronDateTime.Year, cronDateTime.Month, cronDateTime.Day, 0, 0, 0, DateTimeKind.Utc);

        var formattedDate = cronDateTime.ToString("d", CultureInfo.InvariantCulture);
        if (forceDate || !HasAlreadyRun(cronDateTime))
        {
            _logger.LogInformation("Queuing jobs for {CronDateTime}", formattedDate);
            QueueJobs(cronDateTime);
            // success
            return Ok(new QueuingResult {Success = true, ReferenceDate = cronDateTime, Message = $"Queuing jobs for {formattedDate}" });
        }

        // failure
        formattedDate = (string.IsNullOrEmpty(referenceDate)
            ? "today"
            : cronDateTime.ToString("d", CultureInfo.InvariantCulture));
        _logger.LogInformation("Job already executed for {LastJobExecution}", formattedDate);
        return Ok(new QueuingResult {Success = false, ReferenceDate = cronDateTime, Message = $"Job already executed for {formattedDate}" });
    }

    /// <summary>
    /// Checks, whether the job was already run at the given date. The date per <see cref="TenantContext.Identifier"/> will be cached.
    /// </summary>
    /// <param name="referenceDate">The date to check. Hours/minutes/seconds must be set to zero.</param>
    /// <returns>Returns <see langword="true" /> if the <see ref="referenceDate"/> exists in the <see cref="IMemoryCache"/> for the <see cref="TenantContext.Identifier"/></returns>
    private bool HasAlreadyRun(DateTime referenceDate)
    {
        // This is not fool-proof because we use the application cache,
        // but subsequent invocations will be caught.

        const string cronCacheName = "CronCache";
        var cronCache = cronCacheName + _tenantContext.Identifier + referenceDate.ToString("O");

        if (_cache.TryGetValue(cronCache, out _))
            return true;

        _cache.Remove(cronCache);
        _cache.Set(cronCache, cronCache,
            new MemoryCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(23), Priority = CacheItemPriority.Normal});

        return false;
    }
        
    /// <summary>
    /// Sets up a all <see cref="IMailMessageCreator"/>s and queues them for background execution.
    /// </summary>
    /// <param name="referenceDateUtc">The reference date which is used to calculate data for the mail template.</param>
    private void QueueJobs(DateTime referenceDateUtc)
    {
        var smt = _sendMailTask.CreateNewInstance();

        if (_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextMatch != DoNotExecute)
        {
            smt.SetMessageCreator(new AnnounceNextMatchCreator
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc =
                        referenceDateUtc.AddDays(_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextMatch * -1),
                    IcsCalendarBaseUrl = TenantLink.ActionLink(nameof(Calendar), nameof(Match), null,
                        scheme: TenantLink.HttpContext.Request.Scheme) ?? string.Empty
                }
            });
            _queue.QueueTask(smt);
        }

        if (_tenantContext.SiteContext.MatchNotifications.DaysForMatchResultReminder1 != DoNotExecute)
        {
            smt = _sendMailTask.CreateNewInstance();
            smt.SetMessageCreator(new RemindMatchResult
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc = referenceDateUtc.AddDays(_tenantContext.SiteContext.MatchNotifications
                        .DaysForMatchResultReminder1 * -1)
                }
            });
            _queue.QueueTask(smt);
        }

        if (_tenantContext.SiteContext.MatchNotifications.DaysForMatchResultReminder2 != DoNotExecute)
        {
            smt = _sendMailTask.CreateNewInstance();
            smt.SetMessageCreator(new UrgeMatchResultCreator
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc = referenceDateUtc.AddDays(_tenantContext.SiteContext.MatchNotifications
                        .DaysForMatchResultReminder2 * -1)
                }
            });
            _queue.QueueTask(smt);
        }
    }

    private async Task<InvocationResult> InvokeUrl(string urlSegmentValue)
    {
        using var httpClient = new HttpClient();
        var url = string.Empty;
        try
        {
            var routeValues = new RouteValueDictionary {
                { TenantRouteConstraint.Key, urlSegmentValue },
                { "key", GetAuthKey() }
            };
            url = GeneralLink.GetUriByAction(HttpContext, nameof(AutoMail), nameof(Cron),
                routeValues, scheme: Uri.UriSchemeHttps) ?? string.Empty;

            var result = await httpClient.GetAsync(url);
            _logger.LogInformation("Get request for url '{Url}' completed.", url);
            return new InvocationResult
            {
                Success = true, Url = url,
                QueuingResult =
                    JsonConvert.DeserializeObject<QueuingResult>(await result.Content.ReadAsStringAsync()) ?? new QueuingResult(),
                Exception = null
            };
        }
        catch(Exception e)
        {
            const string message = "Error after sending get request for url '{Url}'";
            _logger.LogError(e, message, url);
            var now = DateTime.UtcNow;
                
            return new InvocationResult
            {
                Success = true, Url = url,
                QueuingResult = new QueuingResult
                    {Success = false, ReferenceDate = new DateTime(now.Year, now.Month, now.Day, 0,0,0, now.Kind), Message = $"Error after sending get request for url '{url}'" },
                // depth of inner exceptions could lead to JSON serialization exception, so create a new one
                Exception = new InvalidOperationException(e.Message){Source = e.Source} 
            };
        }
    }

    private bool IsAuthorized(string key)
    {
        if (key == GetAuthKey())
        {
            _logger.LogInformation("Scheduled task was authorized");
            return true;
        }
        _logger.LogInformation("Scheduled task could not be authorized");
        return false;
    }

    private string GetAuthKey()
    {
        var key = _configuration.GetSection("ScheduledTaskKey").Value;
        if (string.IsNullOrWhiteSpace(key)) _logger.LogCritical("ScheduledTaskKey is null or whitespace");
        return key ?? string.Empty;
    }
        
    /// <summary>
    /// Class that stores the result of an <see cref="Cron.AutoMail"/> invocation.
    /// </summary>
    public class InvocationResult
    {
        /// <summary>
        /// <see langword="true"/> if the <see cref="Url"/> could be invoked successfully, else <see langword="false"/>.
        /// </summary>
        public bool Success { get; set; }
            
        /// <summary>
        /// Gets or sets the url that was invoked for the <see cref="Cron.AutoMail"/> api call.
        /// </summary>
        public string Url { get; set; } = string.Empty;
            
        /// <summary>
        /// Gets or set the result of the <see cref="Cron.AutoMail"/> api call.
        /// </summary>
        public QueuingResult QueuingResult { get; set; } = new();
            
        /// <summary>
        /// Gets or sets any invocation exception
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Class that stores the results of a call to <see cref="Cron.AutoMail"/> api.
    /// </summary>
    public class QueuingResult
    {
        /// <summary>
        /// <see langword="true"/> if the mail message was queued, else <see langword="false"/>.
        /// </summary>
        public bool Success { get; set; }
            
        /// <summary>
        /// Gets or sets the reference date that is used for generating mail messages.
        /// </summary>
        public DateTime ReferenceDate { get; set; }
            
        /// <summary>
        /// Gets or sets the plain text explanation of the queuing result.
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}
