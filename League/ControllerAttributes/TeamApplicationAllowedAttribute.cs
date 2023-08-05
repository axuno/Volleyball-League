using League.MultiTenancy;
using Microsoft.AspNetCore.Mvc.Filters;
using TournamentManager.MultiTenancy;

namespace League.ControllerAttributes;

public class TeamApplicationAllowedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext filterContext)
    {
        if (filterContext.HttpContext.RequestServices.GetService(typeof(ITenantContext)) is
                ITenantContext tenantContext &&
            (!tenantContext.TournamentContext.ApplicationAllowed && ((filterContext.RouteData.Values["controller"]?.ToString() ?? string.Empty)
                                                                         .Equals(nameof(League.Controllers.TeamApplication),
                                                                             StringComparison.OrdinalIgnoreCase) &&
                                                                     !(filterContext.RouteData.Values["action"]?.ToString() ?? string.Empty)
                                                                         .Equals(nameof(League.Controllers.TeamApplication.List),
                                                                             StringComparison.OrdinalIgnoreCase)
                )))
        {
            var tenantLink = filterContext.HttpContext.RequestServices.GetRequiredService<TenantLink>();
            filterContext.Result = new RedirectResult(tenantLink.Action(nameof(Controllers.TeamApplication.List),
                nameof(Controllers.TeamApplication))!);
        }
    }
}
