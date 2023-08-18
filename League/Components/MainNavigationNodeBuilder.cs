using System.Security.Claims;
using League.Authorization;
using League.Controllers;
using League.MultiTenancy;
using TournamentManager.MultiTenancy;

namespace League.Components;

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
    /// Gets the <see cref="MultiTenancy.TenantLink"/> generator.
    /// </summary>
    protected readonly TenantLink TenantLink;
        
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
    /// <param name="tenantLink"></param>
    /// <param name="localizer"></param>
    /// <param name="logger"></param>
    public MainNavigationNodeBuilder(TenantStore tenantStore, ITenantContext tenantContext, IAuthorizationService authorizationService, TenantLink tenantLink, IStringLocalizer<MainNavigationNodeBuilder> localizer, ILogger<MainNavigationNodeBuilder> logger)
    {
        TenantStore = tenantStore;
        TenantContext = tenantContext;
        UserClaimsPrincipal = tenantLink.HttpContext.User;
        AuthorizationService = authorizationService;
        TenantLink = tenantLink;
        HttpContext = tenantLink.HttpContext;
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
        #region ** Leagues **
        var leagues = new MainNavigationComponentModel.NavigationNode
        {
            ParentNode = null,
            Key = "Top_Leagues",
            Text = Localizer["League"], Url = "/#" 
        };

        leagues.ChildNodes.Add(new MainNavigationComponentModel.NavigationNode
            {Text = Localizer["Home"], Url = "/welcome", Key = "League_Welcome"});

        leagues.ChildNodes.Add(new MainNavigationComponentModel.NavigationNode
            { Text = Localizer["League Overview"], Url = "/overview", Key = "League_Overview" });

        leagues.ChildNodes.Add(new MainNavigationComponentModel.NavigationNode
            { Text = "Separator", Key = "League_Separator" });

        foreach (var tenant in TenantStore.GetTenants().Values.OrderBy(t => t.SiteContext.Position))
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
        
        #endregion
            
        #region ** Team Infos **

        var teamInfos = new MainNavigationComponentModel.NavigationNode
        {
            ParentNode = null,
            Key = "Top_Teams",
            Text = Localizer["Teams"],
            Url = TenantLink.Action(nameof(Team.Index), nameof(Team))
        };
        teamInfos.ChildNodes.AddRange(new []
        {
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamInfos,
                Key = "Teams_MyTeam",
                Text = Localizer["My team"],
                Url = TenantLink.Action(nameof(Team.MyTeam), nameof(Team)),
                IsVisible = (await AuthorizationService.AuthorizeAsync(UserClaimsPrincipal, PolicyName.MyTeamPolicy)).Succeeded
                    || (await AuthorizationService.AuthorizeAsync(UserClaimsPrincipal, PolicyName.MyTeamAdminPolicy)).Succeeded
            },
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamInfos,
                Key = "Teams_Application",
                Text = Localizer["Register team for next season"],
                Url = TenantLink.Action(nameof(TeamApplication.List), nameof(TeamApplication))
            },
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamInfos,
                Key = "Teams_Contact",
                Text = Localizer["Contact teams"],
                Url = TenantLink.Action(nameof(Team.List), nameof(Team))
            },
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamInfos,
                Key = "Teams_Map",
                Text = Localizer["Geographical spread"],
                Url = TenantLink.Action(nameof(Map.Index), nameof(Map))
            }
        });
        #endregion

        #region ** Match Overview **
        var teamOverview = new MainNavigationComponentModel.NavigationNode
        {
            ParentNode = null,
            Key = "Top_Matches",
            Text = Localizer["Matches"],
            Url = TenantLink.Action(nameof(Match.Index), nameof(Match))
        };
        teamOverview.ChildNodes.AddRange(new[]
        {
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamOverview,
                Key = "Matches_Fixtures",
                Text = Localizer["Fixtures"],
                Url = TenantLink.Action(nameof(Match.Fixtures), nameof(Match))
            },
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = teamOverview,
                Key = "Matches_Results",
                Text = Localizer["Match results"],
                Url = TenantLink.Action(nameof(Match.Results), nameof(Match))
            }
        });
        #endregion
            
        #region ** Ranking tables **
        var rankingTables = new MainNavigationComponentModel.NavigationNode
        {
            ParentNode = null,
            Key = "Top_Ranking",
            Text = Localizer["Tables"],
            Url = TenantLink.Action(nameof(Ranking.Index), nameof(Ranking))
        };
        rankingTables.ChildNodes.AddRange(new[]
        {
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = rankingTables,
                Key = "Ranking_Table",
                Text = Localizer["Current season"],
                Url = TenantLink.Action(nameof(Ranking.Table), nameof(Ranking))
            },
            new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = rankingTables,
                Key = "Ranking_AllTimeTable",
                Text = Localizer["All-time tables"],
                Url = TenantLink.Action(nameof(Ranking.AllTimeTournament), nameof(Ranking))
            }
        });
        #endregion
            
        #region ** Account **

        MainNavigationComponentModel.NavigationNode account;

        if (UserClaimsPrincipal.Identity is { IsAuthenticated: true })
        {
            account = new MainNavigationComponentModel.NavigationNode
            {
                ParentNode = null,
                Key = "Top_Account",
                Text = string.Empty,
                Url = TenantLink.Action(nameof(Manage.Index), nameof(Manage)),
                IconCssClass = "fas fa-1x fa-user-check",
                CssClass = "dropdown-menu-end"
            };
            account.ChildNodes.AddRange(new []
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = account,
                    Key = "Account_Auth_Manage",
                    Text = Localizer["Manage account"],
                    Url = TenantLink.Action(nameof(Manage.Index), nameof(Manage))
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = account,
                    Key = "Account_Auth_SignIn",
                    Text = Localizer["Sign in with other account"],
                    Url = TenantLink.Action(nameof(Account.SignIn), nameof(Account))
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = account,
                    Key = "Account_Auth_SignOut",
                    Text = Localizer["Sign out"],
                    Url = TenantLink.Action(nameof(Account.SignOut), nameof(Account))
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
                Url = TenantLink.Action(nameof(Account.SignIn), nameof(Account)),
                IconCssClass = "fas fa-user-plus",
                CssClass = "dropdown-menu-end"
            };
            account.ChildNodes.AddRange(new[]
            {
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = account,
                    Key = "Account_Anonymous_SignIn",
                    Text = Localizer["Sign in"],
                    Url = TenantLink.Action(nameof(Account.SignIn), nameof(Account))
                },
                new MainNavigationComponentModel.NavigationNode
                {
                    ParentNode = account,
                    Key = "Account_Anonymous_Create",
                    Text = Localizer["Create account"],
                    Url = TenantLink.Action(nameof(Account.CreateAccount), nameof(Account))
                }
            });
        }
        #endregion

        Logger.LogTrace($"League standard list of {nameof(MainNavigationComponentModel.NavigationNode)}s created.");

        if (TenantContext.IsDefault)
        {
            NavigationNodes.AddRange(new List<MainNavigationComponentModel.NavigationNode>(new[]
            {
                leagues, 
                new MainNavigationComponentModel.NavigationNode {Key = "RightAlignSeparator"}, 
            }));
        }
        else
        {
            NavigationNodes.AddRange(new List<MainNavigationComponentModel.NavigationNode>(new[]
            {
                leagues, teamInfos, teamOverview, rankingTables,
                new MainNavigationComponentModel.NavigationNode {Key = "RightAlignSeparator"}, 
                account
            }));
        }
    }
}
