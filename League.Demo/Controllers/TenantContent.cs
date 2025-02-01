using System.Diagnostics.CodeAnalysis;
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
    [SuppressMessage("Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Parameters are validated and sanitized.")]
    public async Task<IActionResult> Index(string category = "home", string topic = "index")
    {
        if (!ModelState.IsValid) return BadRequest();

        // Validate and sanitize the category and topic parameters
        if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(topic) ||
            category.Contains("..") || topic.Contains("..") ||
            category.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
            topic.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            return BadRequest("Invalid category or topic.");
        }

        var webHostEnvironment = (IWebHostEnvironment)_actionContext.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment))!;
        var path = Path.Combine(webHostEnvironment.WebRootPath, "tenant-content", _tenantContext.SiteContext.FolderName, $"{category}_{topic}.html");

        if (!System.IO.File.Exists(path)) return NotFound();
        try
        {
            var html = await System.IO.File.ReadAllTextAsync(path);
            return View("Index", html);
        }
        catch (Exception e)
        {
            try
            {
                await Task.Delay(1000);
                
                var html = await System.IO.File.ReadAllTextAsync(path);
                return View("Index", html);
            }
            catch (Exception exception)
            {
                return NotFound();
            }
        }
    }
}
