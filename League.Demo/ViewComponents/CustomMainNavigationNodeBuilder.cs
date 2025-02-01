using League.Components;
using League.Controllers;
using League.MultiTenancy;
using League.WebApp.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using TournamentManager.MultiTenancy;

namespace League.WebApp.ViewComponents;

/// <summary>
/// Builds the standard league main navigation nodes
/// plus individual nodes
/// </summary>
public class CustomMainNavigationNodeBuilder : MainNavigationNodeBuilder
{
    /// <summary>
    /// Builds the standard league main navigation nodes
    /// plus individual nodes
    /// </summary>
    /// <remarks>
    /// This MainNavigationNodeBuilder uses the resources from the base class for localization.
    /// </remarks>
    public CustomMainNavigationNodeBuilder(TenantStore tenantStore, ITenantContext tenantContext, IAuthorizationService authorizationService, TenantLink tenantUrlHelper, IStringLocalizer<MainNavigationNodeBuilder> localizer, ILogger<CustomMainNavigationNodeBuilder> logger)
        : base(tenantStore, tenantContext, authorizationService, tenantUrlHelper, localizer, logger)
    { }

    /// <inheritdoc />
    protected override async Task CreateStandardNavigationNodes()
    {
        // Create the standard league navigation
        await base.CreateStandardNavigationNodes();
            
        // The views must exist in "~/Views/TenantContent/"
        // and named with $"./{_tenantContext.SiteContext.FolderName}/{topic}"

        #region ** Home Node **
        var homeNode = TenantContext.IsDefault
            ? new MainNavigationComponentModel.NavigationNode
            {
                Key = "Home_League",
                Text = string.Empty,
                Url =  TenantLink.Action(nameof(Home.Welcome), nameof(Home)),
                IconCssClass = "fas fa-1x fa-home"
            }
            : new MainNavigationComponentModel.NavigationNode
            {
                Key = "Home_Tenant",
                Text = string.Empty, 
                Url = "/" + TenantContext.SiteContext.UrlSegmentValue,
                IconCssClass = "fas fa-1x fa-home"
            };

        #endregion

        #region ** Tenant Node **
        var tenantNode = new MainNavigationComponentModel.NavigationNode
        {
            Key = "Top_Tenant",
            Text = Localizer["Info"],
            Url = TenantLink.Action(nameof(TenantContent.Index),
                nameof(TenantContent),
                new { topic = string.Empty })
        };
        tenantNode.ChildNodes.AddRange(
        [
            new MainNavigationComponentModel.NavigationNode
            {
                Key = "Tenant_RuleOfGame",
                Text = Localizer["Rule of game"],
                Url = TenantLink.Action(nameof(TenantContent.Index), nameof(TenantContent),
                    new { topic = "rule-of-game" })
            },
            new MainNavigationComponentModel.NavigationNode
            {
                Key = "Tenant_News",
                Text = Localizer["News"],
                Url = TenantLink.Action(nameof(TenantContent.Index), nameof(TenantContent),
                    new { topic = "news" })
            }
        ]);
            
        if (!TenantContext.IsDefault)
        {
            // Insert the individual node before the Top_Teams node
            InsertTopNavigationNode(tenantNode, "Top_Teams");
        }

        InsertTopNavigationNode(homeNode, NavigationNodes[0].Key);

        #endregion
    }
}
