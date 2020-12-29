using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using League.Components;
using League.DI;
using League.Helpers;
using League.Models.TeamViewModels;
using League.Models.UploadViewModels;
using League.TagHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.DI;
using TournamentManager.ModelValidators;

namespace League.Controllers
{
    [Route("{organization:MatchingTenant}/[controller]")]
    public class Team : AbstractController
    {
        private readonly SiteContext _siteContext;
        private readonly TournamentManager.MultiTenancy.AppDb _appDb;
        private readonly IStringLocalizer<Team> _localizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<Team> _logger;
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
        private readonly SignInManager<Identity.ApplicationUser> _signInManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public Team(SiteContext siteContext, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, IWebHostEnvironment webHostEnvironment, IStringLocalizer<Team> localizer, IAuthorizationService authorizationService, SignInManager<Identity.ApplicationUser> signInManger, ILogger<Team> logger)
        {
            _siteContext = siteContext;
            _timeZoneConverter = timeZoneConverter;
            _webHostEnvironment = webHostEnvironment;
            _appDb = siteContext.AppDb;
            _signInManager = signInManger;
            _localizer = localizer;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect(Url.Action(nameof(List), nameof(Team), new { Organization = _siteContext.UrlSegmentValue }));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var model = new TeamListModel(_timeZoneConverter)
            {
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.TeamTournamentId)
                    , cancellationToken),
                RoundsWithTeams = await _appDb.RoundRepository.GetRoundsWithTeamsAsync(
                    new PredicateExpression(RoundTeamFields.TournamentId == _siteContext.TeamTournamentId),
                    cancellationToken)
            };

            if (model.Tournament == null)
            {
                _logger.LogCritical($"{nameof(_siteContext.TeamTournamentId)} '{_siteContext.TeamTournamentId}' does not exist");
                return NotFound();
            }

