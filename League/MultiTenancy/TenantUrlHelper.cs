//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy;

/// <summary>
/// A class that contains methods to
/// build tenant-specific URLs for ASP.NET MVC within the <see cref="League"/> application.
/// </summary>
public class TenantUrlHelper
{
    // Perf: Reuse the RouteValueDictionary across multiple calls of Action for this UrlHelper
    private readonly RouteValueDictionary _routeValueDictionary = new();

    /// <summary>
    /// Gets the <see cref="ISiteContext"/>.
    /// </summary>
    public ISiteContext SiteContext { get; }

    /// <summary>
    /// Gets or sets the <see cref="IUrlHelper"/>.
    /// </summary>
    public IUrlHelper Url { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantUrlHelper" /> class using the specified
    /// <paramref name="urlHelper"/> and <paramref name="tenantContext"/> .
    /// </summary>
    public TenantUrlHelper(IUrlHelper urlHelper, ITenantContext tenantContext)
    {
        SiteContext = tenantContext.SiteContext;
        Url = urlHelper;
    }

    /// <summary>
    /// Gets a <see cref="RouteValueDictionary"/> using the specified values.
    /// </summary>
    /// <param name="values">The values to use.</param>
    /// <returns>A <see cref="RouteValueDictionary"/> with the specified values.</returns>
    internal RouteValueDictionary GetValuesDictionary(object? values)
    {
        _routeValueDictionary.Clear();

        switch (values)
        {
            // Perf: RouteValueDictionary can be cast to IDictionary<string, object>, but it is
            // special cased to avoid allocating boxed Enumerator.
            case RouteValueDictionary routeValuesDictionary:
            {
                foreach (var kvp in routeValuesDictionary)
                {
                    _routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return _routeValueDictionary;
            }
            case IDictionary<string, object> dictionaryValues:
            {
                foreach (var kvp in dictionaryValues)
                {
                    _routeValueDictionary.Add(kvp.Key, kvp.Value);
                }

                return _routeValueDictionary;
            }
            case not null:
                // Namespace is null for anonymous types
                if (values.GetType().Namespace != null) return _routeValueDictionary;
                
                foreach (PropertyDescriptor propertyDescriptor in TypeDescriptor.GetProperties(values))
                {
                    var obj = propertyDescriptor.GetValue(values);
                    _routeValueDictionary.Add(propertyDescriptor.Name, obj);
                }

                return _routeValueDictionary;
        }

        return _routeValueDictionary;
    }
}
