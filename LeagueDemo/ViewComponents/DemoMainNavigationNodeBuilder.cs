using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using League.Components;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace LeagueDemo.ViewComponents
{
    /// <summary>
    /// Builds the standard league main navigation nodes
    /// plus individual nodes
    /// </summary>
    public class DemoMainNavigationNodeBuilder : MainNavigationNodeBuilder
    {
        /// <inheritdoc />>
        public DemoMainNavigationNodeBuilder(TenantStore tenantStore, ITenantContext tenantContext, IHttpContextAccessor httpContextAccessor, IAuthorizationService authorizationService, LinkGenerator linkGenerator, IStringLocalizer<MainNavigationNodeBuilder> localizer, ILogger<MainNavigationNodeBuilder> logger) : base(tenantStore, tenantContext, httpContextAccessor, authorizationService, linkGenerator, localizer, logger)
        { }

        /// <inheritdoc />>
        protected override async Task CreateStandardNavigationNodes()
        {
            // Create the standard league navigation
            await base.CreateStandardNavigationNodes();
            
            // The view must exist in "~/Views/TenantContent/"
            // and named with $"./{_tenantContext.SiteContext.FolderName}/{category}_{topic}"
            
            // Create an individual "Info" node
            var node = new MainNavigationComponentModel.NavigationNode
            {
                Key = "Top_Info",
                Text = Localizer["Info"],
                Url = LinkGenerator.GetPathByAction(HttpContext, nameof(League.Controllers.TenantContent.Index),
                    nameof(League.Controllers.TenantContent),
                    new { organization = TenantContext.SiteContext.UrlSegmentValue, category = "info", content = string.Empty })
            };
            node.ChildNodes.AddRange(new []
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    Key = "Info_RuleOfGame",
                    Text = Localizer["Rule of game"],
                    Url = LinkGenerator.GetPathByAction(HttpContext, nameof(League.Controllers.TenantContent.Index), nameof(League.Controllers.TenantContent),
                        new { organization = TenantContext.SiteContext.UrlSegmentValue, category = "info", topic = "ruleofgame" })
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    Key = "Info_News",
                    Text = Localizer["News"],
                    Url = LinkGenerator.GetPathByAction(HttpContext, nameof(League.Controllers.TenantContent.Index), nameof(League.Controllers.TenantContent),
                        new { organization = TenantContext.SiteContext.UrlSegmentValue, category = "info", topic = "news"})
                }
            });

            // Insert the individual node before the Top_Teams node
            InsertTopNavigationNode(node, "Top_Teams");
        }
    }
}
