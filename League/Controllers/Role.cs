using System.Security.Claims;
using League.Helpers;
using League.Models.RoleViewModels;
using League.Models.TeamViewModels;
using League.MultiTenancy;
using League.Routing;
using League.TagHelpers;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Role : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly Microsoft.AspNetCore.Identity.SignInManager<Identity.ApplicationUser> _signInManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IStringLocalizer<Role> _localizer;
    private readonly ILogger<Role> _logger;

    private const string _defaultReturnUrl = "/";

    public Role(ITenantContext tenantContext,
        Microsoft.AspNetCore.Identity.SignInManager<Identity.ApplicationUser> signInManager,
        IAuthorizationService authorizationService, IStringLocalizer<Role> localizer,
        ILoggerFactory loggerFactory)
    {
        _tenantContext = tenantContext;
        _signInManager = signInManager;
        _authorizationService = authorizationService;
        _localizer = localizer;
        _logger = loggerFactory.CreateLogger<Role>();
    }

    [HttpGet("[action]/{roleName}/{uid:long}/{tid:long}")]
    public async Task<IActionResult> Remove(string roleName, long uid, long tid, string un,
        string returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest();

        var model = new RoleRemoveModel
        {
            TeamId = tid,
            UserId = uid,
            UserName = un,
            ClaimType = Identity.Constants.ClaimType.ManagesTeam.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)
                ? Identity.Constants.ClaimType.ManagesTeam
                : Identity.Constants.ClaimType.PlaysInTeam,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : _defaultReturnUrl
        };

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.RemoveTeamMember)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new {model.ReturnUrl}));
        }

        return PartialView(Views.ViewNames.Role._RemoveMemberModalPartial, model);
    }

    [HttpPost("[action]/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove([FromForm] RoleRemoveModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
            new { _defaultReturnUrl }));

        model.ClaimType = Identity.Constants.ClaimType.ManagesTeam.ToLowerInvariant() ==
                          model.ClaimType?.ToLowerInvariant()
            ? Identity.Constants.ClaimType.ManagesTeam
            : Identity.Constants.ClaimType.PlaysInTeam;
        model.ReturnUrl ??= _defaultReturnUrl;

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.RemoveTeamMember)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new {model.ReturnUrl}));
        }

        if (model.ClaimType == Identity.Constants.ClaimType.ManagesTeam &&
            (await _tenantContext.DbContext.AppDb.ManagerOfTeamRepository.GetManagerIdsOfTeamAsync(model.TeamId,
                cancellationToken)).Count <= 1)
        {
            _logger.LogInformation("Rejected to remove last claim '{ClaimType}' for team id '{TeamId}' and user id {UserId}",
                model.ClaimType, model.TeamId, model.UserId);
            return JsonResponseRedirect(SetCannotRemoveLastTeamManagerReturnResult(model.ReturnUrl, model.TeamId));
        }

        var removeTeamMember = await _signInManager.UserManager.FindByIdAsync(model.UserId.ToString());
        if (removeTeamMember != null)
        {
            await _signInManager.UserManager.RemoveClaimAsync(removeTeamMember, new(model.ClaimType, model.TeamId.ToString()));
            try
            {
                var result = await _signInManager.UserManager.UpdateAsync(removeTeamMember);
                if (result != IdentityResult.Success) throw new InvalidOperationException($"Updating user id '{removeTeamMember.Id}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to remove claim '{ClaimType}' for team id '{TeamId}' and user id {UserId}", model.ClaimType, model.TeamId, model.UserId);
                return JsonResponseRedirect(SetAdjustedReturnResult(nameof(Remove), model.ReturnUrl, model.TeamId, false));
            }
        }

        return JsonResponseRedirect(SetAdjustedReturnResult(nameof(Remove), model.ReturnUrl, model.TeamId, true));
    }

    [HttpGet("[action]/{tid:long}")]
    public async Task<IActionResult> Add(long tid, string returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
            new { _defaultReturnUrl }));

        var model = new RoleAddModel
        {
            TeamId = tid,
            UserEmail = string.Empty,
            ClaimType = Identity.Constants.ClaimType.PlaysInTeam,
            ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : _defaultReturnUrl
        };

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.AddTeamMember)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new {model.ReturnUrl}));
        }

        return PartialView(Views.ViewNames.Role._AddMemberModalPartial, model);
    }

    [HttpPost("[action]/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add([FromForm] RoleAddModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return PartialView(Views.ViewNames.Role._AddMemberModalPartial, model);
        }

        model.ClaimType = Identity.Constants.ClaimType.ManagesTeam.ToLowerInvariant() ==
                          model.ClaimType?.ToLowerInvariant()
            ? Identity.Constants.ClaimType.ManagesTeam
            : Identity.Constants.ClaimType.PlaysInTeam;
        model.ReturnUrl ??= _defaultReturnUrl;

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.AddTeamMember)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new { model.ReturnUrl }));
        }

        var newTeamMember = model.UserEmail != null ? await _signInManager.UserManager.FindByEmailAsync(model.UserEmail) : null;
        if (newTeamMember != null)
        {
            await _signInManager.UserManager.AddClaimAsync(newTeamMember, new(model.ClaimType, model.TeamId.ToString()));
            try
            {
                var result = await _signInManager.UserManager.UpdateAsync(newTeamMember);
                if(result != IdentityResult.Success) throw new InvalidOperationException($"Updating user id '{newTeamMember.Id}'");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to add claim '{ClaimType}' for team id '{TeamId}' and user id '{UserId}'", model.ClaimType, model.TeamId, newTeamMember.Id);
            }
        }
        else
        {
            ModelState.AddModelError(nameof(model.UserEmail), _localizer["No user found with this e-mail"]);
        }

        if (!ModelState.IsValid)
        {
            return PartialView(Views.ViewNames.Role._AddMemberModalPartial, model);
        }

        return JsonResponseRedirect(SetAdjustedReturnResult(nameof(Add), model.ReturnUrl, model.TeamId, true));
    }

    private string SetAdjustedReturnResult(string method, string returnUrl, long teamId, bool isSuccess)
    {
        if (method.Equals(nameof(Add)) && returnUrl.Contains(TenantLink.Action(nameof(Team.MyTeam), nameof(Team)) ?? string.Empty))
        {
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                new()
                {
                    AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                    MessageId = isSuccess ? MyTeamMessageModel.MessageId.MemberAddSuccess : MyTeamMessageModel.MessageId.MemberAddFailure
                });

            return TenantLink.Action(nameof(Team.MyTeam), nameof(Team), new { id = teamId }) ?? string.Empty;
        }

        if (method.Equals(nameof(Remove)) && returnUrl.Contains(TenantLink.Action(nameof(Team.MyTeam), nameof(Team)) ?? string.Empty))
        {
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                new()
                {
                    AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                    MessageId = isSuccess ? MyTeamMessageModel.MessageId.MemberRemoveSuccess : MyTeamMessageModel.MessageId.MemberRemoveFailure
                });

            return TenantLink.Action(nameof(Team.MyTeam), nameof(Team), new { id = teamId }) ?? string.Empty;
        }

        return returnUrl;
    }

    private string SetCannotRemoveLastTeamManagerReturnResult(string returnUrl, long teamId)
    {
        if (returnUrl.Contains(TenantLink.Action(nameof(Team.MyTeam), nameof(Team)) ?? string.Empty))
        {
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                new()
                {
                    AlertType = SiteAlertTagHelper.AlertType.Danger,
                    MessageId = MyTeamMessageModel.MessageId.MemberCannotRemoveLastTeamManager
                });

            return TenantLink.Action(nameof(Team.MyTeam), nameof(Team), new { id = teamId }) ?? string.Empty;
        }

        return returnUrl;
    }
}
