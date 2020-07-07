using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using Axuno.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace League.Controllers
{
    /// <summary>
    /// Captcha Controller
    /// </summary>
    [Route("captcha")]
    public class Captcha : AbstractController
	{
		public async Task<IActionResult> Index()
        {
            return await GetSvgContent();
        }

        private Task<ContentResult> GetSvgContent()
        {
            using var ci = new CaptchaSvgGenerator(null, 151, 51, Color.FromArgb(0x01, 0x8D, 0xFF),
                Color.FromArgb(0, 255, 255, 255), Color.FromArgb(0x01, 0x8D, 0xFF));

            var result = ci.SetTextWithMathCalc(5).ToString(); // GenerateRandomString(5)
            HttpContext.Session.SetString(CaptchaSessionKeyName, result);

            // Change the response headers to output an un-cached image.
            HttpContext.Response.Clear();
            HttpContext.Response.Headers.Add("Expires", DateTime.UtcNow.Date.AddDays(-1).ToString("R"));
            HttpContext.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
            HttpContext.Response.Headers.Add("Pragma", "no-cache");

            HttpContext.Response.ContentType = "image/svg+xml";
            return Task.FromResult(Content(ci.Image));
        }

	    private string CaptchaSessionKeyName => CaptchaSvgGenerator.CaptchaSessionKeyName;
	}
}
