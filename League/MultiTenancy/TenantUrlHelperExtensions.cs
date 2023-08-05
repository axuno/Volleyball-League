//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using League.Routing;

namespace League.MultiTenancy;

public static class TenantLinkExtensions
{
    /// <summary>
    /// Generates a URL with a tenant-specific path for an action method, which contains the specified
    /// <paramref name="action" /> and <paramref name="controller" /> names.
    /// </summary>
    /// <param name="tenantLink">The <see cref="TenantLink"/></param>
    /// <param name="action">The name of the action method.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="values">The route values. Used to expand parameters in the route template.</param>
    /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
    /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
    /// <returns>The generated path.</returns>
    public static string? Action(this TenantLink tenantLink, string? action = null, string? controller = null, object? values = null, PathString? pathBase = null, FragmentString fragment = default)
    {
        var valuesDict = tenantLink.GetValuesDictionary(values);
        
        if (!valuesDict.ContainsKey(TenantRouteConstraint.Key))
            valuesDict.Add(TenantRouteConstraint.Key, tenantLink.SiteContext.UrlSegmentValue);

        return tenantLink.LinkGenerator.GetPathByAction(tenantLink.HttpContext, action, controller, valuesDict, pathBase, fragment);
    }

    /// <summary>
    /// Generates a URL with a tenant-specific path for an action method, which contains the specified
    /// <paramref name="action" /> and <paramref name="controller" /> names.
    /// </summary>
    /// <param name="tenantLink">The <see cref="TenantLink"/></param>
    /// <param name="action">The name of the action method.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="values">The route values. Used to expand parameters in the route template.</param>
    /// <param name="pathBase">An optional URI path base. Prepended to the path in the resulting URI.</param>
    /// <param name="fragment">An optional URI fragment. Appended to the resulting URI.</param>
    /// <param name="scheme">The URI scheme, applied to the resulting URI.</param>
    /// <param name="host">The URI host/authority, applied to the resulting URI.</param>
    /// <returns>The generated URI.</returns>
    public static string? ActionLink(this TenantLink tenantLink,
        string? action = null,
        string? controller = null,
        object? values = null,
        string? scheme = null,
        HostString? host = null,
        PathString? pathBase = null,
        FragmentString fragment = default)
    {
        var valuesDict = tenantLink.GetValuesDictionary(values);
        
        if (!valuesDict.ContainsKey(TenantRouteConstraint.Key))
            valuesDict.Add(TenantRouteConstraint.Key, tenantLink.SiteContext.UrlSegmentValue);

        return tenantLink.LinkGenerator.GetUriByAction(tenantLink.HttpContext, action, controller, valuesDict, scheme, host, pathBase, fragment);
    }
}
