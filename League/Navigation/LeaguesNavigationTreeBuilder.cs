using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using Microsoft.Extensions.Localization;
using TournamentManager.MultiTenancy;

namespace League.Navigation
{
    /// <summary>
    /// Reads the Leagues from configuration and inserts them into the navigation bar.
    /// </summary>
    public class LeaguesNavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly TenantStore _tenantStore;
        private readonly IStringLocalizer<NavigationResource> _localizer;

        public LeaguesNavigationTreeBuilder(TenantStore tenantStore, IStringLocalizer<NavigationResource> localizer)
        {
            _tenantStore = tenantStore;
            _localizer = localizer;
        }
        public string Name => string.Join(".", nameof(League), nameof(Navigation), nameof(LeaguesNavigationTreeBuilder));

        public Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            var topNode = new NavigationNode { Text = _localizer["League"], Url = "/#"};
            var treeNode = new TreeNode<NavigationNode>(topNode);
            treeNode.AddChild(new NavigationNode { Text = _localizer["Home"], Url = "/welcome", Key = "LeagueWelcome" });
            foreach (var tenant in _tenantStore.GetTenants().Values)
            {
                if (!string.IsNullOrEmpty(tenant.Identifier) && !tenant.SiteContext.HideInMenu)
                {
                    treeNode.AddChild(
                        new NavigationNode
                        {
                            Text = tenant.OrganizationContext.ShortName,
                            Url = "/" + tenant.SiteContext.UrlSegmentValue,
                            Key = "UrlSegment_" + tenant.SiteContext.UrlSegmentValue,
                            PreservedRouteParameters = "organization",
                        });
                }
            }
            treeNode.AddChild(new NavigationNode { Text = _localizer["League Overview"], Url = "/overview", Key = "LeagueOverview" });
            
            // To return a TreeNode, that won't be rendered, use: new TreeNode<NavigationNode>(new NavigationNode());
            return Task.FromResult(treeNode);
        }
    }
}
