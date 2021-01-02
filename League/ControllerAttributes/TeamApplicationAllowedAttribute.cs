using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using TournamentManager.MultiTenancy;

namespace League.ControllerAttributes
{
	public class TeamApplicationAllowedAttribute : ActionFilterAttribute
    {
		public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.RequestServices.GetService(typeof(ITenantContext)) is
                    ITenantContext tenantContext &&
                (!tenantContext.TournamentContext.ApplicationAllowed && (filterContext.RouteData.Values["controller"].ToString()
                                                         .Equals(nameof(League.Controllers.TeamApplication),
                                                             StringComparison.OrdinalIgnoreCase) &&
                                                     !filterContext.RouteData.Values["action"].ToString()
                                                         .Equals(nameof(League.Controllers.TeamApplication.List),
                                                             StringComparison.OrdinalIgnoreCase)
                    )))
            {
                filterContext.Result = new RedirectToActionResult(nameof(Controllers.TeamApplication.List),
                    nameof(Controllers.TeamApplication), new { Organization = tenantContext.SiteContext.UrlSegmentValue });
            }
        }
	}
}