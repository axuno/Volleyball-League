using System;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy;

/// <summary>
/// Resolves a tenant using the <see cref="TenantStore"/> and the <see cref="HttpContext"/> using the Base Path Strategy (i.e. the first path segment).
/// If a tenant could be resolved, a cookie named <see cref="CookieNames.MostRecentTenant"/> is added to the <see cref="HttpContext"/>.
/// </summary>
public class TenantResolver
{
    private readonly TenantStore _tenantStore;
    private readonly HttpContext? _httpContext;
        
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="tenantStore"></param>
    /// <param name="httpContextAccessor"></param>
    public TenantResolver(TenantStore tenantStore, IHttpContextAccessor httpContextAccessor)
    {
        _tenantStore = tenantStore;
        _httpContext = httpContextAccessor.HttpContext;
    }
        
    /// <summary>
    /// Resolves the tenant using the Base Path Strategy (i.e. the first path segment).
    /// </summary>
    /// <returns>Returns the resolved <see cref="ITenantContext"/> if a tenant was resolved, otherwise <see langword="null"/>.</returns>
    public ITenantContext Resolve()
    {
        if (_httpContext == null) return _tenantStore.GetDefaultTenant() ?? throw new InvalidOperationException("HttpContext is null and no default ITenantContext found.");

        var request = _httpContext.Request;
        // first segment:  /
        // second segment: organization/
        // third segment:  account/
        var uri = new Uri(request.GetDisplayUrl());
        // used if site is identified by the first segment of the URI
        var urlSegmentValue = uri.Segments.Length > 1 ? uri.Segments[1].TrimEnd('/') : string.Empty;
        var tenant = _tenantStore.GetTenantByUrlSegment(urlSegmentValue);
        if (tenant != null)
        {
            SetMostRecentTenantCookie(tenant);
            return tenant;
        }
            
        return _tenantStore.GetDefaultTenant() ?? throw new InvalidOperationException("No default ITenantContext found.");
    }
        
    private void SetMostRecentTenantCookie(ITenantContext tenant)
    {
        if (_httpContext == null) return;
            
        if (tenant.SiteContext.UrlSegmentValue == string.Empty) _httpContext.Response.Cookies.Delete(CookieNames.MostRecentTenant);

        _httpContext.Response.Cookies.Append(
            CookieNames.MostRecentTenant,
            tenant.SiteContext.UrlSegmentValue,
            new CookieOptions
            {
                Path = "/",
                HttpOnly = false,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.Now.AddYears(1),
                IsEssential = true
            }
        );
    }
}
