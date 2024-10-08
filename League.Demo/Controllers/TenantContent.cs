using League.Routing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using TournamentManager.MultiTenancy;

namespace League.WebApp.Controllers;

/// <summary>
/// This controller is able to return any tenant-specific content from <see cref="Scriban"/> templates.
/// </summary>
[Route(TenantRouteConstraint.Template)]
public class TenantContent : League.Controllers.AbstractController
{
    // Note:
    // We cannot search a view by 'File.Exists' because this won't work with precompiled views.
    // ViewEngine.FindView will get
    // -> precompiled views
    // -> view files (.cshtml) in case razor runtime compilation is on
    // -> localized view files (e.g.: .de.cshtml) depending on request culture
    // -> view files could also come from other providers, not only PhysicalFileProvider
    private readonly ICompositeViewEngine _viewEngine;
    private readonly ActionContext _actionContext;
    private readonly ITenantContext _tenantContext;

    public TenantContent(ICompositeViewEngine viewEngine, IActionContextAccessor actionContextAccessor, ITenantContext tenantContext)
    {
        _viewEngine = viewEngine;
        _actionContext = actionContextAccessor.ActionContext!;
        _tenantContext = tenantContext;
    }

    [Route("{category=home}/{topic=index}")]
    [HttpGet]
    public IActionResult Index(string category = "home", string topic = "index")
    {
        if (!ModelState.IsValid) return BadRequest();

        // Note: Indicate the current controller-specific tenant directory with the "./" prefix
        var view = $"./{_tenantContext.SiteContext.FolderName}/{category}_{topic}";
        var result = _viewEngine.FindView(_actionContext, view, false);
        if(!result.Success) return NotFound();
            
        // give the full absolute view path as argument, so searching the view won't run a second time
        return View(result.View.Path);
    }
}
