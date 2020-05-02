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
            // Create a CAPTCHA image
		    var stream = new MemoryStream();

            await Task.Run(() =>
            {
                var ci = new CaptchaImageGenerator(null, 150, 50, "Arial", Color.FromArgb(0x01, 0x8D, 0xFF),
                    Color.White, Color.FromArgb(0x01, 0x8D, 0xFF)) {AddNoise = true, WarpText = false};

                var result = ci.SetTextWithMathCalc(5).ToString(); // GenerateRandomString(5)
                HttpContext.Session.SetString(CaptchaSessionKeyName, result);

                // Change the response headers to output an un-cached image.
                HttpContext.Response.Clear();
                HttpContext.Response.Headers.Add("Expires", DateTime.UtcNow.Date.AddDays(-1).ToString("R"));
		        HttpContext.Response.Headers.Add("Cache-Control", "no-store, no-cache, must-revalidate");
		        HttpContext.Response.Headers.Add("Pragma", "no-cache");

                HttpContext.Response.ContentType = "image/jpeg";
                
                // Write the image to the response stream in JPEG format.
                ci.Image.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
		        // Dispose the CAPTCHA image object.
		        ci.Dispose();
            });
		    stream.Position = 0;
            return new FileStreamResult(stream, "image/jpeg");
        }

	    private string CaptchaSessionKeyName => CaptchaImageGenerator.CaptchaSessionKeyName;
	}
}
