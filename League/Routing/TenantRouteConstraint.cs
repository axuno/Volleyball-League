using System;
using System.Linq;
using League.MultiTenancy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using TournamentManager.MultiTenancy;

namespace League.Routing;

public class TenantRouteConstraint : IRouteConstraint
{
    public TenantRouteConstraint()
    {
        // Note: Dependency injection for IRouteConstraint only works for Singleton services.
        //       => TenantStore could be used for DI, TenantResolver could not.
        //       => We acquire all service types in the 'Match' method from HttpContext scope.
    }

    public const string Name = "MatchingTenant";
    public bool Match(HttpContext? httpContext, IRouter? route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
        if (httpContext == null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        var tenantResolver = httpContext.RequestServices.GetRequiredService<TenantResolver>();
        var tenantStore = httpContext.RequestServices.GetRequiredService<TenantStore>();

        switch (routeDirection)
        {
            case RouteDirection.IncomingRequest:
                var tenant = tenantResolver.Resolve();
                return tenantResolver.Resolve().SiteContext.UrlSegmentValue
                    .Equals(values[parameterName]?.ToString(), StringComparison.InvariantCultureIgnoreCase);
            case RouteDirection.UrlGeneration:
            default:
                return tenantStore.GetTenants().Values.FirstOrDefault(t =>
                    t.SiteContext.UrlSegmentValue.Equals(values[parameterName]?.ToString(),
                        StringComparison.InvariantCultureIgnoreCase)) != null;
        }
    }
}
