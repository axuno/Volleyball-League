using System;
using System.Linq;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using TournamentManager.Data;
using TournamentManager.MultiTenancy;
using OrganizationContext = TournamentManager.Data.OrganizationContext;

namespace League.DI
{
    /// <summary>
    /// Provides organization-specific, site-specific data, and organization-specific access to the repositories.
    /// Content is available per HttpRequest with dependency injection.
    /// </summary>
    [Obsolete("Implement with ITenantContext instead.")]
    public class SiteContext : OrganizationContext, ISite
    {
        #region ** CTOR **
        /// <summary>
        /// CTOR for dependency injection.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="tenantStore"></param>
        public SiteContext(IHttpContextAccessor httpContextAccessor, OrganizationContextResolver organizationContextResolver, TenantStore tenantStore) :
            this(httpContextAccessor?.HttpContext, organizationContextResolver, tenantStore)
        { }

        /// <summary>
        /// Mainly helper CTOR called by injection CTOR.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="tenantStore"></param>
        public SiteContext(HttpContext httpContext, OrganizationContextResolver organizationContextResolver,
            TenantStore tenantStore) :
            this(ResolveOrganizationKey(httpContext, tenantStore), organizationContextResolver, tenantStore)
        { }

        /// <summary>
        /// CTOR finally called by injection CTOR. Good to be used instantiating with DI.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="tenantStore"></param>
        public SiteContext(string organizationKey, OrganizationContextResolver organizationContextResolver, TenantStore tenantStore)
        {
            TenantStore = tenantStore;
            OrganizationContextResolver = organizationContextResolver;
            // copies the resolved OrganizationContext properties to this siteContext
            Resolve(this, organizationKey);
        }
        #endregion

        #region ** ISite **
        public string HostName { get; set; }
        public string UrlSegmentValue { get; set; }
        public string FolderName { get; set; }
        public string IdentityCookieName { get; set; }
        public string SessionName { get; set; }
        public bool HideInMenu { get; set; }
        #endregion

        /// <summary>
        /// Resolves the organization key from the <see cref="HttpRequest"/>.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="tenantStore"><see cref="TournamentManager.MultiTenancy.TenantStore"/></param>
        /// <returns>Returns the organization key, if found in the <see cref="TenantStore"/>, else null.</returns>
        public static string ResolveOrganizationKey(HttpContext httpContext, TenantStore tenantStore)
        {
            if (httpContext == null) return null;

            var request = httpContext.Request;
            // used if site is identified by host name (and port)
            var host = request.Host.ToString();
            // first segment:  /
            // second segment: organization/
            // third segment:  account/
            var uri = new Uri(request.GetDisplayUrl());
            // used if site is identified by the first segment of the URI
            var siteSegment = uri.Segments.Length > 1 ? uri.Segments[1].TrimEnd('/') : string.Empty;
            foreach (var tenant in tenantStore.GetTenants().Values)
            {
                if (!string.IsNullOrEmpty(tenant.SiteContext.HostName))
                {
                    if (tenant.SiteContext.HostName.Equals(host, StringComparison.InvariantCultureIgnoreCase) && (string.IsNullOrEmpty(tenant.SiteContext.UrlSegmentValue)))
                    {
                        return tenant.Identifier;
                    }
                }
                else
                {
                    if (tenant.SiteContext.UrlSegmentValue.Equals(siteSegment, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return tenant.Identifier;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves the <see cref="SiteContext"/> for the given organization key.
        /// </summary>
        /// <param name="tenantIdentifier"></param>
        /// <returns>Returns the <see cref="SiteContext"/> if the organization key can be resolved, else null.</returns>
        public SiteContext Resolve(string tenantIdentifier)
        {
            var osc = new SiteContext(tenantIdentifier, OrganizationContextResolver, TenantStore);
            return Resolve(osc, tenantIdentifier);
        }
        
        private SiteContext Resolve(SiteContext siteContext, string tenantIdentifier)
        {
            var tenantContext = TenantStore.GetTenantByIdentifier(tenantIdentifier ?? string.Empty);
            if (tenantContext == null) return null;

            OrganizationContextResolver.CopyContextTo(siteContext, tenantIdentifier); // shallow-copy all OrganizationContext properties
            siteContext.HostName = tenantContext.SiteContext.HostName;
            siteContext.UrlSegmentValue = tenantContext.SiteContext.UrlSegmentValue;
            siteContext.FolderName = tenantContext.SiteContext.FolderName;
            siteContext.IdentityCookieName = tenantContext.SiteContext.IdentityCookieName;
            siteContext.SessionName = tenantContext.SiteContext.SessionName;
            siteContext.HideInMenu = tenantContext.SiteContext.HideInMenu;

            return siteContext;
        }

        /// <summary>
        /// Gets the <see cref="TenantStore"/>.
        /// </summary>
        public TenantStore TenantStore { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrganizationContextResolver"/>.
        /// </summary>
        public OrganizationContextResolver OrganizationContextResolver { get; private set; }
    }
}