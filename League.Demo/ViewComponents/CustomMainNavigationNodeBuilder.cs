using System;
using System.Linq;
using System.Threading.Tasks;
using League.Components;
using League.MultiTenancy;
using League.WebApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.WebApp.ViewComponents;

/// <summary>
/// Builds the standard league main navigation nodes
/// plus individual nodes
/// </summary>
public class CustomMainNavigationNodeBuilder : MainNavigationNodeBuilder
{
    /// <inheritdoc />
    public CustomMainNavigationNodeBuilder(TenantStore tenantStore, ITenantContext tenantContext, IAuthorizationService authorizationService, TenantLink tenantUrlHelper, IStringLocalizer<MainNavigationNodeBuilder> localizer, ILogger<MainNavigationNodeBuilder> logger) : base(tenantStore, tenantContext, authorizationService, tenantUrlHelper, localizer, logger)
    { }

    /// <inheritdoc />
    protected override async Task CreateStandardNavigationNodes()
    {
        // Create the standard league navigation
        await base.CreateStandardNavigationNodes();
            
        // The views must exist in "~/Views/TenantContent/"
        // and named with $"./{_tenantContext.SiteContext.FolderName}/{category}_{topic}"

        #region ** Home **
        var home = TenantContext.IsDefault
            ? new MainNavigationComponentModel.NavigationNode
            {
                Text = string.Empty,
                Url =  TenantLink.Action(nameof(Home.Welcome), nameof(Home)),
                IconCssClass = "fas fa-1x fa-home", Key = "Home_League"
            }
            : new MainNavigationComponentModel.NavigationNode
            {
                Text = string.Empty, 
                Url = "/" + TenantContext.SiteContext.UrlSegmentValue,
                IconCssClass = "fas fa-1x fa-home", Key = "Home_Tenant"
            };

        #endregion

        #region ** Info **
        var info = new MainNavigationComponentModel.NavigationNode
        {
            Key = "Top_Info",
            Text = Localizer["Info"],
            Url = TenantLink.Action(nameof(TenantContent.Index),
                nameof(TenantContent),
                new { category = "info", content = string.Empty })
        };
        info.ChildNodes.AddRange(new []
        {
            new MainNavigationComponentModel.NavigationNode
            {
                Key = "Info_RuleOfGame",
                Text = Localizer["Rule of game"],
                Url = TenantLink.Action(nameof(TenantContent.Index), nameof(TenantContent),
                    new { category = "info", topic = "ruleofgame" })
            },
            new MainNavigationComponentModel.NavigationNode
            {
                Key = "Info_News",
                Text = Localizer["News"],
                Url = TenantLink.Action(nameof(TenantContent.Index), nameof(TenantContent),
                    new { category = "info", topic = "news"})
            }
        });
            
        if (!TenantContext.IsDefault)
        {
            // Insert the individual node before the Top_Teams node
            InsertTopNavigationNode(info, "Top_Teams");
        }

        InsertTopNavigationNode(home, NavigationNodes.First().Key);

        #endregion
    }
}