            return View(ViewNames.Team.List, model);
        }

        [Authorize(nameof(Authorization.PolicyName.MyTeamPolicy))]
        [HttpGet("my-team/{id:long?}")]
        public async Task<IActionResult> MyTeam(long? id, CancellationToken cancellationToken)
        {
            // make sure that any changes are immediately reflected in the user's application cookie
            await _signInManager.RefreshSignInAsync(await _signInManager.UserManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier)));
            
            var userTeamIds = GetUserClaimTeamIds();
            // As admin, any selected team will be returned as "my team"
            if (id.HasValue && (await _authorizationService.AuthorizeAsync(User, Authorization.PolicyName.MyTeamAdminPolicy)).Succeeded)
            {
                userTeamIds.Clear();
                userTeamIds.Add(id.Value);
            }
            
            if (id.HasValue && !userTeamIds.Contains(id.Value))
            {
                return RedirectToAction(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, id = default(long?) });
            }

            var teamUserRoundInfos = await _appDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TournamentId == _siteContext.TeamTournamentId &
                                        (TeamUserRoundFields.IsManager == true | TeamUserRoundFields.IsPlayer == true))
                    .AddWithAnd(TeamUserRoundFields.TeamId.In(userTeamIds)),
                cancellationToken);

            if (teamUserRoundInfos.Count == 0)
            {
                return View(ViewNames.Team.MyTeamNotFound, await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.TeamTournamentId)
                    , cancellationToken));
            }

            var model = new MyTeamShowModel(_timeZoneConverter)
            {
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.TeamTournamentId)
                    , cancellationToken),
                TeamUserRoundInfos = teamUserRoundInfos,
                TeamVenueRoundInfos = await _appDb.TeamRepository.GetTeamVenueRoundInfoAsync(
                    new PredicateExpression(TeamVenueRoundFields.TournamentId == _siteContext.TeamTournamentId)
                        .AddWithAnd(TeamVenueRoundFields.TeamId.In(userTeamIds)), cancellationToken),
                ActiveTeamId = id ?? userTeamIds.First(),
                TeamPhotoStaticFile =
                    new TeamPhotoStaticFile(_webHostEnvironment, _siteContext, new NullLogger<TeamPhotoStaticFile>())
            };

            return View(ViewNames.Team.MyTeam, model);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Single(long id, CancellationToken cancellationToken)
        {
            var model = new TeamSingleModel(_timeZoneConverter)
            {
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.TeamTournamentId)
                    , cancellationToken),
                TeamVenueRoundInfo = (await _appDb.TeamRepository.GetTeamVenueRoundInfoAsync(
                        new PredicateExpression(TeamVenueRoundFields.TournamentId == _siteContext.TeamTournamentId &
                                                TeamVenueRoundFields.TeamId == id), cancellationToken))
                    .FirstOrDefault(),
                TeamUserRoundInfos = await _appDb.TeamRepository.GetTeamUserRoundInfosAsync(
                    new PredicateExpression(TeamUserRoundFields.TournamentId == _siteContext.TeamTournamentId &
                                            TeamUserRoundFields.TeamId == id & TeamUserRoundFields.IsManager == true), cancellationToken),
                ShowContactInfos = (await _authorizationService.AuthorizeAsync(User, Authorization.PolicyName.SeeTeamContactsPolicy)).Succeeded
            };

            if (model.TeamVenueRoundInfo == null)
            {
                return NotFound(id);
            }

            model.PhotoUriInfo = new TeamPhotoStaticFile(_webHostEnvironment, _siteContext, new NullLogger<TeamPhotoStaticFile>()).GetUriInfo(id);

            return View(ViewNames.Team.Single, model);
        }

        [HttpGet("[action]/{teamId:long}")]
        public async Task<IActionResult> Edit(long teamId, CancellationToken cancellationToken)
        {
            var team = await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == teamId), cancellationToken);
            if (team == null)
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(team.Id),
                Authorization.TeamOperations.EditTeam)).Succeeded)
            {
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new { ReturnUrl = Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue }) }));
            }

            var model = new TeamEditModel
            {
                Round = await GetRoundSelectorComponentModel(team, cancellationToken),
                Team = GetTeamEditorComponentModel(team)
            };

            return PartialView(ViewNames.Team._EditTeamModalPartial, model);
        }

        [HttpPost("[action]/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] TeamEditModel model, CancellationToken cancellationToken)
        {
            TeamEntity team = null;
            if (model.Team != null && !model.Team.IsNew)
            {
                team = await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == model.Team.Id), cancellationToken);
                if (team == null)
                {
                    return NotFound();
                }

                if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(team.Id),
                    Authorization.TeamOperations.EditTeam)).Succeeded)
                {
                    return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                        new {ReturnUrl = Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue })}));
                }

                team.TeamInRounds.AddRange(await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                    new PredicateExpression(RoundFields.TournamentId == _siteContext.TeamTournamentId & TeamInRoundFields.TeamId == team.Id),
                    cancellationToken));

                if (team.TeamInRounds.Count > 1)
                    _logger.LogError("Teams ID {0} belongs to {1} rounds for tournament ID {2}", team.Id,
                        team.TeamInRounds.Count, _siteContext.TeamTournamentId);
            }

            team ??= new TeamEntity();

            model.TeamEntity = team;
            model.Round = await GetRoundSelectorComponentModel(team, cancellationToken);
            model.Team = GetTeamEditorComponentModel(team);
            // sync input with new model instance
            if (!await TryUpdateModelAsync(model))
            {
                return PartialView(ViewNames.Team._EditTeamModalPartial, model);
            }

            model.MapFormFieldsToEntity();
            ModelState.Clear();

            if (!await model.ValidateAsync(new TeamValidator(model.TeamEntity, _siteContext), _siteContext.TeamTournamentId, ModelState, cancellationToken))
            {
                return PartialView(ViewNames.Team._EditTeamModalPartial, model);
            }

            try
            {
                if (await _appDb.GenericRepository.SaveEntityAsync(model.TeamEntity, false, true, cancellationToken))
                {
                    TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MyTeamMessageModel.MessageId.TeamDataSuccess });
                    return JsonAjaxRedirectForModal(Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, id = team.Id}));
                }
            }
            catch (Exception e)
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MyTeamMessageModel.MessageId.TeamDataFailure});
                _logger.LogCritical(e, "Error saving team id '{0}'", model.Team.IsNew ? "new" : model.Team.Id.ToString());
                return JsonAjaxRedirectForModal(Url.Action(nameof(MyTeam), new { Organization = _siteContext.UrlSegmentValue, id = team.Id }));
            }

            // We never should come this far
            return PartialView(ViewNames.Team._EditTeamModalPartial, model);
        }

        [HttpGet("[action]/{tid}")]
        public async Task<IActionResult> ChangeVenue(long tid, CancellationToken cancellationToken)
        {
            var teamEntity = await
                _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == tid),
                    cancellationToken);
            if (teamEntity == null)
            {
                return NotFound();
            }

            return PartialView(ViewNames.Team._ChangeVenueModalPartial, (TeamId: teamEntity.Id, VenueId: teamEntity.VenueId));
        }

        [HttpGet("[action]/{tid}")]
        public async Task<IActionResult> SelectVenue(long tid, string returnUrl, CancellationToken cancellationToken)
        {
            var teamEntity = await
                _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == tid),
                    cancellationToken);
            if (teamEntity == null)
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamEntity.Id),
                Authorization.TeamOperations.EditTeam)).Succeeded)
            {
                return RedirectToAction(nameof(Error.AccessDenied), nameof(Error),
                    new { ReturnUrl = Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, tid}) });
            }

            return PartialView(ViewNames.Team._SelectVenueModalPartial,
                new TeamVenueSelectModel
                {
                    TournamentId = _siteContext.TeamTournamentId, TeamId = teamEntity.Id, VenueId = teamEntity.VenueId,
                    ReturnUrl = Url.IsLocalUrl(returnUrl)
                        ? returnUrl
                        : Url.Action(nameof(MyTeam), nameof(Team), new {Organization = _siteContext.UrlSegmentValue})
                }
            );
        }

        [HttpPost("[action]/{*segments}")]
        public async Task<IActionResult> SelectVenue([FromForm][Bind("TeamId, VenueId")] TeamVenueSelectModel model, CancellationToken cancellationToken)
        {
            // model binding is not case-sensitive

            var teamEntity = await
                _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == model.TeamId),
                    cancellationToken);

            if (teamEntity == null || !model.VenueId.HasValue ||
                !await _appDb.VenueRepository.IsValidVenueIdAsync(model.VenueId, cancellationToken))
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamEntity.Id),
                Authorization.TeamOperations.EditTeam)).Succeeded)
            {
                return JsonAjaxRedirectForModal(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new { ReturnUrl = Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, model.TeamId }) }));
            }

            model.TournamentId = _siteContext.TeamTournamentId;

            if (!ModelState.IsValid)
            {
                return PartialView(ViewNames.Team._SelectVenueModalPartial, model);
            }
            
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MyTeamMessageModel.MessageId.TeamDataFailure});
            try
            {
                teamEntity.VenueId = model.VenueId;
                if(await _appDb.GenericRepository.SaveEntityAsync(teamEntity, false, false, cancellationToken))
                {
                    TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MyTeamMessageModel.MessageId.VenueCreateSuccess});
                }
                else
                {
                    throw new Exception("Failed to save team entity");
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to save selected venue for team id {0}, venue id {1}", model.TeamId, model.VenueId);
            }
            
            return JsonAjaxRedirectForModal(Url.Action(nameof(MyTeam), nameof(Team), new { Organization = _siteContext.UrlSegmentValue, model.TeamId }));
        }

        private IList<long> GetUserClaimTeamIds()
        {
            var teamIds = new HashSet<long>();
            if (User.Claims == null)
            {
                return teamIds.ToList();
            }
            
            foreach (var claim in User.Claims.Where(c => c.Type == Identity.Constants.ClaimType.ManagesTeam || c.Type == Identity.Constants.ClaimType.PlaysInTeam))
            {
                if(!long.TryParse(claim.Value, out var teamId)) continue;
                teamIds.Add(teamId);
            }

            return teamIds.ToList();
        }

        private async Task<RoundSelectorComponentModel> GetRoundSelectorComponentModel(TeamEntity team, CancellationToken cancellationToken)
        {
            var tir = (await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                    new PredicateExpression(TeamInRoundFields.TeamId == team.Id &
                                            RoundFields.TournamentId == _siteContext.TeamTournamentId), cancellationToken))
                .FirstOrDefault();
            return new RoundSelectorComponentModel
            {
                RoundNotSpecifiedKey = null,
                SelectedRoundId = tir?.RoundId,
                ShowSelector = await _appDb.MatchRepository.GetMatchCountAsync(
                    new PredicateExpression(
                        RoundFields.TournamentId == _siteContext.TeamTournamentId),
                    cancellationToken) == 0,
                TournamentId = _siteContext.TeamTournamentId,
                HtmlFieldPrefix = nameof(TeamEditModel.Round)
            };
        }

        private TeamEditorComponentModel GetTeamEditorComponentModel(TeamEntity team)
        {
            var model = new TeamEditorComponentModel();
            model.MapEntityToFormFields(team);
            model.HtmlFieldPrefix = nameof(TeamEditModel.Team);
            return model;
        }
    }
}