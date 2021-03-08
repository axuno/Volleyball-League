using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using League.Authorization;
using League.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;
#nullable enable

namespace League.Components
{
    /// <summary>
    /// Builds the standard league main navigation nodes.
    /// </summary>
    public class MainNavigationNodeBuilder : IMainNavigationNodeBuilder
    {
        /// <summary>
        /// Get the <see cref="TenantStore"/>.
        /// </summary>
        protected readonly TenantStore TenantStore;
        
        /// <summary>
        /// Gets the <see cref="ITenantContext"/>.
        /// </summary>
        protected readonly ITenantContext TenantContext;
        
        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        protected readonly ILogger<MainNavigationNodeBuilder> Logger;
        
        /// <summary>
        /// Gets the <see cref="HttpContext"/>.
        /// </summary>
        protected readonly HttpContext HttpContext;
        
        /// <summary>
        /// Gets the <see cref="IUrlHelper"/>.
        /// </summary>
        protected readonly IUrlHelper UrlHelper;
        
        /// <summary>
        /// Gets the <see cref="AuthorizationService"/>.
        /// </summary>
        protected readonly IAuthorizationService AuthorizationService;
        
        /// <summary>
        /// Gets the <see cref="ClaimsPrincipal"/> for the current request.
        /// </summary>
        protected readonly ClaimsPrincipal UserClaimsPrincipal;

        /// <summary>
        /// Gets the <see cref="List{T}"/> of type <see cref="MainNavigationComponentModel.NavigationNode"/>s.
        /// </summary>
        protected readonly List<MainNavigationComponentModel.NavigationNode> NavigationNodes;
        
        /// <summary>
        /// Gets the <see cref="IStringLocalizer{T}"/> for the <see cref="MainNavigationNodeBuilder"/>.
        /// </summary>
        protected readonly IStringLocalizer<MainNavigationNodeBuilder> Localizer;
        
        // To ensure that nodes can be only added once
        private bool _standardNavigationNodesAdded;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="tenantStore"></param>
        /// <param name="tenantContext"></param>
        /// <param name="authorizationService"></param>
        /// <param name="urlHelper"></param>
        /// <param name="localizer"></param>
        /// <param name="logger"></param>
        public MainNavigationNodeBuilder(TenantStore tenantStore, ITenantContext tenantContext, IAuthorizationService authorizationService, IUrlHelper urlHelper, IStringLocalizer<MainNavigationNodeBuilder> localizer, ILogger<MainNavigationNodeBuilder> logger)
        {
            TenantStore = tenantStore;
            TenantContext = tenantContext;
            UserClaimsPrincipal = urlHelper.ActionContext.HttpContext.User;
            AuthorizationService = authorizationService;
            UrlHelper = urlHelper;
            HttpContext = urlHelper.ActionContext.HttpContext;
            Logger = logger;
            NavigationNodes = new List<MainNavigationComponentModel.NavigationNode>();
            Localizer = localizer;
            _standardNavigationNodesAdded = false;
        }
        
        /// <inheritdoc/>
        public void InsertTopNavigationNode(MainNavigationComponentModel.NavigationNode nodeToInsert, string nodeKeyName)
        {
            var index = NavigationNodes.FindIndex(0,
                navigationNode => navigationNode.Key.Equals(nodeKeyName, StringComparison.InvariantCultureIgnoreCase));

            index = index == -1 ? 0 : index;
            NavigationNodes.Insert(index, nodeToInsert);
        }

        /// <inheritdoc/>
        public bool TryRemoveTopNavigationNode(string nodeKeyName)
        {
            var index = NavigationNodes.FindIndex(0,
                navigationNode => navigationNode.Key.Equals(nodeKeyName, StringComparison.InvariantCultureIgnoreCase));
            if (index == -1) return false;
            
            NavigationNodes.RemoveAt(index);
            return true;
        }

        /// <inheritdoc/>
        public List<MainNavigationComponentModel.NavigationNode> GetNavigationNodes()
        {
            if (_standardNavigationNodesAdded) return NavigationNodes;
            
            CreateStandardNavigationNodes().Wait();
            _standardNavigationNodesAdded = true;

            return NavigationNodes;
        }
        
