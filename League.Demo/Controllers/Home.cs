using System.Linq;
using League.Views;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.WebApp.Controllers;

[Route("")]
public class Home : League.Controllers.AbstractController
{
    private readonly TenantStore _tenantStore;
    private readonly ILogger<Home> _logger;

    public Home(TenantStore tenantStore, ILogger<Home> logger)
    {
        _tenantStore = tenantStore;
        _logger = logger;
    }

    [Route("")]
    [HttpGet]
    public IActionResult Index()
    {
        if (Request.Cookies.TryGetValue(CookieNames.MostRecentTenant, out var urlSegmentValue) && _tenantStore.GetTenants().Any(sl => sl.Value.SiteContext.UrlSegmentValue == urlSegmentValue))
        {
            if (!string.IsNullOrEmpty(urlSegmentValue)) return Redirect($"/{urlSegmentValue}");
        }

        return Redirect(Url.Action(nameof(Welcome), nameof(Home))!);
    }

    [Route("/[action]")]
    [HttpGet]
    public IActionResult Welcome()
    {
        return View(ViewNames.Home.Welcome);
    }

    [Route("/about-league")]
    [HttpGet]
    public IActionResult AboutLeague()
    {
        return View(ViewNames.Home.AboutLeague);
    }

    [Route("/[action]")]
    [HttpGet]
    public IActionResult Overview()
    {
        return View(ViewNames.Home.Overview);
    }

    [Route("/legal-disclosure")]
    [HttpGet]
    public IActionResult LegalDisclosure()
    {
        return View(ViewNames.Home.LegalDisclosure);
    }

    [Route("/[action]")]
    [HttpGet]
    public IActionResult Privacy()
    {
        return View(ViewNames.Home.Privacy);
    }

    [Route("/picture-credits")]
    [HttpGet]
    public IActionResult PictureCredits()
    {
        return View(ViewNames.Home.PictureCredits);
    }
}
