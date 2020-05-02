using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace League.Controllers
{
    public abstract class AbstractController : Controller
    {
        /// <summary>
        /// Returns a JSON result which will be evaluated in the JavaScript for a
        /// Bootstrap modal form in order to redirect the browser to the target URL.
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <returns>returns { "redirectUrl": "value-of-variable-redirectUrl" }</returns>
        /// <example>
        /// return JsonForModalAjaxRedirect(Url.Action("Index", "Home", Request.Scheme));
        /// </example>
        [NonAction]
        protected JsonResult JsonAjaxRedirectForModal(string redirectUrl)
        {
            var data = new { redirectUrl };
            return Json(data);
        }

        /// <summary>
        /// Gets the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> of the current user
        /// as <see langword="long"/> integer or. The controller has attribute [Authorize], so user must be set.
        /// </summary>
        /// <returns>Gets the <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/> of the current user
        /// as <see langword="long"/> integer.</returns>
        [NonAction]
        protected long GetCurrentUserId()
        {
            return long.Parse(User.Claims.First(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier).Value);
        }
    }
}
