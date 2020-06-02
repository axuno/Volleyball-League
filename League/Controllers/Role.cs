using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.DI;
using League.Helpers;
using League.Models.RoleViewModels;
using League.Models.TeamApplicationViewModels;
using League.Models.TeamViewModels;
using League.TagHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using TournamentManager.DAL.EntityClasses;

namespace League.Controllers
{
    [Route("{organization:ValidOrganizations}/[controller]")]
    public class Role : AbstractController
    {
        private readonly SiteContext _siteContext;
        private readonly Microsoft.AspNetCore.Identity.SignInManager<Identity.ApplicationUser> _signInManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IStringLocalizer<Role> _localizer;
        private readonly ILogger<Role> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private const string _defaultReturnUrl = "/";

        public Role(SiteContext siteContext,
            Microsoft.AspNetCore.Identity.SignInManager<Identity.ApplicationUser> signInManager,
            IAuthorizationService authorizationService, IStringLocalizer<Role> localizer,
            ILoggerFactory loggerFactory)
        {
            _siteContext = siteContext;
            _signInManager = signInManager;
            _authorizationService = authorizationService;
            _localizer = localizer;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<Role>();
        }

        [HttpGet("[action]/{roleName}/{uid:long}/{tid:long}")]
        public async Task<IActionResult> Remove(string roleName, long uid, long tid, string un,
            string returnUrl, CancellationToken cancellationToken)
        {
            var model = new RoleRemoveModel
            {
                TeamId = tid,
                UserId = uid,
                UserName = un,
                ClaimType = Identity.Constants.ClaimType.ManagesTeam.ToLowerInvariant() == roleName?.ToLowerInvariant()
                    ? Identity.Constants.ClaimType.ManagesTeam
                    : Identity.Constants.ClaimType.PlaysInTeam,
                ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : _defaultReturnUrl
            };

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.RemoveTeamMember)).Succeeded)
            {
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new {model.ReturnUrl}));
            }

            return PartialView(ViewNames.Role._RemoveMemberModalPartial, model);
        }

        [HttpPost("[action]/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove([FromForm] RoleRemoveModel model, CancellationToken cancellationToken)
        {
            model.ClaimType = Identity.Constants.ClaimType.ManagesTeam.ToLowerInvariant() ==
                             model.ClaimType?.ToLowerInvariant()
                ? Identity.Constants.ClaimType.ManagesTeam
                : Identity.Constants.ClaimType.PlaysInTeam;
            model.ReturnUrl ??= _defaultReturnUrl;

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.RemoveTeamMember)).Succeeded)
            {
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new {model.ReturnUrl}));
            }

            if (model.ClaimType == Identity.Constants.ClaimType.ManagesTeam &&
                (await _siteContext.AppDb.ManagerOfTeamRepository.GetManagerIdsOfTeamAsync(model.TeamId,
                    cancellationToken)).Count <= 1)
            {
                _logger.LogInformation("Rejected to remove last claim '{0}' for team id '{1}' and user id {2}",
                    model.ClaimType, model.TeamId, model.UserId);
                return JsonAjaxRedirectForModal(SetCannotRemoveLastTeamManagerReturnResult(model.ReturnUrl, model.TeamId));
            }

            var removeTeamMember = await _signInManager.UserManager.FindByIdAsync(model.UserId.ToString());
            if (removeTeamMember != null)
            {
                await _signInManager.UserManager.RemoveClaimAsync(removeTeamMember, new Claim(model.ClaimType, model.TeamId.ToString()));
                try
                {
                    var result = await _signInManager.UserManager.UpdateAsync(removeTeamMember);
                    if (result != IdentityResult.Success) throw new Exception($"Updating user id '{removeTeamMember.Id}'");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to remove claim '{0}' for team id '{1}' and user id {2}", model.ClaimType, model.TeamId, model.UserId);
                    return JsonAjaxRedirectForModal(SetAdjustedReturnResult(nameof(Remove), model.ReturnUrl, model.TeamId, false));
                }
            }

            return JsonAjaxRedirectForModal(SetAdjustedReturnResult(nameof(Remove), model.ReturnUrl, model.TeamId, true));
        }

        [HttpGet("[action]/{tid:long}")]
        public async Task<IActionResult> Add(long tid, string returnUrl, CancellationToken cancellationToken)
        {
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
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new {model.ReturnUrl}));
            }

            return PartialView(ViewNames.Role._AddMemberModalPartial, model);
        }

        [HttpPost("[action]/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add([FromForm] RoleAddModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return PartialView(ViewNames.Role._AddMemberModalPartial, model);
            }

            model.ClaimType = Identity.Constants.ClaimType.ManagesTeam.ToLowerInvariant() ==
                             model.ClaimType?.ToLowerInvariant()
                ? Identity.Constants.ClaimType.ManagesTeam
                : Identity.Constants.ClaimType.PlaysInTeam;
            model.ReturnUrl ??= _defaultReturnUrl;

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(model.TeamId),
                Authorization.TeamOperations.AddTeamMember)).Succeeded)
            {
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new { model.ReturnUrl }));
            }

            var newTeamMember = await _signInManager.UserManager.FindByEmailAsync(model.UserEmail);
            if (newTeamMember != null)
            {
                await _signInManager.UserManager.AddClaimAsync(newTeamMember, new Claim(model.ClaimType, model.TeamId.ToString()));
                try
                {
                    var result = await _signInManager.UserManager.UpdateAsync(newTeamMember);
                    if(result != IdentityResult.Success) throw new Exception($"Updating user id '{newTeamMember.Id}'");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to add claim '{0}' for team id '{1}' and user id '{2}'", model.ClaimType, model.TeamId, newTeamMember.Id);
                }
            }
            else
            {
                ModelState.AddModelError(nameof(model.UserEmail), _localizer["No user found with this e-mail"]);
            }

            if (!ModelState.IsValid)
            {
                return PartialView(ViewNames.Role._AddMemberModalPartial, model);
            }

            return JsonAjaxRedirectForModal(SetAdjustedReturnResult(nameof(Add), model.ReturnUrl, model.TeamId, true));
        }

        private string SetAdjustedReturnResult(string method, string returnUrl, long teamId, bool isSuccess)
        {
            if (method.Equals(nameof(Add)) && returnUrl.Contains(Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue })))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                    new MyTeamMessageModel.MyTeamMessage
                    {
                        AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                        MessageId = isSuccess ? MyTeamMessageModel.MessageId.MemberAddSuccess : MyTeamMessageModel.MessageId.MemberAddFailure
                    });

                return Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, id = teamId });
            }

            if (method.Equals(nameof(Remove)) && returnUrl.Contains(Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue })))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                    new MyTeamMessageModel.MyTeamMessage
                    {
                        AlertType = isSuccess ? SiteAlertTagHelper.AlertType.Success : SiteAlertTagHelper.AlertType.Danger,
                        MessageId = isSuccess ? MyTeamMessageModel.MessageId.MemberRemoveSuccess : MyTeamMessageModel.MessageId.MemberRemoveFailure
                    });

                return Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, id = teamId});
            }

            return returnUrl;
        }

        private string SetCannotRemoveLastTeamManagerReturnResult(string returnUrl, long teamId)
        {
            if (returnUrl.Contains(Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue })))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage),
                    new MyTeamMessageModel.MyTeamMessage
                    {
                        AlertType = SiteAlertTagHelper.AlertType.Danger,
                        MessageId = MyTeamMessageModel.MessageId.MemberCannotRemoveLastTeamManager
                    });

                return Url.Action(nameof(Team.MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, id = teamId });
            }

            return returnUrl;
        }
    }
}