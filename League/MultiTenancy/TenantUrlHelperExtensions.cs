//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using Microsoft.AspNetCore.Mvc;

namespace League.MultiTenancy;

public static class TenantUrlHelperExtensions
{
    /// <summary>
    /// Generates a URL with a tenant-specific path for an action method, which contains the specified
    /// <paramref name="action" /> and <paramref name="controller" /> names.
    /// </summary>
    /// <param name="tenantUrlHelper">The <see cref="TenantUrlHelper"/></param>
    /// <param name="action">The name of the action method.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <returns>The generated URL.</returns>
    public static string? Action(this TenantUrlHelper tenantUrlHelper, string? action, string? controller)
    {
        return Action(tenantUrlHelper, action, controller, null, null, null);
    }

    /// <summary>
    /// Generates a URL with a tenant-specific path for an action method, which contains the specified
    /// <paramref name="action" /> and <paramref name="controller" /> names.
    /// </summary>
    /// <param name="tenantUrlHelper">The <see cref="TenantUrlHelper"/></param>
    /// <param name="action">The name of the action method.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="values"></param>
    /// <returns>The generated URL.</returns>
    public static string? Action(this TenantUrlHelper tenantUrlHelper, string? action, string? controller, object? values)
    {
        return Action(tenantUrlHelper, action, controller, values, null, null);
    }

    /// <summary>
    /// Generates a URL with a tenant-specific path for an action method, which contains the specified
    /// <paramref name="action" /> and <paramref name="controller" /> names.
    /// </summary>
    /// <param name="tenantUrlHelper">The <see cref="TenantUrlHelper"/></param>
    /// <param name="action">The name of the action method.</param>
    /// <param name="controller">The name of the controller.</param>
    /// <param name="values"></param>
    /// <param name="protocol"></param>
    /// <param name="host"></param>
    /// <returns>The generated URL.</returns>
    public static string? Action(this TenantUrlHelper tenantUrlHelper, string? action, string? controller, object? values, string? protocol, string? host)
    {
        var valuesDict = tenantUrlHelper.GetValuesDictionary(values);
        
        if (!valuesDict.ContainsKey("organization"))
            valuesDict.Add("organization", tenantUrlHelper.SiteContext.UrlSegmentValue);
        
        return tenantUrlHelper.Url.Action(action, controller, valuesDict, protocol, host);
    }
}
