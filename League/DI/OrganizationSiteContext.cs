using System;
using System.Linq;
using System.Reflection.Metadata;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.DependencyInjection;
using TournamentManager.Data;

namespace League.DI
{
    /// <summary>
    /// Provides organization-specific, site-specific data, and organization-specific access to the repositories.
    /// Content is available per HttpRequest with dependency injection.
    /// </summary>
    public class OrganizationSiteContext : OrganizationContext,  IOrganizationSite
    {
        #region ** CTOR **
        /// <summary>
        /// CTOR for dependency injection.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public OrganizationSiteContext(IHttpContextAccessor httpContextAccessor, OrganizationContextResolver organizationContextResolver, OrganizationSiteList siteList) :
            this(httpContextAccessor?.HttpContext, organizationContextResolver, siteList)
        { }

        /// <summary>
        /// Mainly helper CTOR called by injection CTOR.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public OrganizationSiteContext(HttpContext httpContext, OrganizationContextResolver organizationContextResolver,
            OrganizationSiteList siteList) :
            this(ResolveOrganizationKey(httpContext, siteList), organizationContextResolver, siteList)
        {
            SetMostRecentOrganizationCookie(httpContext, base.OrganizationKey);
        }

        /// <summary>
        /// CTOR finally called by injection CTOR. Good to be used instantiating with DI.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public OrganizationSiteContext(string organizationKey, OrganizationContextResolver organizationContextResolver, OrganizationSiteList siteList)
        {
            SiteList = siteList;
            OrganizationContextResolver = organizationContextResolver;
            // copies the resolved OrganizationContext properties to this OrganizationSiteContext
            Resolve(this, organizationKey);
        }
        #endregion

        #region ** IOrganizationSite **
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
        /// <param name="siteList"><see cref="OrganizationSiteList"/></param>
        /// <returns>Returns the organization key, if found in the <see cref="OrganizationSiteList"/>, else null.</returns>
        public static string ResolveOrganizationKey(HttpContext httpContext, OrganizationSiteList siteList)
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
            foreach (var site in siteList)
            {
                if (!string.IsNullOrEmpty(site.HostName))
                {
                    if (site.HostName == host && (string.IsNullOrEmpty(site.UrlSegmentValue)))
                    {
                        return site.OrganizationKey;
                    }
                }
                else
                {
                    if (site.UrlSegmentValue == siteSegment)
                    {
                        return site.OrganizationKey;
                    }
                }
            }

            return null;
        }

        private static void SetMostRecentOrganizationCookie(HttpContext httpContext, string organizationKey)
        {
            if (httpContext == null || organizationKey == null) return;
            
            if (organizationKey == string.Empty) httpContext.Response.Cookies.Delete(CookieNames.MostRecentOrganization);

            httpContext.Response.Cookies.Append(
                CookieNames.MostRecentOrganization,
                organizationKey,
                new CookieOptions()
                {
                    Path = "/",
                    HttpOnly = false,
                    Secure = false,
                    Expires = DateTimeOffset.Now.AddYears(1),
                    IsEssential = true
                }
            );
        }

        /// <summary>
        /// Resolves the <see cref="OrganizationSiteContext"/> for the given organization key.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <returns>Returns the <see cref="OrganizationSiteContext"/> if the organization key can be resolved, else null.</returns>
        public OrganizationSiteContext Resolve(string organizationKey)
        {
            var osc = new OrganizationSiteContext(organizationKey, OrganizationContextResolver, SiteList);
            return Resolve(osc, organizationKey);
        }
        
        private OrganizationSiteContext Resolve(OrganizationSiteContext organizationSiteContext, string organizationKey)
        {
            var siteSettings = SiteList.FirstOrDefault(sl => sl.OrganizationKey == (organizationKey ?? string.Empty));
            if (siteSettings == null) return null;

            OrganizationContextResolver.CopyContextTo(organizationSiteContext, organizationKey); // shallow-copy all OrganizationContext properties
            organizationSiteContext.HostName = siteSettings.HostName;
            organizationSiteContext.UrlSegmentValue = siteSettings.UrlSegmentValue;
            organizationSiteContext.FolderName = siteSettings.FolderName;
            organizationSiteContext.IdentityCookieName = siteSettings.IdentityCookieName;
            organizationSiteContext.SessionName = siteSettings.SessionName;
            organizationSiteContext.HideInMenu = siteSettings.HideInMenu;

            return organizationSiteContext;
        }

        /// <summary>
        /// Gets the <see cref="OrganizationSiteList"/>.
        /// </summary>
        public OrganizationSiteList SiteList { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrganizationContextResolver"/>.
        /// </summary>
        public OrganizationContextResolver OrganizationContextResolver { get; private set; }
    }
}