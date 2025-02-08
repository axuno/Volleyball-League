using System.Diagnostics.CodeAnalysis;
using League.Helpers;
using League.MultiTenancy;
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
    private readonly ITenantContentProvider _contentProvider;
    private readonly ILogger<TenantContent> _logger;

    public TenantContent(ITenantContext tenantContext, ITenantContentProvider contentProvider, ILogger<TenantContent> logger)
    {
        _tenantContext = tenantContext;
        _contentProvider = contentProvider;
        _logger = logger;
    }

    [Route("")]
    [HttpGet]
    public async Task<IActionResult> Home()
    {
        return await GetView("home");
    }

    [Route("{topic}")]
    [HttpGet]
    [SuppressMessage("Security", "CA3003:Review code for file path injection vulnerabilities", Justification = "Parameters are validated and sanitized.")]
    public async Task<IActionResult> Home(string topic)
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

        return await GetView(topic);
    }

    private async Task<IActionResult> GetView(string topic)
    {
        var contentItem = await _contentProvider.GetContentItem(_tenantContext, topic);
        string? html = null;

        if (contentItem is { ReadHtmlContentAsync: not null })
        {
            const int maxAttempts = 3;
            const int delay = 250;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                try
                {
                    html = await contentItem.ReadHtmlContentAsync();
                    break; // Exit the loop if the read is successful
                }
                catch (Exception ex)
                {
                    // Might occur if the file is locked by another process, e.g. when it is being written to.
                    _logger.LogError(ex, "Error reading HTML content for topic: {Topic}, attempt: {Attempt}", topic, attempt + 1);
                    if (attempt < maxAttempts - 1)
                    {
                        await Task.Delay(delay);
                    }
                }
            }
        }

        if (html == null) return NotFound();

        ViewData.Title(contentItem!.PageTitel ?? _tenantContext.OrganizationContext.ShortName);
        ViewData.Description(contentItem.Description ?? _tenantContext.OrganizationContext.Description);
        ViewData["LastModified"] = contentItem.LastModified.ToString("yyyy-MM-ddTHH:mm:ssK");
        ViewData["Date"] = contentItem.PubDate.ToString("yyyy-MM-ddTHH:mm:ssK");

        return View(nameof(Home), html);
    }
}
