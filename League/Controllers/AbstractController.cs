using System;
using System.Linq;
using League.MultiTenancy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace League.Controllers;

/// <summary>
/// Abstract class for controllers, derived from <see cref="Controller"/>.
/// </summary>
public abstract class AbstractController : Controller
{
    private TenantUrlHelper? _tenantUrlHelper;

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
    protected JsonResult JsonAjaxRedirectForModal(string? redirectUrl)
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

    /// <summary>
    /// Gets or sets the <see cref="TenantUrlHelper"/>.
    /// </summary>
    protected TenantUrlHelper TenantUrl
    {
        get
        {
            _tenantUrlHelper ??= HttpContext.RequestServices.GetRequiredService<TenantUrlHelper>();
            return _tenantUrlHelper;
        }
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            _tenantUrlHelper = value;
        }
    }

    /// <summary>
    /// Creates a <see cref="T:Microsoft.AspNetCore.Mvc.LocalRedirectResult" /> object with <see cref="P:Microsoft.AspNetCore.Mvc.LocalRedirectResult.Permanent" /> set to
    /// true and <see cref="P:Microsoft.AspNetCore.Mvc.LocalRedirectResult.PreserveMethod" /> set to true
    /// (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status308PermanentRedirect" />) using the specified <paramref name="localUrl" />.
    /// </summary>
    /// <param name="localUrl">The local URL to redirect to.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.LocalRedirectResult" /> for the response.</returns>
    [NonAction]
    public override LocalRedirectResult LocalRedirectPermanentPreserveMethod(string localUrl) => !string.IsNullOrEmpty(localUrl) ? new LocalRedirectResult(localUrl, true, true) : throw new ArgumentException("Argument cannot be null or empty", nameof (localUrl));

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to an action with the same name as current one.
    /// The 'controller' and 'action' names are retrieved from the ambient values of the current request.
    /// </summary>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    /// <example>
    /// A POST request to an action named "Product" updates a product and redirects to an action, also named
    /// "Product", showing details of the updated product.
    /// <code>
    /// [HttpGet]
    /// public IActionResult Product(int id)
    /// {
    ///     var product = RetrieveProduct(id);
    ///     return View(product);
    /// }
    /// 
    /// [HttpPost]
    /// public IActionResult Product(int id, Product product)
    /// {
    ///     UpdateProduct(product);
    ///     return RedirectToAction();
    /// }
    /// </code>
    /// </example>
    [NonAction]
    public override RedirectToActionResult RedirectToAction() => RedirectToAction(null);

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the <paramref name="actionName" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(string? actionName) => RedirectToAction(actionName, (object) null);

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the
    /// <paramref name="actionName" /> and <paramref name="routeValues" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(string? actionName, object? routeValues) => RedirectToAction(actionName, (string) null, routeValues);

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the
    /// <paramref name="actionName" /> and the <paramref name="controllerName" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(string? actionName, string? controllerName) => RedirectToAction(actionName, controllerName, (object) null);

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the specified
    /// <paramref name="actionName" />, <paramref name="controllerName" />, and <paramref name="routeValues" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(
      string? actionName,
      string? controllerName,
      object? routeValues)
    {
      return RedirectToAction(actionName, controllerName, routeValues, (string) null);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the specified
    /// <paramref name="actionName" />, <paramref name="controllerName" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(
      string? actionName,
      string? controllerName,
      string? fragment)
    {
      return RedirectToAction(actionName, controllerName, (object) null, fragment);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status302Found" />) to the specified action using the specified <paramref name="actionName" />,
    /// <paramref name="controllerName" />, <paramref name="routeValues" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToAction(
      string? actionName,
      string? controllerName,
      object? routeValues,
      string? fragment)
    {
      return new RedirectToActionResult(actionName, controllerName, routeValues, fragment)
      {
        UrlHelper = Url
      };
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status307TemporaryRedirect" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to false and <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.PreserveMethod" />
    /// set to true, using the specified <paramref name="actionName" />, <paramref name="controllerName" />,
    /// <paramref name="routeValues" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The route data to use for generating the URL.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPreserveMethod(
      string? actionName = null,
      string? controllerName = null,
      object? routeValues = null,
      string? fragment = null)
    {
      return new RedirectToActionResult(actionName, controllerName, routeValues, false, true, fragment)
      {
        UrlHelper = Url
      };
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(string? actionName) => RedirectToActionPermanent(actionName, (object) null);

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />
    /// and <paramref name="routeValues" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(
      string? actionName,
      object? routeValues)
    {
      return RedirectToActionPermanent(actionName, (string) null, routeValues);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />
    /// and <paramref name="controllerName" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(
      string? actionName,
      string? controllerName)
    {
      return RedirectToActionPermanent(actionName, controllerName, (object) null);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />,
    /// <paramref name="controllerName" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(
      string? actionName,
      string? controllerName,
      string? fragment)
    {
      return RedirectToActionPermanent(actionName, controllerName, (object) null, fragment);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />,
    /// <paramref name="controllerName" />, and <paramref name="routeValues" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(
      string? actionName,
      string? controllerName,
      object? routeValues)
    {
      return RedirectToActionPermanent(actionName, controllerName, routeValues, (string) null);
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true using the specified <paramref name="actionName" />,
    /// <paramref name="controllerName" />, <paramref name="routeValues" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The parameters for a route.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanent(
      string? actionName,
      string? controllerName,
      object? routeValues,
      string? fragment)
    {
      return new RedirectToActionResult(actionName, controllerName, routeValues, true, fragment)
      {
        UrlHelper = Url
      };
    }

    /// <summary>
    /// Redirects (<see cref="F:Microsoft.AspNetCore.Http.StatusCodes.Status308PermanentRedirect" />) to the specified action with
    /// <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.Permanent" /> set to true and <see cref="P:Microsoft.AspNetCore.Mvc.RedirectToActionResult.PreserveMethod" />
    /// set to true, using the specified <paramref name="actionName" />, <paramref name="controllerName" />,
    /// <paramref name="routeValues" />, and <paramref name="fragment" />.
    /// </summary>
    /// <param name="actionName">The name of the action.</param>
    /// <param name="controllerName">The name of the controller.</param>
    /// <param name="routeValues">The route data to use for generating the URL.</param>
    /// <param name="fragment">The fragment to add to the URL.</param>
    /// <returns>The created <see cref="T:Microsoft.AspNetCore.Mvc.RedirectToActionResult" /> for the response.</returns>
    [NonAction]
    public override RedirectToActionResult RedirectToActionPermanentPreserveMethod(
      string? actionName = null,
      string? controllerName = null,
      object? routeValues = null,
      string? fragment = null)
    {
      return new RedirectToActionResult(actionName, controllerName, routeValues, true, true, fragment)
      {
        UrlHelper = Url
      };
    }
}
