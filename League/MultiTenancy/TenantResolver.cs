using System;
using System.Collections.Generic;
using System.Linq;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy
{
    /// <summary>
    /// Resolves a tenant using the <see cref="TenantStore"/> and the <see cref="HttpContext"/> using the Base Path Strategy (i.e. the first path segment).
    /// If a tenant could be resolved, a cookie named <see cref="CookieNames.MostRecentTenant"/> is added to the <see cref="HttpContext"/>.
    /// </summary>
    public class TenantResolver
    {
        private readonly IReadOnlyDictionary<string, TenantContext> _tenants;
        private readonly HttpContext _httpContext;
        
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="tenantStore"></param>
        /// <param name="httpContextAccessor"></param>
        public TenantResolver(TenantStore tenantStore, IHttpContextAccessor httpContextAccessor)
        {
            _tenants = tenantStore.GetTenants();
            _httpContext = httpContextAccessor.HttpContext;
        }
        
        /// <summary>
        /// Resolves the tenant using the Base Path Strategy (i.e. the first path segment).
        /// </summary>
        /// <returns>Returns the resolved <see cref="ITenantContext"/> if a tenant was resolved, otherwise <see langword="null"/>.</returns>
        public ITenantContext Resolve()
        {
            if (_httpContext == null) return _tenants.First(t => t.Value.IsDefault).Value;

            var request = _httpContext.Request;
            // used if site is identified by host name (and port)
            var host = request.Host.ToString();
            // first segment:  /
            // second segment: organization/
            // third segment:  account/
            var uri = new Uri(request.GetDisplayUrl());
            // used if site is identified by the first segment of the URI
            var siteSegment = uri.Segments.Length > 1 ? uri.Segments[1].TrimEnd('/') : string.Empty;
            foreach (var tenant in _tenants.Values)
            {
                if (!string.IsNullOrEmpty(tenant.SiteContext.UrlSegmentValue) && tenant.SiteContext.UrlSegmentValue == siteSegment)
                {
                    SetMostRecentTenantCookie(tenant);
                    return tenant;
                }
            }

            return _tenants.Values.FirstOrDefault(t => t.IsDefault);
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
                    Expires = DateTimeOffset.Now.AddYears(1),
                    IsEssential = true
                }
            );
        }
    }
}
