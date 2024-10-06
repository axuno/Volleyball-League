using System.Security.Claims;
using Axuno.Web;
using League.BackgroundTasks;
using League.Models.AccountViewModels;
using League.Models.ContactViewModels;
using League.Routing;
using League.Views;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Route("[controller]")]
public class Contact : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IStringLocalizer<Account> _localizer;
    private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
    private readonly SendEmailTask _sendEmailTask;
    private readonly ILogger<Contact> _logger;

    public Contact(ITenantContext tenantContext, Axuno.BackgroundTask.IBackgroundQueue queue, SendEmailTask sendEmailTask, ILogger<Contact> logger, IStringLocalizer<Account> localizer)
    {
        _tenantContext = tenantContext;
        _queue = queue;
        _sendEmailTask = sendEmailTask;
        _logger = logger;            
        _localizer = localizer;
        _tenantContext = tenantContext;
    }
        
    [HttpGet("/" + TenantRouteConstraint.Template + "/[controller]", Name = RouteNames.TenantContact)]
    [HttpGet("", Name = RouteNames.GeneralContact)]
    public IActionResult Index()
    {
        var model = new ContactViewModel();
        if (User.Identity is { IsAuthenticated: true })
        {
            model.Email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value ?? string.Empty;
            model.Gender = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Gender)?.Value ?? string.Empty;
            model.FirstName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value ?? string.Empty;
            model.LastName = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Surname)?.Value ?? string.Empty;
        }
            
        // Note: The form will use route name to generate the action url
        // Also, if we are in an tenant context, the tenant organization contact url will be used, otherwise the general one
        return View(ViewNames.Contact.Index, model);
    }

    [HttpPost("/" + TenantRouteConstraint.Template + "/[controller]", Name = RouteNames.TenantContact)]
    [HttpPost("", Name = RouteNames.GeneralContact)]
    [ValidateAntiForgeryToken]
    public IActionResult Index(ContactViewModel model)
    {
        if (model.Captcha != HttpContext.Session.GetString(CaptchaSvgGenerator.CaptchaSessionKeyName))
        {
            ModelState.AddModelError(nameof(CreateAccountViewModel.Captcha), _localizer["Math task result was incorrect"]);
        }
        if (!ModelState.IsValid)
        {
            return View(ViewNames.Contact.Index, model);
        }

        SendEmail(model);
        _logger.LogDebug("Mail sent: {Model}", model);

        return _tenantContext.IsDefault
            ? RedirectToRoute(RouteNames.GeneralContactConfirmation)
            : RedirectToRoute(RouteNames.TenantContactConfirmation,
                new RouteValueDictionary { { TenantRouteConstraint.Key, _tenantContext.SiteContext.UrlSegmentValue } });
    }

    [HttpGet("/" + TenantRouteConstraint.Template + "/contact-confirmation", Name = RouteNames.TenantContactConfirmation)]
    [HttpGet("/contact-confirmation", Name = RouteNames.GeneralContactConfirmation)]
    public IActionResult ContactConfirmation()
    {
        return View(ViewNames.Contact.ContactConfirmation);
    }

    private void SendEmail(ContactViewModel model)
    {
        _sendEmailTask.SetMessageCreator(new Emailing.Creators.ContactFormCreator
        {
            Parameters =
            {
                ContactForm = model,
                CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture
            }
        });

        _queue.QueueTask(_sendEmailTask);
    }
}