        /// <summary>
        /// Creates the standard league navigation nodes. This method may be called only once.
        /// </summary>
        /// <returns>A <see cref="List{T}"/> of <see cref="MainNavigationComponentModel.NavigationNode"/>s.</returns>
        protected virtual async Task CreateStandardNavigationNodes()
        {
            #region ** Home **
            var home = TenantContext.IsDefault
                ? new MainNavigationComponentModel.NavigationNode
                {
                    Text = string.Empty,
                    Url =  UrlHelper.Action(nameof(League.Controllers.Home.Welcome), nameof(League.Controllers.Home),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue}),
                    IconCssClass = "fas fa-1x fa-home", Key = "Home_League"
                }
                : new MainNavigationComponentModel.NavigationNode
                {
                    Text = string.Empty, 
                    Url = "/" + TenantContext.SiteContext.UrlSegmentValue,
                    IconCssClass = "fas fa-1x fa-home", Key = "Home_Tenant"
                };
            #endregion
            
            #region ** Leagues **
            var leagues = new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = null,
                Key = "Top_Leagues",
                Text = Localizer["League"], Url = "/#" 
            };

            leagues.ChildNodes.Add(new MainNavigationComponentModel.NavigationNode
                {Text = Localizer["Home"], Url = "/welcome", Key = "League_Welcome"});
            
            foreach (var tenant in TenantStore.GetTenants().Values)
            {
                if (!string.IsNullOrEmpty(tenant.Identifier) && !tenant.SiteContext.HideInMenu)
                {
                    leagues.ChildNodes.Add(
                        new MainNavigationComponentModel.NavigationNode
                        {
                            ParentNode = leagues,
                            Key = "League_" + tenant.Identifier,
                            Text = tenant.OrganizationContext.ShortName,
                            Url = "/" + tenant.SiteContext.UrlSegmentValue,
                        });
                }
            }
            leagues.ChildNodes.Add(new MainNavigationComponentModel.NavigationNode { Text = Localizer["League Overview"], Url = "/overview", Key = "LeagueOverview" });
            #endregion
            
            #region ** Team Infos **

