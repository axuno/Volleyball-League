using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace League.Views
{
    /// <summary>
    /// Expands the default search path list by tenant-specific folders per HttpRequest.
    /// </summary>
    /// <code>
    /// The ViewLocationExpander must be added to <see cref="RazorViewEngineOptions"/> like so:
    /// services.Configure&lt;RazorViewEngineOptions&gt;(options =&gt; {
    /// options.ViewLocationExpanders.Add(new LeagueViewLocationExpander());
    /// });
    /// </code>
    public class LeagueViewLocationExpander : IViewLocationExpander
    {
        private readonly List<string> _tenantSearchPaths = new();

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            // just provides additional data incorporated into the cache key used for the location value
            context.Values["customviewlocation"] = nameof(LeagueViewLocationExpander);
            
            try
            {
                var tenantContext = (TournamentManager.MultiTenancy.ITenantContext)context.ActionContext.HttpContext.RequestServices.GetService(typeof(TournamentManager.MultiTenancy.ITenantContext));
                _tenantSearchPaths.Clear();
                if (!tenantContext.IsDefault)
                {
                    _tenantSearchPaths.AddRange(new[]
                    {
                        $"/Views/{{1}}/{tenantContext.SiteContext.FolderName}/{{0}}.cshtml",
                    });
                }
            }
            catch
            {
                // SiteContext cannot be resolved
            }
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            return _tenantSearchPaths.Union(viewLocations); // in order to replace all existing locations, leave "Union" away.
        }
    }
}
