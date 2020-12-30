using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using TournamentManager.MultiTenancy;

namespace League.Identity
{
    public class LeagueCookieManager : ICookieManager
    {
        #region Private Members

        private readonly ICookieManager _concreteManager;
 
        #endregion

        public LeagueCookieManager()
        {
            _concreteManager = new ChunkingCookieManager();
        }

        #region Public Methods

        public void AppendResponseCookie(HttpContext context, string key, string value, CookieOptions options)
        {
            var siteContext = context.RequestServices.GetRequiredService<ITenantContext>();
            _concreteManager.AppendResponseCookie(context, $"{key}{siteContext.SiteContext.IdentityCookieName}", value, options);
        }

        public void DeleteCookie(HttpContext context, string key, CookieOptions options)
        {
            var siteContext = context.RequestServices.GetRequiredService<ITenantContext>();
            _concreteManager.DeleteCookie(context, $"{key}{siteContext.SiteContext.IdentityCookieName}", options);
        }

        public string GetRequestCookie(HttpContext context, string key)
        {
            var siteContext = context.RequestServices.GetRequiredService<ITenantContext>();
            return _concreteManager.GetRequestCookie(context, $"{key}{siteContext.SiteContext.IdentityCookieName}");
        }

        #endregion
    }
}
