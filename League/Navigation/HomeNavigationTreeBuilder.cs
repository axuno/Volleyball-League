using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace League.Navigation
{
    /// <summary>
    /// Creates the Info menu section.
    /// </summary>
    public class HomeNavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<HomeNavigationTreeBuilder> _logger;
        private readonly IStringLocalizer<NavigationResource> _localizer;
        private readonly IUrlHelper _urlHelper;

        public HomeNavigationTreeBuilder(ITenantContext tenantContext, ILogger<HomeNavigationTreeBuilder> logger, IStringLocalizer<NavigationResource> localizer, IUrlHelper urlHelper)
        {
            _tenantContext = tenantContext;
            _logger = logger;
            _localizer = localizer;
            _urlHelper = urlHelper;
        }
        public string Name => string.Join(".", nameof(League), nameof(Navigation), nameof(HomeNavigationTreeBuilder));

        public Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            NavigationNode topNode;
            if (_tenantContext.IsDefault)
            {
                topNode = new NavigationNode { Text = string.Empty, Url = _urlHelper.Action(nameof(League.Controllers.Home.Welcome), nameof(League.Controllers.Home)), IconCssClass = "fas fa-1x fa-home", Key = "Home"};
            }
            else
            {
                topNode = new NavigationNode { Text = string.Empty, Url = "/" + _tenantContext.SiteContext.UrlSegmentValue, IconCssClass = "fas fa-1x fa-home", Key = "Home"};
            }
           
            var treeNode = new TreeNode<NavigationNode>(topNode);
            
            /*
            
            treeNode.AddChild(new NavigationNode { Text = _localizer["Rule of game"], Url = _urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { organization=_tenantContext.SiteContext.UrlSegmentValue, section="info", content="ruleofgame"}), Key = "RuleOfGame" });
            treeNode.AddChild(new NavigationNode { Text = _localizer["News"], Url =_urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { organization=_tenantContext.SiteContext.UrlSegmentValue, section="info", content="news"}), Key = "News" });
            treeNode.AddChild(new NavigationNode { Text = _localizer["Tournaments"], Url = "https://volleyball-turnier.de/", Key = "Tournaments" });
            
            if(string.IsNullOrEmpty(treeNode.Value.Url)) _logger.LogError("TreeNode for {0} has empty Url", treeNode.Value.Text);
            if(string.IsNullOrEmpty(treeNode.Children[0].Value.Url)) _logger.LogError("TreeNode for {0}/{1} has empty Url", treeNode.Value.Text, treeNode.Children[0].Value.Text);
            if(string.IsNullOrEmpty(treeNode.Children[1].Value.Url)) _logger.LogError("TreeNode for {0}/{1} has empty Url", treeNode.Value.Text, treeNode.Children[0].Value.Text);
            */ 
            return Task.FromResult(treeNode);
        }
    }
}
