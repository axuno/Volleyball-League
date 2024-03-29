﻿using League.MultiTenancy;

namespace League.Controllers;

/// <summary>
/// Abstract class for controllers, derived from <see cref="Controller"/>.
/// </summary>
public abstract class AbstractController : Controller
{
    private TenantLink? _tenantLink;
    private LinkGenerator? _generalLink;

    /// <summary>
    /// Returns a JSON result which will be evaluated in the JavaScript for a
    /// Bootstrap modal form in order to redirect the browser to the target URL.
    /// </summary>
    /// <param name="redirectUrl"></param>
    /// <returns>returns { "redirectUrl": "value-of-variable-redirectUrl" }</returns>
    /// <example>
    /// return JsonResponseRedirect(Url.Action("Index", "Home", Request.Scheme));
    /// </example>
    [NonAction]
    protected JsonResult JsonResponseRedirect(string? redirectUrl)
    {
        var data = new { redirectUrl };
        return Json(data);
    }

    /// <summary>
    /// Gets the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> of the current user
    /// as <see langword="long"/> integer or. The controller has attribute [Authorize], so user must be set.
    /// </summary>
    /// <returns>Gets the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> of the current user
    /// as <see langword="long"/> integer.</returns>
    [NonAction]
    protected long? GetCurrentUserId()
    {
        var userId = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userId, out var id) ? id : null;
    }

    /// <summary>
    /// Gets the <see cref="MultiTenancy.TenantLink" /> generator.
    /// </summary>
    protected TenantLink TenantLink
    {
        get
        {
            _tenantLink ??= HttpContext.RequestServices.GetRequiredService<TenantLink>();
            return _tenantLink;
        }
    }

    /// <summary>
    /// Gets the <see cref="LinkGenerator" /> from the AspNetCore.Routing namespace.
    /// </summary>
    protected LinkGenerator GeneralLink
    {
        get
        {
            _generalLink ??= TenantLink.LinkGenerator;
            return _generalLink;
        }
    }
}
