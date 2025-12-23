//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using System.ComponentModel;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy;

/// <summary>
/// A class that contains methods to
/// build tenant-specific paths or URIs MVC within the <see cref="League"/> application.
/// </summary>
public class TenantLink
{
    // Perf: Reuse the RouteValueDictionary across multiple calls of Action for this UrlHelper
    private readonly RouteValueDictionary _routeValueDictionary = [];

    /// <summary>
    /// Gets the <see cref="ISiteContext"/>.
    /// </summary>
    public ISiteContext SiteContext { get; }

    /// <summary>
    /// Gets the underlying <see cref="LinkGenerator"/> used by <see cref="TenantLink"/>.
    /// </summary>
    public LinkGenerator LinkGenerator { get; }

    /// <summary>
    /// Gets the <see cref="HttpContext"/> for the current request.
    /// </summary>
    public HttpContext HttpContext { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TenantLink" /> class using the specified
    /// <paramref name="context"/>, <paramref name="defaultLinkGenerator"/> and <paramref name="tenantContext"/> .
    /// </summary>
    public TenantLink(IHttpContextAccessor context, LinkGenerator defaultLinkGenerator, ITenantContext tenantContext)
    {
        SiteContext = tenantContext.SiteContext;
        LinkGenerator = defaultLinkGenerator;
        HttpContext = context.HttpContext!;
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
