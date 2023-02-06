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
    private readonly TenantStore _tenantStore;

    public TenantRouteConstraint(TenantStore tenantStore)
    {
        // Note: Dependency injection for IRouteConstraint only works for Singleton services.
        //       => TenantStore can be used for DI, TenantResolver as scoped service cannot.
        _tenantStore = tenantStore;
    }

    /// <summary>
    /// The name used as for this <see cref="TenantRouteConstraint"/>.
    /// </summary>
    public const string Name = "MatchingTenant";

    /// <summary>
    /// The key of the tenant used in a <see cref="RouteValueDictionary"/>
    /// </summary>
    public const string Key = "Organization";

    /// <summary>
    /// The value for the constraint used in <see cref="Route"/> templates.
    /// </summary>
    public const string Template = "{" + Key + ":" + Name + "}";

    /// <inheritdoc />
    public bool Match(HttpContext? httpContext, IRouter? route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
    {
        switch (routeDirection)
        {
            case RouteDirection.IncomingRequest:
                ArgumentNullException.ThrowIfNull(httpContext);
                var tenantResolver = httpContext.RequestServices.GetRequiredService<TenantResolver>();
                var tenant = tenantResolver.Resolve();
                return tenant.SiteContext.UrlSegmentValue
                    .Equals(values[parameterName]?.ToString(), StringComparison.InvariantCultureIgnoreCase);
            case RouteDirection.UrlGeneration:
            default:
                return _tenantStore.GetTenants().Values.FirstOrDefault(t =>
                    t.SiteContext.UrlSegmentValue.Equals(values[parameterName]?.ToString(),
                        StringComparison.InvariantCultureIgnoreCase)) != null;
        }
    }
}
