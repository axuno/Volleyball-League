using System.Diagnostics.CodeAnalysis;
using League.Routing;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

/// <summary>
/// This controller returns tenant-specific content.
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

    [Route("{topic=index}")]
    [HttpGet]
    [SuppressMessage("Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Parameters are validated and sanitized.")]
    public async Task<IActionResult> Index(string topic = "index")
    {
        if (!ModelState.IsValid) return BadRequest();

        // Validate and sanitize the category and topic parameters
        if (string.IsNullOrWhiteSpace(topic) ||
            topic.Contains("..") ||
            topic.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            _logger.LogWarning("Invalid topic: {Topic}", topic);
            return BadRequest("Invalid topic.");
        }

        var path = Path.Combine(_webHostEnvironment.WebRootPath, "tenant-content", _tenantContext.SiteContext.FolderName, $"{topic}.html");

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
                _logger.LogError(ex, "Error reading tenant content file: {Path}", path);
                return NotFound();
            }
        }
    }
}
