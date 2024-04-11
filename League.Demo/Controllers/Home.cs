using League.Views;
using Microsoft.AspNetCore.Mvc;
using TournamentManager.MultiTenancy;

namespace League.WebApp.Controllers;

[Route("")]
public class Home : League.Controllers.AbstractController
{
    private readonly TenantStore _tenantStore;

    public Home(TenantStore tenantStore)
    {
        _tenantStore = tenantStore;
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
        return View("Welcome");
    }

    [Route("/about-league")]
    [HttpGet]
    public IActionResult AboutLeague()
    {
        return View("AboutLeague");
    }

    [Route("/[action]")]
    [HttpGet]
    public IActionResult Overview()
    {
        return View("Overview");
    }

    [Route("/legal-disclosure")]
    [HttpGet]
    public IActionResult LegalDisclosure()
    {
        return View("LegalDisclosure");
    }

    [Route("/[action]")]
    [HttpGet]
    public IActionResult Privacy()
    {
        return View("Privacy");
    }

    [Route("/picture-credits")]
    [HttpGet]
    public IActionResult PictureCredits()
    {
        return View("PictureCredits");
    }
}
