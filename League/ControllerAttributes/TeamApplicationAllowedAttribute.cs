using League.MultiTenancy;
using Microsoft.AspNetCore.Mvc.Filters;
using TournamentManager.MultiTenancy;

namespace League.ControllerAttributes;

[AttributeUsage(AttributeTargets.Class)]
public class TeamApplicationAllowedAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.RequestServices.GetService(typeof(ITenantContext)) is
                ITenantContext tenantContext &&
            (!(DateTime.UtcNow >= tenantContext.TournamentContext.ApplicationStart && DateTime.UtcNow < tenantContext.TournamentContext.ApplicationEnd)
             && ((context.RouteData.Values["controller"]?.ToString() ?? string.Empty)
                 .Equals(nameof(League.Controllers.TeamApplication),
                     StringComparison.OrdinalIgnoreCase) &&
                 !(context.RouteData.Values["action"]?.ToString() ?? string.Empty)
                     .Equals(nameof(League.Controllers.TeamApplication.List),
                         StringComparison.OrdinalIgnoreCase)
             )))
        {
            var tenantLink = context.HttpContext.RequestServices.GetRequiredService<TenantLink>();
            context.Result = new RedirectResult(tenantLink.Action(nameof(Controllers.TeamApplication.List),
                nameof(Controllers.TeamApplication))!);
        }
    }
}
