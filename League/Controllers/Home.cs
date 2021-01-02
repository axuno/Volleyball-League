using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Axuno.Web;
using League.BackgroundTasks.Email;
using League.Models.AccountViewModels;
using League.Models.HomeViewModels;
using League.MultiTenancy;
using League.Routing;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;


namespace League.Controllers
{
    [Route("")]
    public class Home : AbstractController
    {
        private readonly AppDb _appDb;
        private readonly ITenantContext _tenantContext;
        private readonly TenantStore _tenantStore;
        private readonly IStringLocalizer<Account> _localizer;
        private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
        private readonly ContactEmailTask _contactEmailTask;
        private readonly ILogger<Home> _logger;

        public Home(ITenantContext tenantContext, TenantStore tenantStore, Axuno.BackgroundTask.IBackgroundQueue queue, ContactEmailTask contactEmailTask, ILogger<Home> logger, IStringLocalizer<Account> localizer)
        {
            _tenantContext = tenantContext;
            _tenantStore = tenantStore;
            _appDb = _tenantContext.DbContext.AppDb;
            _queue = queue;
            _contactEmailTask = contactEmailTask;
            _logger = logger;            
            _localizer = localizer;
            _tenantContext = tenantContext;
        }

        [Route("")]
        public IActionResult Index()
        {
            if (Request.Cookies.TryGetValue(CookieNames.MostRecentTenant, out var urlSegmentValue) && _tenantStore.GetTenants().Any(sl => sl.Value.SiteContext.UrlSegmentValue == urlSegmentValue))
            {
                if (!string.IsNullOrEmpty(urlSegmentValue)) return Redirect($"/{urlSegmentValue}");
            }

            return Redirect(Url.Action(nameof(Welcome), nameof(Home)));
        }

        [Route("/[action]")]
        public IActionResult Welcome()
        {
            return View(ViewNames.Home.Welcome);
        }

        [Route("/about-league")]
        public IActionResult AboutLeague()
        {
            return View(ViewNames.Home.AboutLeague);
        }

        [Route("/[action]")]
        public IActionResult Overview()
        {
            return View(ViewNames.Home.Overview);
        }

        [Route("/legal-disclosure")]
        public IActionResult LegalDisclosure()
        {
            return View(ViewNames.Home.LegalDisclosure);
        }

        [Route("/[action]")]
        public IActionResult Privacy()
        {
            return View(ViewNames.Home.Privacy);
        }

        [Route("/picture-credits")]
        public IActionResult PictureCredits()
        {
            return View(ViewNames.Home.PictureCredits);
        }

        [HttpGet("{organization:MatchingTenant}/[action]", Name = RouteNames.HomeOrganizationContact)]
        [HttpGet("/[action]", Name = RouteNames.HomeGeneralContact)]
        public IActionResult Contact()
        {
            var model = new ContactViewModel();
            if (User.Identity.IsAuthenticated)
            {
                model.Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                model.Gender = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Gender)?.Value;
                model.FirstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value;
                model.LastName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value;
            }
            
            // Note: The form will use route name to generate the action url
            // Also, if we are in an organization context, the organization contact url will be used, otherwise the general one
            return View(ViewNames.Home.Contact, model);
        }

        [HttpPost("{organization:MatchingTenant}/[action]", Name = RouteNames.HomeOrganizationContact)]
        [HttpPost("/[action]", Name = RouteNames.HomeGeneralContact)]
        public IActionResult Contact(ContactViewModel model)
        {
            if (model.Captcha != HttpContext.Session.GetString(CaptchaSvgGenerator.CaptchaSessionKeyName))
            {
                ModelState.AddModelError(nameof(CreateAccountViewModel.Captcha), _localizer["Math task result was incorrect"]);
            }
            if (!ModelState.IsValid)
            {
                return View(ViewNames.Home.Contact, model);
            }

            SendEmail(model);
            _logger.LogTrace("Mail sent: {@model}", model);

            return _tenantContext.IsDefault ? RedirectToRoute(RouteNames.HomeGeneralContactConfirmation) : RedirectToRoute(RouteNames.HomeOrganizationContactConfirmation, new { Organization = _tenantContext.SiteContext.UrlSegmentValue });
        }

        [HttpGet("{organization:MatchingTenant}/contact-confirmation", Name = RouteNames.HomeOrganizationContactConfirmation)]
        [HttpGet("/contact-confirmation", Name = RouteNames.HomeGeneralContactConfirmation)]
        public IActionResult ContactConfirmation()
        {
            return View(ViewNames.Home.ContactConfirmation);
        }

        private void SendEmail(ContactViewModel model)
        {
            _contactEmailTask.Timeout = TimeSpan.FromMinutes(1);
            _contactEmailTask.EmailCultureInfo = CultureInfo.DefaultThreadCurrentUICulture;

            _contactEmailTask.ViewNames = new[] { null, ViewNames.Emails.ContactEmailTxt };
            _contactEmailTask.LogMessage = "Send contact form as email";
            _contactEmailTask.Model = (Form: model, TenantContext: _tenantContext);
            _queue.QueueTask(_contactEmailTask);
        }
    }
}