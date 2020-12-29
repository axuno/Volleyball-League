using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Configuration;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy
{
    public class TenantResolver
    {
        private readonly IReadOnlyDictionary<string, TenantContext> _tenants;
        private readonly HttpContext _httpContext;
        
        public TenantResolver(TenantStore tenantStore, IHttpContextAccessor httpContextAccessor)
        {
            _tenants = tenantStore.GetTenants();
            _httpContext = httpContextAccessor.HttpContext;
        }
        
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
                if (!string.IsNullOrEmpty(tenant.SiteContext.HostName) && tenant.SiteContext.HostName == host)
                {
                    SetMostRecentOrganizationCookie(tenant);
                    return tenant;
                }
                if (!string.IsNullOrEmpty(tenant.SiteContext.UrlSegmentValue) && tenant.SiteContext.UrlSegmentValue == siteSegment)
                {
                    SetMostRecentOrganizationCookie(tenant);
                    return tenant;
                }
            }

            return null;
        }
        
        private void SetMostRecentOrganizationCookie(ITenantContext tenant)
        {
            if (_httpContext == null) return;
            
            if (tenant.SiteContext.UrlSegmentValue == string.Empty) _httpContext.Response.Cookies.Delete(CookieNames.MostRecentOrganization);

            _httpContext.Response.Cookies.Append(
                CookieNames.MostRecentOrganization,
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
