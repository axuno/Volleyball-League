//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using League.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

#if DEBUG

namespace League.Areas.Admin.Controllers;

public class Debug : AbstractController
{
    [Area("Admin")]
    [HttpGet]
    [Route("[area]/[controller]/[action]")]
    public IActionResult Routes([FromServices] IEnumerable<EndpointDataSource> endpointSources)
    {
        /*
         *  Credits for the article and code samples from Gérald Barré aka. meziantou
         *  https://www.meziantou.net/list-all-routes-in-an-asp-net-core-application.htm
         */

        var sb = new StringBuilder();
        sb.Append("*** Registered Endpoint Routes ***").AppendFormat("{0}{0}", Environment.NewLine);
        var endpoints = endpointSources.SelectMany(es => es.Endpoints);
        foreach (var endpoint in endpoints)
        {
            sb.Append(endpoint.DisplayName).Append(" =: ");
            if (endpoint is RouteEndpoint routeEndpoint)
            {
                sb.Append(routeEndpoint.RoutePattern.RawText).Append(" =: ");
                _ = routeEndpoint.RoutePattern.RawText;
                _ = routeEndpoint.RoutePattern.PathSegments;
                _ = routeEndpoint.RoutePattern.Parameters;
                _ = routeEndpoint.RoutePattern.InboundPrecedence;
                _ = routeEndpoint.RoutePattern.OutboundPrecedence;
            }
            else
            {
                sb.Append(Environment.NewLine);
            }

            var routeNameMetadata = endpoint.Metadata
                .OfType<RouteNameMetadata>().FirstOrDefault();
            _ = routeNameMetadata?.RouteName;

            var httpMethodsMetadata = endpoint.Metadata.OfType<HttpMethodMetadata>().FirstOrDefault();
            if (httpMethodsMetadata?.HttpMethods != null)
            {
                sb.AppendJoin(" ", httpMethodsMetadata.HttpMethods);
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine);
            }
            _ = httpMethodsMetadata?.HttpMethods; // [GET, POST, ...]
            // Note: There are more metadata types available!
        }

        return Content(sb.ToString());
    }
}

#endif
