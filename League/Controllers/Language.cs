using System;
using System.Linq;
using System.Net;
using System.Runtime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace League.Controllers
{
    [Route("[controller]")]
    public class Language : Controller
    {
        private readonly RequestLocalizationOptions _requestLocalizationOptions;
        private readonly ILogger<Language> _logger;

        public Language(IOptions<RequestLocalizationOptions> requestLocalizationOptions, ILogger<Language> logger)
        {
            _requestLocalizationOptions = requestLocalizationOptions.Value;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index(string culture, string uiCulture, string returnUrl)
        {
            // QueryStringRequestCultureProvider processes arguments and requires one of both
            if (culture == null && uiCulture == null)
            {
                _logger.LogDebug($"{nameof(Language)} controller was invoked without {nameof(culture)} or {nameof(uiCulture)} query string");
                return RedirectToLocal(returnUrl);
            }

            // get languages from header submitted by the browser
            var languages = Request.GetTypedHeaders()
                       .AcceptLanguage
                       .OrderByDescending(x => x.Quality ?? 1) // Quality defines priority from 0 to 1, where 1 is the highest.
                       .Select(x => x.Value.ToString())
                       .ToArray();

            // The culture for the current thread was already set by QueryStringRequestCultureProvider
            var requestCulture = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.Culture;
            var requestUiCulture = HttpContext.Features.Get<IRequestCultureFeature>()?.RequestCulture.UICulture;

            var cookieProvider = _requestLocalizationOptions.RequestCultureProviders
                    .OfType<CookieRequestCultureProvider>()
                    .FirstOrDefault();

            if (requestCulture == null || cookieProvider == null)
            {
                _logger.LogError("Request culture or cookie provider are null");
                return RedirectToLocal(returnUrl);
            }

            // Preferred browser language is equal to language selected with query string
            if (languages.Length == 0 || languages.First().StartsWith(requestCulture.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                Response.Cookies.Delete(cookieProvider.CookieName);
                return RedirectToLocal(returnUrl);
            }

            // set the cookie for future language detection
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(requestCulture, requestUiCulture));
            Response.Cookies.Append(cookieProvider.CookieName,
                cookieValue,
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1), IsEssential = true });

            return RedirectToLocal(returnUrl);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            returnUrl = WebUtility.UrlDecode(returnUrl);
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return Redirect("/");
        }
    }
}