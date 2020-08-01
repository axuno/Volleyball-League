using System;
using System.Linq;
using League.Views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using TournamentManager.Data;

namespace League.DI
{
    /// <summary>
    /// Provides organization-specific, site-specific data, and organization-specific access to the repositories.
    /// Content is available per HttpRequest with dependency injection.
    /// </summary>
    public class SiteContext : OrganizationContext,  ISite
    {
        #region ** CTOR **
        /// <summary>
        /// CTOR for dependency injection.
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public SiteContext(IHttpContextAccessor httpContextAccessor, OrganizationContextResolver organizationContextResolver, SiteList siteList) :
            this(httpContextAccessor?.HttpContext, organizationContextResolver, siteList)
        { }

        /// <summary>
        /// Mainly helper CTOR called by injection CTOR.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public SiteContext(HttpContext httpContext, OrganizationContextResolver organizationContextResolver,
            SiteList siteList) :
            this(ResolveOrganizationKey(httpContext, siteList), organizationContextResolver, siteList)
        {
            SetMostRecentOrganizationCookie(httpContext, UrlSegmentValue);
        }

        /// <summary>
        /// CTOR finally called by injection CTOR. Good to be used instantiating with DI.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <param name="organizationContextResolver"></param>
        /// <param name="siteList"></param>
        public SiteContext(string organizationKey, OrganizationContextResolver organizationContextResolver, SiteList siteList)
        {
            SiteList = siteList;
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
        /// <param name="siteList"><see cref="SiteList"/></param>
        /// <returns>Returns the organization key, if found in the <see cref="SiteList"/>, else null.</returns>
        public static string ResolveOrganizationKey(HttpContext httpContext, SiteList siteList)
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

        private static void SetMostRecentOrganizationCookie(HttpContext httpContext, string urlSegmentValue)
        {
            if (httpContext == null || urlSegmentValue == null) return;
            
            if (urlSegmentValue == string.Empty) httpContext.Response.Cookies.Delete(CookieNames.MostRecentOrganization);

            httpContext.Response.Cookies.Append(
                CookieNames.MostRecentOrganization,
                urlSegmentValue,
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
        /// Resolves the <see cref="SiteContext"/> for the given organization key.
        /// </summary>
        /// <param name="organizationKey"></param>
        /// <returns>Returns the <see cref="SiteContext"/> if the organization key can be resolved, else null.</returns>
        public SiteContext Resolve(string organizationKey)
        {
            var osc = new SiteContext(organizationKey, OrganizationContextResolver, SiteList);
            return Resolve(osc, organizationKey);
        }
        
        private SiteContext Resolve(SiteContext siteContext, string organizationKey)
        {
            var siteSettings = SiteList.FirstOrDefault(sl => sl.OrganizationKey == (organizationKey ?? string.Empty));
            if (siteSettings == null) return null;

            OrganizationContextResolver.CopyContextTo(siteContext, organizationKey); // shallow-copy all OrganizationContext properties
            siteContext.HostName = siteSettings.HostName;
            siteContext.UrlSegmentValue = siteSettings.UrlSegmentValue;
            siteContext.FolderName = siteSettings.FolderName;
            siteContext.IdentityCookieName = siteSettings.IdentityCookieName;
            siteContext.SessionName = siteSettings.SessionName;
            siteContext.HideInMenu = siteSettings.HideInMenu;

            return siteContext;
        }

        /// <summary>
        /// Gets the <see cref="SiteList"/>.
        /// </summary>
        public SiteList SiteList { get; private set; }

        /// <summary>
        /// Gets the <see cref="OrganizationContextResolver"/>.
        /// </summary>
        public OrganizationContextResolver OrganizationContextResolver { get; private set; }
    }
}