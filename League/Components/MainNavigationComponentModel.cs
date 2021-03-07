using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
#nullable enable

namespace League.Components
{
    /// <summary>
    /// The component model for the <see cref="MainNavigation"/> <see cref="Microsoft.AspNetCore.Mvc.ViewComponent"/>.
    /// </summary>
    public class MainNavigationComponentModel
    {
        /// <summary>
        /// Represents a node in the main navigation
        /// </summary>
        public class NavigationNode
        {
            /// <summary>
            /// The parent node of this code (null if no parent exists)
            /// </summary>
            public NavigationNode? ParentNode { get; set; }
            /// <summary>
            /// The key of the node. It is used equality comparisons.
            /// </summary>
            public string Key { get; set; } = string.Empty;
            /// <summary>
            /// The target url which will be used in the link element.
            /// </summary>
            public string Url { get; set; } = string.Empty;
            /// <summary>
            /// The display text of the navigation node. The text should be blank if an <see cref="IconCssClass"/> is set.
            /// </summary>
            public string Text { get; set; } = string.Empty;
            /// <summary>
            /// The description text of navigation node. Can e.g. be rendered as title attribute of a menu element.
            /// </summary>
            public string Description { get; set; } = string.Empty;
            /// <summary>
            /// The target used for the navigation link element (e.g.: &quot;_blank&quot;).
            /// </summary>
            public string Target { get; set; } = string.Empty;
            /// <summary>
            /// The icon css class, which will be rendered. Usually it is used instead of a <see cref="Text"/>.
            /// </summary>
            public string IconCssClass { get; set; } = string.Empty;
            /// <summary>
            /// The css class to use the menu entry.
            /// </summary>
            public string CssClass { get; set; } = string.Empty;
            /// <summary>
            /// If <see langword="true"/> the navigation link can be clicked.
            /// </summary>
            public bool IsEnabled { get; set; } = true;
            /// <summary>
            /// If <see langword="true"/> the navigation link is visible.
            /// </summary>
            public bool IsVisible { get; set; } = true;
            /// <summary>
            /// The <see cref="List{NavigationNode}"/> of child <see cref="NavigationNode"/>s.
            /// </summary>
            public List<NavigationNode> ChildNodes { get; } = new List<NavigationNode>();
        }
        
        /// <summary>
        /// Gets the top navigation <see cref="List{NavigationNode}"/> of type <see cref="NavigationNode"/>.
        /// </summary>
        public List<NavigationNode> TopNavigationNodes { get; } = new List<NavigationNode>();
        
        /// <summary>
        /// Gets the active <see cref="NavigationNode"/> for the current <see cref="Microsoft.AspNetCore.Http.HttpContext.Request"/>.
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns>The active <see cref="NavigationNode"/> for the current <see cref="Microsoft.AspNetCore.Http.HttpContext.Request"/> or <see langword="null"/> if no node was found.</returns>
        public Task<NavigationNode?> GetActiveNode(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            return Task.FromResult(this.FindActiveNodeFromUrl(TopNavigationNodes, httpContext));
        }
    }
}

