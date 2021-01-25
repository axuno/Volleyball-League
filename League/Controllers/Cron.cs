using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using League.BackgroundTasks;
using League.Emailing.Creators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

#nullable enable
namespace League.Controllers
{
    public class Cron : AbstractController
    {
        private readonly ITenantContext _tenantContext;
        private readonly IAuthorizationService _authorizationService;
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
        private readonly ILogger<Cron> _logger;
        private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
        private readonly SendEmailTask _sendMailTask;
        private readonly TenantStore _tenantStore;
        private readonly IMemoryCache _cache;
        private const int DoNotExecute = 0; // zero would mean a notification on the match day
        
        public Cron(TenantStore tenantStore, ITenantContext tenantContext, IAuthorizationService authorizationService,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, Axuno.BackgroundTask.IBackgroundQueue queue,
            SendEmailTask sendMailTask, IMemoryCache cache, ILogger<Cron> logger)
        {
            _tenantContext = tenantContext;
            _authorizationService = authorizationService;
            _timeZoneConverter = timeZoneConverter;
            _queue = queue;
            _sendMailTask = sendMailTask;
            _tenantStore = tenantStore;
            _cache = cache;
            _logger = logger;
        }
        
        [HttpGet("/cron/automail/all")]
        public async Task<ContentResult> RunAll()
        {
            var urlSegments = new List<string>();
            foreach (var (_, tenant) in _tenantStore.GetTenants())
            {
                if (!tenant.IsDefault) urlSegments.Add(tenant.SiteContext.UrlSegmentValue);
            }

            var tasks = new Task<ValueTuple<bool, string>>[urlSegments.Count];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = InvokeUrl(urlSegments[i]);
            }

            var sb = new StringBuilder();
            sb.Append($"Running {tasks.Length} jobs...\n");

            try
            {
                await Task.WhenAll(tasks);
                sb.Append("Tasks completed.\n");
                foreach (var task in tasks)
                {
                    var (success, url) = task.Result;
                    sb.AppendFormat("{0}: {1}\n", success ? "Success" : "Failure", url);
                }
            }
            catch (AggregateException e)
            {
                foreach (var ex in e.InnerExceptions)
                {
                    sb.Append($"Error: {ex.Message}\n");
                }
            }

            return Content(sb.ToString(), "text/plain", Encoding.UTF8);
        }
        
        
        [HttpGet("{organization:MatchingTenant}/cron/automail/{datetime?}")]
        public ContentResult AutoMail(string? datetime)
        {
            var forceDate = datetime?.EndsWith("!") ?? false;

            if (datetime == null || !DateTime.TryParse(datetime.TrimEnd('!'), out var cronDateTime))
            {
                cronDateTime = DateTime.UtcNow;
            }
            
            // Strip hours, minutes, seconds
            cronDateTime = new DateTime(cronDateTime.Year, cronDateTime.Month, cronDateTime.Day);

            string message;
            if (forceDate || !HasAlreadyRun(cronDateTime))
            {
                message = $"Queuing jobs for {(string.IsNullOrEmpty(datetime) ? "today" : cronDateTime.ToString("d", CultureInfo.InvariantCulture))}";
                _logger.LogInformation(message);
                QueueJobs(cronDateTime);
            }
            else
            {
                message = $"Jobs already executed for {(string.IsNullOrEmpty(datetime) ? "today" : cronDateTime.ToString("d", CultureInfo.InvariantCulture))}";
                _logger.LogInformation(message);
            }

            return Content(message);
        }

        /// <summary>
        /// Checks, whether the job was already run at the given date.
        /// </summary>
        /// <param name="datetime">The date to check. Hours/minutes/seconds must be set to zero.</param>
        /// <returns></returns>
        private bool HasAlreadyRun(DateTime datetime)
        {
            // This is not fool-proof because we use the application cache,
            // but subsequent invocations will be caught.

            const string cronCacheName = "CronCache";
            var cronCache = cronCacheName + _tenantContext.Identifier + datetime.ToString("O");

            if (_cache.TryGetValue(cronCacheName, out _))
                return true;

            _cache.Remove(cronCacheName);
            _cache.Set(cronCacheName, cronCache,
                new MemoryCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(23), Priority = CacheItemPriority.Normal});

            return false;
        }
        
        private void QueueJobs(DateTime referenceDateUtc)
        {
            var smt = _sendMailTask.CreateNewInstance();

            if (_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextmatch != DoNotExecute)
            {
                smt.SetMessageCreator(new AnnounceNextMatchCreator
                {
                    Parameters =
                    {
                        CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                        ReferenceDateUtc =
                            referenceDateUtc.AddDays(_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextmatch * -1),
                        IcsCalendarBaseUrl = Url.Action(nameof(Calendar), nameof(Match),
                            new {Organization = _tenantContext.SiteContext.UrlSegmentValue},
                            Url.ActionContext.HttpContext.Request.Scheme)
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

        private async Task<ValueTuple<bool, string>> InvokeUrl(string urlSegmentValue)
        {
            using var httpClient = new HttpClient();
            string url = string.Empty;
            try
            {
                url = Url.Action(nameof(AutoMail), nameof(Cron),
                    new {organization = urlSegmentValue}, Uri.UriSchemeHttps);

                await httpClient.GetAsync(url);
            }
            catch
            {
                _logger.LogError("Error after sending get request for {0}", url);
                return (false, url);
            }

            _logger.LogInformation("Get request for {0} completed.", url);
            return (true, url);
        }
    }
}