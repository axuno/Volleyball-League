using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
    [Route("{organization:MatchingTenant}/[controller]")]
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
        
        public async Task<ContentResult> RunAll()
        {
            var urlSegments = new List<string>();
            foreach (var (_, value) in _tenantStore.GetTenants())
            {
                if (!value.IsDefault) urlSegments.Add(value.SiteContext.UrlSegmentValue);
            }

            var tasks = new Task<string>[urlSegments.Count];
            for (var i = 0; i < tasks.Length; i++)
            {
                tasks[i] = InvokeUrl(urlSegments[i]);
            }

            var sb = new StringBuilder();
            sb.Append($"Running {tasks.Length} jobs...\n");

            try
            {
                await Task.WhenAll(tasks);
                sb.Append("Tasks completed successfully.\n");
                foreach (var task in tasks)
                {
                    sb.Append($"{task.Result}\n");
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
        
        
        [HttpGet("{organization:OnlyValidOrganizations}/cron/automail/{datetime?}")]
        public async Task<ContentResult> AutoMail(string? datetime)
        {
            var forceDate = datetime?.EndsWith("!") ?? false;
            if (forceDate) datetime = datetime?.TrimEnd('!');

            if (datetime == null || !DateTime.TryParse(datetime, out DateTime cronDateTime))
            {
                cronDateTime = DateTime.Now;
            }
			
            // Todo: Cron inactive !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if (false)
            {
                return Content($"<pre>Configuration for '{_tenantContext.Identifier}' is not active.</pre>");
            }

            if (forceDate || RunOncePerDay(cronDateTime))
            {
                Index();
            }
            else
            {
                return Content("Cron already executed for " + (string.IsNullOrEmpty(datetime) ? "today" : datetime));
            }

            // item1 = Log, item2 = message
            return Content("<pre>Cron executed</pre>");
        }

        private bool RunOncePerDay(DateTime datetime)
        {
            // This is not fool-proof because we use the application cache,
            // but subsequent invocations will be catched.

            const string cronCacheName = "CronCache";
            var cronCache = cronCacheName + _tenantContext.Identifier + datetime;

            if (_cache.TryGetValue(cronCache, out _))
                return false;

            _cache.Remove(cronCache);
            _cache.Set(cronCacheName, cronCache,
                new MemoryCacheEntryOptions {AbsoluteExpiration = DateTimeOffset.UtcNow.AddHours(23), Priority = CacheItemPriority.Normal});

            return true;
        }
        

        [HttpGet("")]
        public IActionResult Index()
        {
            var smt = _sendMailTask.CreateNewInstance();
            smt.SetMessageCreator(new RemindMatchResult
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc = new DateTime(2018, 07, 27)
                }
            });

            _queue.QueueTask(smt);
            
            smt = _sendMailTask.CreateNewInstance();
            smt.SetMessageCreator(new AnnounceNextMatchCreator
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc = new DateTime(2018, 07, 27),
                    IcsCalendarUrl = Url.Action(nameof(Calendar), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue, id = "{0}" }, Url.ActionContext.HttpContext.Request.Scheme)
                }
            });
            _queue.QueueTask(smt);
            
            smt = _sendMailTask.CreateNewInstance();
            smt.SetMessageCreator(new UrgeMatchResultCreator()
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ReferenceDateUtc = new DateTime(2018, 07, 27)
                }
            });
            _queue.QueueTask(smt);
            
            return Content("Create reminders");
        }

        private async Task<string> InvokeUrl(string tenantIdentifier)
        {
            var httpClient = new HttpClient();
            try
            {
                var url = Url.Action(nameof(AutoMail), nameof(Cron),
                    new { organization = tenantIdentifier }, Uri.UriSchemeHttps);

                await httpClient.GetAsync(url);
            }
            finally
            {
                httpClient.Dispose();
            }

            return tenantIdentifier;
        }
    }
}