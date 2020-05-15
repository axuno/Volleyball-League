using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace League.Views
{
    /// <summary>
    /// Expands the default search path list by organization specific folders per HttpRequest.
    /// </summary>
    /// <code>
    /// The ViewLocationExpander must be added to <see cref="RazorViewEngineOptions"/> like so:
    /// services.Configure&lt;RazorViewEngineOptions&gt;(options =&gt; {
    /// options.ViewLocationExpanders.Add(new LeagueViewLocationExpander());
    /// });
    /// </code>
    public class LeagueViewLocationExpander : IViewLocationExpander
    {
        private readonly List<string> _organizationSearchPaths = new List<string>();

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // just provides additional data incorporated into the cache key used for the location value
            context.Values["customviewlocation"] = nameof(LeagueViewLocationExpander);
            
            try
            {
                var siteContext = (DI.SiteContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(DI.SiteContext));
                _organizationSearchPaths.Clear();
                if (!string.IsNullOrEmpty(siteContext.OrganizationKey))
                {
                    _organizationSearchPaths.AddRange(new[]
                    {
                        $"/Views/{{1}}/{siteContext.FolderName}/{{0}}.cshtml",
                        $"/Views/Emails/{siteContext.FolderName}/{{0}}.cshtml",
                        $"/Views/shared/{siteContext.FolderName}/{{0}}.cshtml"
                    });
                }
            }
            catch
            {
                // DI.siteContext cannot be resolved
            }
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return _organizationSearchPaths.Union(viewLocations); // in order to replace all existing locations, leave "Union" away.
        }
    }
}
