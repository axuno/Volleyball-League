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
    public class InfosNavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<InfosNavigationTreeBuilder> _logger;
        private readonly IStringLocalizer<NavigationResource> _localizer;
        private readonly IUrlHelper _urlHelper;

        public InfosNavigationTreeBuilder(ITenantContext tenantContext, ILogger<InfosNavigationTreeBuilder> logger, IStringLocalizer<NavigationResource> localizer, IUrlHelper urlHelper)
        {
            _tenantContext = tenantContext;
            _logger = logger;
            _localizer = localizer;
            _urlHelper = urlHelper;
        }
        public string Name => string.Join(".", nameof(League), nameof(Navigation), nameof(InfosNavigationTreeBuilder));

        public Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            if (_tenantContext.IsDefault)
            {
                // A TreeNode, that won't be rendered
                return Task.FromResult(new TreeNode<NavigationNode>(new NavigationNode()));
            }

            var topNode = new NavigationNode { Text = _localizer["Info"], Url = _urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { organization=_tenantContext.SiteContext.UrlSegmentValue, section="info", content=""}), Key = "Info"};
            var treeNode = new TreeNode<NavigationNode>(topNode);
            treeNode.AddChild(new NavigationNode { Text = _localizer["Rule of game"], Url = _urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { organization=_tenantContext.SiteContext.UrlSegmentValue, section="info", content="ruleofgame"}), Key = "RuleOfGame" });
            treeNode.AddChild(new NavigationNode { Text = _localizer["News"], Url =_urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { organization=_tenantContext.SiteContext.UrlSegmentValue, section="info", content="news"}), Key = "News" });
            treeNode.AddChild(new NavigationNode { Text = _localizer["Tournaments"], Url = "https://volleyball-turnier.de/", Key = "Tournaments" });
            
            if(string.IsNullOrEmpty(treeNode.Value.Url)) _logger.LogError("TreeNode for {0} has empty Url", treeNode.Value.Text);
            if(string.IsNullOrEmpty(treeNode.Children[0].Value.Url)) _logger.LogError("TreeNode for {0}/{1} has empty Url", treeNode.Value.Text, treeNode.Children[0].Value.Text);
            if(string.IsNullOrEmpty(treeNode.Children[1].Value.Url)) _logger.LogError("TreeNode for {0}/{1} has empty Url", treeNode.Value.Text, treeNode.Children[0].Value.Text);
                
            return Task.FromResult(treeNode);
        }
    }
}
