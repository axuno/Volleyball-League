/* Copyright (c) .NET Foundation. All rights reserved.
   Licensed under the Apache License, Version 2.0, http://www.apache.org/licenses/LICENSE-2.0. See License.txt in the project root for license information.
   Source: https://github.com/aspnet/Entropy/blob/master/samples/Mvc.RenderViewToString/RazorViewToStringRenderer.cs
   Enhanced with improvements from Razor.Templating.Core (https://github.com/soundaranbu/Razor.Templating.Core)
   Original code modified by axuno. Modifications Copyright (c) by axuno.
*/

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace League.Emailing;

public class RazorViewToStringRenderer(
    IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider,
    IHttpContextAccessor httpContextAccessor)
{
    /// <summary>
    /// Renders a Razor view to a string with strong typing support.
    /// </summary>
    /// <typeparam name="TModel">The type of the model.</typeparam>
    /// <param name="viewName">The name or path of the view to render.</param>
    /// <param name="model">The model to pass to the view.</param>
    /// <param name="isMainPage">Whether this is a main page (true) or partial (false).</param>
    /// <returns>The rendered view as a string.</returns>
    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, bool isMainPage = true)
    {
        var actionContext = GetActionContext();
        var view = FindView(actionContext, viewName, isMainPage);

        await using var output = new StringWriter();
        var viewContext = new ViewContext(
            actionContext,
            view,
            new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            },
            new TempDataDictionary(
                actionContext.HttpContext,
                tempDataProvider),
            output,
            new HtmlHelperOptions());

        await view.RenderAsync(viewContext);

        return output.ToString();
    }

    private IView FindView(ActionContext actionContext, string viewName, bool isMainPage)
    {
        var getViewResult = viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage);
        if (getViewResult.Success)
        {
            return getViewResult.View;
        }

        var findViewResult = viewEngine.FindView(actionContext, viewName, isMainPage);
        if (findViewResult.Success)
        {
            return findViewResult.View;
        }

        var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
        var errorMessage = string.Join(
            Environment.NewLine,
            new[] {
                $"Unable to find view '{viewName}'. The following locations were searched:"
            }.Concat(searchedLocations)
            .Concat([
                "Hint:",
                "- Check whether you have added reference to the Razor Class Library that contains the view files.",
                "- Check whether the view file name is correct or exists at the given path."
            ]));

        throw new ViewNotFoundException(errorMessage);
    }

    private ActionContext GetActionContext()
    {
        var httpContext = httpContextAccessor.HttpContext;
        var endpoint = httpContext?.GetEndpoint();
        var actionDescriptor = endpoint?.Metadata.GetMetadata<ActionDescriptor>();

        ActionContext actionContext;

        if (httpContext is null || endpoint is null)
        {
            // Non HTTP request scenarios like console, worker services
            actionContext = GetDefaultActionContext();
        }
        else
        {
            actionContext = new ActionContext(httpContext, httpContext.GetRouteData(), actionDescriptor ?? new ActionDescriptor());
        }

        return actionContext;
    }

    private ActionContext GetDefaultActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };
        var app = new ApplicationBuilder(serviceProvider);
        var routeBuilder = new RouteBuilder(app)
        {
            DefaultHandler = new CustomRouter()
        };

        routeBuilder.MapRoute(
            string.Empty,
            "{controller}/{action}/{id}",
            new RouteValueDictionary(new { id = "defaultid" }));

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor
        {
            DisplayName = nameof(RazorViewToStringRenderer)
        });
        actionContext.RouteData.Routers.Add(routeBuilder.Build());
        return actionContext;
    }
}

internal class CustomRouter : IRouter
{
    public VirtualPathData? GetVirtualPath(VirtualPathContext context)
    {
        return null;
    }

    public Task RouteAsync(RouteContext context)
    {
        return Task.CompletedTask;
    }
}
