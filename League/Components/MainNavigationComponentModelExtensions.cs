using System.Collections.Generic;
using System.Linq;

namespace League.Components;

/// <summary>
/// Extensions for the <see cref="MainNavigationComponentModel"/> and <see cref="MainNavigationComponentModel.NavigationNode"/>.
/// </summary>
public static class MainNavigationComponentModelExtensions
{
    /// <summary>
    /// Checks, whether the <see cref="MainNavigationComponentModel.NavigationNode"/> contains child nodes
    /// where <see cref="MainNavigationComponentModel.NavigationNode.IsVisible"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    public static bool HasVisibleChildNodes(this MainNavigationComponentModel.NavigationNode node)
    {
        return node.ChildNodes.Any(childNode => childNode.ShouldShow());
    }

    /// <summary>
    /// Gets the active <see cref="MainNavigationComponentModel.NavigationNode"/> for the current <see cref="Microsoft.AspNetCore.Http.HttpContext.Request"/>.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="nodes"></param>
    /// <param name="httpContext"></param>
    /// <returns>The active <see cref="MainNavigationComponentModel.NavigationNode"/> for the current <see cref="Microsoft.AspNetCore.Http.HttpContext.Request"/> or <see langword="null"/> if no node was found.</returns>
    public static MainNavigationComponentModel.NavigationNode? FindActiveNodeFromUrl(this MainNavigationComponentModel model, List<MainNavigationComponentModel.NavigationNode> nodes, Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        foreach (var node in nodes)
        {
            // First check if the url can be found in a child node!
            if (node.ChildNodes.Any())
            {
                var found = FindActiveNodeFromUrl(model, node.ChildNodes, httpContext);
                if (found != null) return found;
            }
            // Second check if this node contains the url
            if (node.Url != null && node.Url.Equals(httpContext.Request.Path.Value)) return node;
        }

        return null;
    }
        
    /// <summary>
    /// Gets a string of css classes which can be used for link.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpContext"></param>
    /// <param name="node"></param>
    /// <param name="inputClass"></param>
    /// <param name="activeClass"></param>
    /// <param name="makeParentNodesActive"></param>
    /// <returns>A string of css classes which can be used for link.</returns>
    public static string? GetClass(
        this MainNavigationComponentModel model,
        Microsoft.AspNetCore.Http.HttpContext httpContext,
        MainNavigationComponentModel.NavigationNode? node,
        string? inputClass = null,
        string activeClass = "active",
        bool makeParentNodesActive = false)
    {
        if (node == null)
            return inputClass;
        var currentNode = model.GetActiveNode(httpContext).Result;
            
        if (currentNode != null && node.Key.Equals(currentNode.Key))
            inputClass = string.IsNullOrEmpty(inputClass) ? activeClass : activeClass + " " + inputClass;
        else if (makeParentNodesActive && model.HasActiveChild(httpContext, node))
            inputClass = string.IsNullOrEmpty(inputClass) ? activeClass : activeClass + " " + inputClass;
        if (string.IsNullOrEmpty(node.CssClass))
            return inputClass;
        return !string.IsNullOrEmpty(inputClass) ? inputClass + " " + node.CssClass : node.CssClass;
    }

    /// <summary>
    /// Get a html element for an icon.
    /// </summary>
    /// <param name="node"></param>
    /// <returns>A html element for an icon.</returns>
    public static string GetIcon(this MainNavigationComponentModel.NavigationNode? node)
    {
        return node == null || string.IsNullOrEmpty(node.IconCssClass)
            ? string.Empty
            : "<i class='" + node.IconCssClass + "'></i> ";
    }

    /// <summary>
    /// Checks, whether the given <see cref="MainNavigationComponentModel.NavigationNode"/> is the active node.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpContext"></param>
    /// <param name="node"></param>
    /// <returns><see langword="true"/>, if the given <see cref="MainNavigationComponentModel.NavigationNode"/> is the active node.</returns>
    public static bool IsActiveNode(this MainNavigationComponentModel model, Microsoft.AspNetCore.Http.HttpContext httpContext,
        MainNavigationComponentModel.NavigationNode? node)
    {
        var currentNode = model.GetActiveNode(httpContext).Result;
        return node != null && currentNode != null && node.Key.Equals(currentNode.Key);
    }

    /// <summary>
    /// Checks, whether the node is visible and the node url is not null.
    /// </summary>
    /// <param name="node"></param>
    /// <returns><see langref="true"/> if the node is visible and the node url is not null.</returns>
    public static bool ShouldShow(this MainNavigationComponentModel.NavigationNode? node)
    {
        return node != null && (node.IsVisible && node.Url != null || node.ChildNodes.Any(n => n.ShouldShow()));
    }

    /// <summary>
    /// Checks, whether the given <see cref="MainNavigationComponentModel.NavigationNode"/> contains an active child node.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="httpContext"></param>
    /// <param name="node"></param>
    /// <returns><see langword="true"/>, if the given <see cref="MainNavigationComponentModel.NavigationNode"/> contains an active child node.</returns>
    public static bool HasActiveChild(this MainNavigationComponentModel model, Microsoft.AspNetCore.Http.HttpContext httpContext, MainNavigationComponentModel.NavigationNode? node)
    {
        var currentNode = model.GetActiveNode(httpContext).Result;
            
        if (node == null || currentNode == null)
            return false;
        if (node.Key.Equals(currentNode.Key))
            return true;
        if (!node.ChildNodes.Any())
            return false;
            
        return model.FindActiveNodeFromUrl(node.ChildNodes, httpContext) != null;
    }
}