            var teamInfos = new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = null,
                Key = "Top_Teams",
                Text = Localizer["Teams"],
                Url = UrlHelper.Action(nameof(Team.Index), nameof(Team),
                    new {organization = TenantContext.SiteContext.UrlSegmentValue})
            };
            teamInfos.ChildNodes.AddRange(new []
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamInfos,
                    Key = "Teams_MyTeam",
                    Text = Localizer["My team"],
                    Url = UrlHelper.Action(nameof(Team.MyTeam), nameof(Team),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue}),
                    IsVisible = (await AuthorizationService.AuthorizeAsync(UserClaimsPrincipal, PolicyName.MyTeamAdminPolicy)).Succeeded
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamInfos,
                    Key = "Teams_Application",
                    Text = Localizer["Register team for next season"],
                    Url = UrlHelper.Action(nameof(TeamApplication.List), nameof(TeamApplication),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamInfos,
                    Key = "Teams_Contact",
                    Text = Localizer["Contact teams"],
                    Url = UrlHelper.Action(nameof(Team.List), nameof(Team),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamInfos,
                    Key = "Teams_Map",
                    Text = Localizer["Geographical spread"],
                    Url = UrlHelper.Action(nameof(Map.Index), nameof(Map),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                }
            });
            #endregion

            #region ** Match Overview **
            var teamOverview = new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = null,
                Key = "Top_Matches",
                Text = Localizer["Matches"],
                Url = UrlHelper.Action(nameof(Match.Index), nameof(Match),
                    new {organization = TenantContext.SiteContext.UrlSegmentValue})
            };
            teamOverview.ChildNodes.AddRange(new[]
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamOverview,
                    Key = "Matches_Fixtures",
                    Text = Localizer["Fixtures"],
                    Url = UrlHelper.Action(nameof(Match.Fixtures), nameof(Match),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = teamOverview,
                    Key = "Matches_Results",
                    Text = Localizer["Match results"],
                    Url = UrlHelper.Action(nameof(Match.Results), nameof(Match),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                }
            });
            #endregion
            
            #region ** Ranking tables **
            var rankingTables = new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = null,
                Key = "Top_Ranking",
                Text = Localizer["Tables"],
                Url = UrlHelper.Action(nameof(Ranking.Index), nameof(Ranking),
                    new {organization = TenantContext.SiteContext.UrlSegmentValue})
            };
            rankingTables.ChildNodes.AddRange(new[]
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = rankingTables,
                    Key = "Ranking_Table",
                    Text = Localizer["Current season"],
                    Url = UrlHelper.Action(nameof(Ranking.Table), nameof(Ranking),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = rankingTables,
                    Key = "Ranking_AllTimeTable",
                    Text = Localizer["All-time tables"],
                    Url = UrlHelper.Action(nameof(Ranking.AllTimeTournament), nameof(Ranking),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue})
                }
            });
            #endregion
            
            #region ** Account **

            MainNavigationComponentModel.NavigationNode account;

            if (UserClaimsPrincipal.Identity.IsAuthenticated)
            {
                account = new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = null,
                    Key = "Top_Account",
                    Text = string.Empty,
                    Url = UrlHelper.Action(nameof(Manage.Index), nameof(Manage),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue}),
                    IconCssClass = "fas fa-1x fa-user-check",
                    CssClass = "dropdown-menu-right"
                };
                account.ChildNodes.AddRange(new []
                {
                    new MainNavigationComponentModel.NavigationNode
                    {
                        ParentNode = account,
                        Key = "Account_Auth_Manage",
                        Text = Localizer["Manage account"],
                        Url = UrlHelper.Action(nameof(Manage.Index), nameof(Manage),
                            new {organization = TenantContext.SiteContext.UrlSegmentValue})
                    },
                    new MainNavigationComponentModel.NavigationNode
                    {
                        ParentNode = account,
                        Key = "Account_Auth_SignIn",
                        Text = Localizer["Sign in with other account"],
                        Url = UrlHelper.Action(nameof(Account.SignIn), nameof(Account),
                            new {organization = TenantContext.SiteContext.UrlSegmentValue})
                    },
                    new MainNavigationComponentModel.NavigationNode
                    {
                        ParentNode = account,
                        Key = "Account_Auth_SignOut",
                        Text = Localizer["Sign out"],
                        Url = UrlHelper.Action(nameof(Account.SignOut), nameof(Account),
                            new {organization = TenantContext.SiteContext.UrlSegmentValue})
                    }
                });
            }
            else
            {
                account = new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = null,
                    Key = "Top_Account",
                    Text = string.Empty,
                    Url = UrlHelper.Action(nameof(Account.SignIn), nameof(Account),
                        new {organization = TenantContext.SiteContext.UrlSegmentValue}),
                    IconCssClass = "fas fa-user-plus",
                    CssClass = "dropdown-menu-right"
                };
                account.ChildNodes.AddRange(new[]
                {
                    new MainNavigationComponentModel.NavigationNode
                    {
                        ParentNode = account,
                        Key = "Account_Anonymous_SignIn",
                        Text = Localizer["Sign in"],
                        Url = UrlHelper.Action(nameof(Account.SignIn), nameof(Account),
                            new {organization = TenantContext.SiteContext.UrlSegmentValue})
                    },
                    new MainNavigationComponentModel.NavigationNode
                    {
                        ParentNode = account,
                        Key = "Account_Anonymous_Create",
                        Text = Localizer["Create account"],
                        Url = UrlHelper.Action(nameof(Account.CreateAccount), nameof(Account),
                            new {organization = TenantContext.SiteContext.UrlSegmentValue})
                    }
                });
            }
            #endregion

            Logger.LogTrace($"League standard list of {nameof(MainNavigationComponentModel.NavigationNode)}s created.");

            if (TenantContext.IsDefault)
            {
                NavigationNodes.AddRange(new List<MainNavigationComponentModel.NavigationNode>(new[]
                {
                    home, leagues, 
                    new MainNavigationComponentModel.NavigationNode {Key = "RightAlignSeparator"}, 
                }));
            }
            else
            {
                NavigationNodes.AddRange(new List<MainNavigationComponentModel.NavigationNode>(new[]
                {
                    home, leagues, teamInfos, teamOverview, rankingTables,
                    new MainNavigationComponentModel.NavigationNode {Key = "RightAlignSeparator"}, 
                    account
                }));
            }
        }
    }
}
