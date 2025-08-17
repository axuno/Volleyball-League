using Microsoft.AspNetCore.Mvc.Razor;

namespace League.Views;

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
    public void PopulateValues(ViewLocationExpanderContext context)
    {
        context.Values["customviewlocation"] = nameof(LeagueViewLocationExpander);
    }

    public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
    {
        var tenantSearchPaths = new List<string>();

        try
        {
            var tenantContext = (TournamentManager.MultiTenancy.ITenantContext)
                context.ActionContext.HttpContext.RequestServices.GetService(typeof(TournamentManager.MultiTenancy.ITenantContext))!;

            if (!tenantContext.IsDefault)
            {
                tenantSearchPaths.Add($"/Views/{{1}}/{tenantContext.SiteContext.FolderName}/{{0}}.cshtml");
            }
        }
        catch
        {
            // SiteContext cannot be resolved
        }

        // Concat avoids modifying the original collection
        return tenantSearchPaths.Concat(viewLocations);
    }
}

