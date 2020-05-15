using System;
using League.DI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace League.ControllerAttributes
{
	public class TeamApplicationAllowedAttribute : ActionFilterAttribute
    {
		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.RequestServices.GetService(typeof(SiteContext)) is
                    SiteContext siteContext &&
                (!siteContext.ApplicationAllowed && (filterContext.RouteData.Values["controller"].ToString()
                                                         .Equals(nameof(League.Controllers.TeamApplication),
                                                             StringComparison.OrdinalIgnoreCase) &&
                                                     !filterContext.RouteData.Values["action"].ToString()
                                                         .Equals(nameof(League.Controllers.TeamApplication.List),
                                                             StringComparison.OrdinalIgnoreCase)
                    )))
            {
                filterContext.Result = new RedirectToActionResult(nameof(Controllers.TeamApplication.List),
                    nameof(Controllers.TeamApplication), new { });
            }
        }
	}
}