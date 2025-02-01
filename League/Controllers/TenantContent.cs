using System.Diagnostics.CodeAnalysis;
using League.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

/// <summary>
/// This controller is able to return any tenant-specific content from <see cref="Scriban"/> templates.
/// </summary>
[Route(TenantRouteConstraint.Template)]
public class TenantContent : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<TenantContent> _logger;

    public TenantContent(IWebHostEnvironment webHostEnvironment, ITenantContext tenantContext, ILogger<TenantContent> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _tenantContext = tenantContext;
        _logger = logger;
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
            _logger.LogWarning("Invalid category or topic: {category}, {topic}", category, topic);
            return BadRequest("Invalid category or topic.");
        }

        var path = Path.Combine(_webHostEnvironment.WebRootPath, "tenant-content", _tenantContext.SiteContext.FolderName, $"{category}_{topic}.html");

        if (!System.IO.File.Exists(path)) return NotFound();
        try
        {
            var html = await System.IO.File.ReadAllTextAsync(path);
            return View(nameof(Index), html);
        }
        catch
        {
            try
            {
                await Task.Delay(1000);
                
                var html = await System.IO.File.ReadAllTextAsync(path);
                return View(nameof(Index), html);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error reading tenant content file: {path}", path);
                return NotFound();
            }
        }
    }
}
