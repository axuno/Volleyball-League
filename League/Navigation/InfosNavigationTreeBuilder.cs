using System.Threading.Tasks;
using cloudscribe.Web.Navigation;
using League.DI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace League.Navigation
{
    /// <summary>
    /// Creates the Info menu section.
    /// </summary>
    public class InfosNavigationTreeBuilder : INavigationTreeBuilder
    {
        private readonly OrganizationSiteContext _organizationSiteContext;
        private readonly IStringLocalizer<NavigationResource> _localizer;
        private readonly IUrlHelper _urlHelper;

        public InfosNavigationTreeBuilder(OrganizationSiteContext organizationSiteContext, IStringLocalizer<NavigationResource> localizer, IUrlHelper urlHelper)
        {
            _organizationSiteContext = organizationSiteContext;
            _localizer = localizer;
            _urlHelper = urlHelper;
        }
        public string Name => string.Join(".", nameof(League), nameof(Navigation), nameof(InfosNavigationTreeBuilder));

        public Task<TreeNode<NavigationNode>> BuildTree(NavigationTreeBuilderService service)
        {
            var topNode = new NavigationNode { Text = _localizer["Info"], Url = _urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { section="info", content=""}), Key = "Info"};
            var treeNode = new TreeNode<NavigationNode>(topNode);
            treeNode.AddChild(new NavigationNode { Text = _localizer["Rule of game"], Url = _urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { section="info", content="ruleofgame"}), Key = "RuleOfGame" });
            treeNode.AddChild(new NavigationNode { Text = _localizer["News"], Url =_urlHelper.Action(nameof(League.Controllers.Organization.Index), nameof(League.Controllers.Organization), new { section="info", content="news"}), Key = "News" });
            
            // To return a TreeNode, that won't be rendered, use: new TreeNode<NavigationNode>(new NavigationNode());
            return Task.FromResult(treeNode);
        }
    }
}
