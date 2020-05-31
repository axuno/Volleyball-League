using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axuno.Tools.GeoSpatial;
using League.BackgroundTasks.Email;
using League.Components;
using League.ConfigurationPoco;
using League.DI;
using League.Helpers;
using League.Models.TeamApplicationViewModels;
using League.Models.TeamViewModels;
using League.Models.VenueViewModels;
using League.TagHelpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;

namespace League.Controllers
{
    [Route("{organization:ValidOrganizations}/team-application")]
    [ControllerAttributes.TeamApplicationAllowed] // if not allowed, all controller actions are redirected to action "list"
    [Authorize]
    public class TeamApplication : AbstractController
    {
        private readonly SiteContext _siteContext;
        private readonly AppDb _appDb;
        private readonly IStringLocalizer<TeamApplication> _localizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<TeamApplication> _logger;
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
        private readonly RegionInfo _regionInfo;
        private readonly GoogleConfiguration _googleConfig;
        private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
        private readonly TeamApplicationEmailTask _teamApplicationEmailTask;
        private const string TeamApplicationSessionName = "TeamApplicationSession";

        public TeamApplication(SiteContext siteContext,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
            IStringLocalizer<TeamApplication> localizer, IAuthorizationService authorizationService,
            RegionInfo regionInfo,
            IConfiguration configuration, Axuno.BackgroundTask.IBackgroundQueue queue,
            TeamApplicationEmailTask teamApplicationEmailTask, ILogger<TeamApplication> logger)
        {
            _siteContext = siteContext;
            _timeZoneConverter = timeZoneConverter;
            _regionInfo = regionInfo;
            _googleConfig = new GoogleConfiguration();
            configuration.Bind(nameof(GoogleConfiguration), _googleConfig);
            _queue = queue;
            _teamApplicationEmailTask = teamApplicationEmailTask;
            _appDb = siteContext.AppDb;
            _localizer = localizer;
            _authorizationService = authorizationService;
            _logger = logger;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect(Url.Action(nameof(List), nameof(TeamApplication), new { Organization = _siteContext.UrlSegmentValue }));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> List(CancellationToken cancellationToken)
        {
            var model = new ApplicationListModel(_timeZoneConverter)
            {
                Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.ApplicationTournamentId), 
                    cancellationToken),
                TournamentRoundTeams = await _appDb.TeamRepository.GetLatestTeamTournamentAsync(new PredicateExpression(LatestTeamTournamentFields.TournamentId == _siteContext.ApplicationTournamentId), cancellationToken)
            };
            var tir = await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                new PredicateExpression(RoundFields.Id.In(model.TournamentRoundTeams.Select(rt => rt.RoundId).Distinct())),
                cancellationToken);
            tir.ToList().ForEach(t => model.TeamRegisteredOn.Add(t.TeamId, t.ModifiedOn));

            if (model.Tournament == null)
            {
                _logger.LogCritical($"{nameof(_siteContext.ApplicationTournamentId)} '{_siteContext.ApplicationTournamentId}' does not exist");
                return NotFound();
            }

            return View(ViewNames.TeamApplication.List, model);
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Start(CancellationToken cancellationToken)
        {
            var sessionModel = await GetNewSessionModel(cancellationToken);
            SaveModelToSession(sessionModel);

            return View(ViewNames.TeamApplication.Start, sessionModel);
        }
        
        [HttpGet("select-team")]
        public async Task<IActionResult> SelectTeam(CancellationToken cancellationToken)
        {
            var teamSelectModel = await GetTeamSelectModel(cancellationToken);

            var sessionModel = await GetModelFromSession(cancellationToken);
            if (sessionModel.IsFromSession)
            {
                teamSelectModel.SelectedTeamId = sessionModel.Team.IsNew ? default : sessionModel.Team.Id;
            }
            SaveModelToSession(sessionModel);

            if (teamSelectModel.TeamsManagedByUser.Any())
            {
                return View(ViewNames.TeamApplication.SelectTeam, teamSelectModel);
            }

            // No existing team can be selected, so prepare to enter a new one
            sessionModel.Team.IsNew = true;
            return RedirectToAction(nameof(EditTeam), new { Organization = _siteContext.UrlSegmentValue });
        }

        [HttpPost("select-team/{*segments}")]
        public async Task<IActionResult> SelectTeam([FromForm] ApplicationSelectTeamModel selectTeamModel, CancellationToken cancellationToken)
        {
            selectTeamModel = await GetTeamSelectModel(cancellationToken);
            if (!await TryUpdateModelAsync(selectTeamModel))
            {
                return View(ViewNames.TeamApplication.SelectTeam, selectTeamModel);
            }

            // The SelectedTeamId is a required field => must have a value now
            Debug.Assert(selectTeamModel.SelectedTeamId != null, "selectTeamModel.SelectedTeamId != null");

            var sessionModel = await GetModelFromSession(cancellationToken);
            if (sessionModel.TeamInRoundIsSet && sessionModel.TeamIsSet &&
                sessionModel.Team.Id != selectTeamModel.SelectedTeamId)
            {
                sessionModel.TeamInRoundIsSet = false;
                sessionModel.TeamIsSet = false;
            }
            // The full team data is not completed yet
            sessionModel.Team.Id = selectTeamModel.SelectedTeamId.Value;
            sessionModel.Team.IsNew = false;
            
            SaveModelToSession(sessionModel);
            return RedirectToAction(nameof(EditTeam), new { Organization = _siteContext.UrlSegmentValue });
        }

        [HttpGet("edit-team/{teamId:long}")]
        public async Task<IActionResult> EditTeam(long teamId, CancellationToken cancellationToken)
        {
            var teamSelectModel = await GetTeamSelectModel(cancellationToken);
            if (teamSelectModel.TeamsManagedByUser.All(t => t.TeamId != teamId))
            {
                return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            }

            var sessionModel = await GetNewSessionModel(cancellationToken);
            sessionModel.Team.Id = teamId;
            sessionModel.Team.IsNew = false;
            SaveModelToSession(sessionModel);
            return RedirectToAction(nameof(EditTeam), new { Organization = _siteContext.UrlSegmentValue, teamId = string.Empty });
        }

        [HttpGet("edit-team")]
        public async Task<IActionResult> EditTeam(bool? isNew, CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            if (isNew.HasValue && isNew.Value)
            {
                sessionModel.Team = GetTeamEditorComponentModel(new TeamEntity());
                sessionModel.TeamIsSet = false;
                sessionModel.TeamInRoundIsSet = false;
            }
            
            var teamEntity = new TeamEntity();
            // Team.IsNew and Team.Id were set in step "select team"
            if (!sessionModel.Team.IsNew && !sessionModel.TeamIsSet)
            {
                teamEntity = await _appDb.TeamRepository.GetTeamEntityAsync(
                    new PredicateExpression(TeamFields.Id == sessionModel.Team.Id),
                    cancellationToken);
                if (teamEntity == null) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
                sessionModel.Team.MapEntityToFormFields(teamEntity);
            }

            if (!teamEntity.IsNew)
            {
                if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamEntity.Id),
                    Authorization.TeamOperations.SignUpForSeason)).Succeeded)
                {
                    return RedirectToAction(nameof(Error.AccessDenied), nameof(Error),
                        new { ReturnUrl = Url.Action(nameof(EditTeam), nameof(TeamApplication), new { Organization = _siteContext.UrlSegmentValue }) });
                }
            }

            var teamEditModel = new TeamEditModel
            {
                Round = await GetRoundSelectorComponentModel(teamEntity, cancellationToken),
                Team = GetTeamEditorComponentModel(teamEntity)
            };

            if(sessionModel.TeamInRoundIsSet) teamEditModel.Round.SelectedRoundId = sessionModel.TeamInRound.RoundId;
            if (sessionModel.TeamIsSet) teamEditModel.Team = sessionModel.Team;
            
            return View(ViewNames.TeamApplication.EditTeam, teamEditModel);
        }

        [HttpPost("edit-team/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditTeam([FromForm] TeamEditModel teamEditModel, CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            TeamEntity teamEntity = null;
            if (teamEditModel.Team != null && !teamEditModel.Team.IsNew)
            {
                teamEntity = await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == teamEditModel.Team.Id), cancellationToken);
                if (teamEntity == null)
                {
                    return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
                }

                if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamEntity.Id),
                    Authorization.TeamOperations.SignUpForSeason)).Succeeded)
                {
                    return RedirectToAction(nameof(Error.AccessDenied), nameof(Error),
                        new { ReturnUrl = Url.Action(nameof(EditTeam), nameof(TeamApplication), new { Organization = _siteContext.UrlSegmentValue }) });
                }

                teamEntity.TeamInRounds.AddRange(await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                    new PredicateExpression(RoundFields.TournamentId == _siteContext.ApplicationTournamentId & TeamInRoundFields.TeamId == teamEntity.Id),
                    cancellationToken));

                if (teamEntity.TeamInRounds.Count > 1)
                    _logger.LogError("Teams ID {0} belongs to {1} rounds for tournament ID {2}", teamEntity.Id,
                        teamEntity.TeamInRounds.Count, _siteContext.ApplicationTournamentId);
            }

            teamEntity ??= new TeamEntity();

            teamEditModel.TeamEntity = teamEntity;
            teamEditModel.Round = await GetRoundSelectorComponentModel(teamEntity, cancellationToken);
            teamEditModel.Team = GetTeamEditorComponentModel(teamEntity);
            // sync input with new model instance
            if (!await TryUpdateModelAsync(teamEditModel))
            {
                return View(ViewNames.TeamApplication.EditTeam, teamEditModel);
            }

            teamEditModel.MapFormFieldsToEntity();
            ModelState.Clear();

            if (!await teamEditModel.ValidateAsync(new TeamValidator(teamEditModel.TeamEntity, _siteContext), _siteContext.ApplicationTournamentId, ModelState, cancellationToken))
            {
                return View(ViewNames.TeamApplication.EditTeam, teamEditModel);
            }

            if (teamEntity.TeamInRounds.Any())
            {
                var tir = teamEntity.TeamInRounds.First();
                sessionModel.TeamInRound.MapEntityToFormFields(tir);
                sessionModel.TeamInRoundIsSet = true;
            }

            sessionModel.Team.MapEntityToFormFields(teamEntity);
            sessionModel.TeamIsSet = true;
            SaveModelToSession(sessionModel);

            return RedirectToAction(nameof(SelectVenue), new { Organization = _siteContext.UrlSegmentValue });
        }

        [HttpGet("select-venue")]
        public async Task<IActionResult> SelectVenue(CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            sessionModel.Venue.IsNew = true;
            SaveModelToSession(sessionModel);

            var teamEntity = new TeamEntity();
            if (!sessionModel.Team.IsNew)
            {
                teamEntity = await _appDb.TeamRepository.GetTeamEntityAsync(
                    new PredicateExpression(TeamFields.Id == sessionModel.Team.Id),
                    cancellationToken);
                if (teamEntity == null) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            }

            return View(ViewNames.TeamApplication.SelectVenue, new TeamVenueSelectModel
                {
                    TournamentId = sessionModel.PreviousTournamentId ?? _siteContext.ApplicationTournamentId,
                    TeamId = teamEntity.Id,
                    VenueId = (sessionModel.VenueIsSet && !sessionModel.Venue.IsNew)
                        ? sessionModel.Venue.Id
                        : teamEntity.VenueId
                }
            );
        }

        [HttpPost("select-venue/{*segments}")]
        public async Task<IActionResult> SelectVenue([FromForm][Bind("TeamId, VenueId")] TeamVenueSelectModel selectVenueModel, CancellationToken cancellationToken)
        {
            // model binding is not case-sensitive
            // Note: TeamId is not taken from TeamVenueSelectModel, but from ApplicationSessionModel

            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            if (!sessionModel.Team.IsNew)
            {
                var teamEntity = await _appDb.TeamRepository.GetTeamEntityAsync(
                    new PredicateExpression(TeamFields.Id == sessionModel.Team.Id),
                    cancellationToken);
                if (teamEntity == null) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            }

            if (selectVenueModel.VenueId.HasValue && !await _appDb.VenueRepository.IsValidVenueIdAsync(selectVenueModel.VenueId, cancellationToken))
            {
                return RedirectToAction(nameof(SelectVenue), new { Organization = _siteContext.UrlSegmentValue });
            }

            selectVenueModel.TournamentId = sessionModel.PreviousTournamentId ?? _siteContext.ApplicationTournamentId;

            if (!ModelState.IsValid)
            {
                return View(ViewNames.TeamApplication.SelectVenue, selectVenueModel);
            }

            sessionModel.Venue.IsNew = !selectVenueModel.VenueId.HasValue;
            sessionModel.Venue.Id = selectVenueModel.VenueId ?? 0;
            
            SaveModelToSession(sessionModel);

            return RedirectToAction(nameof(EditVenue), new { Organization = _siteContext.UrlSegmentValue });
        }

        [HttpGet("edit-venue")]
        public async Task<IActionResult> EditVenue(bool? isNew, CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            if (isNew.HasValue && isNew.Value)
            {
                sessionModel.Venue = GetVenueEditorComponentModel(new VenueEntity());
                sessionModel.VenueIsSet = false;
            }

            var venueEntity = new VenueEntity();
            var venueTeams = new List<VenueTeamRow>();
            if (!sessionModel.Venue.IsNew)
            {
                venueEntity = (await _appDb.VenueRepository.GetVenuesAsync(
                    new PredicateExpression(VenueFields.Id == sessionModel.Venue?.Id),
                    cancellationToken)).FirstOrDefault();

                if (venueEntity == null)
                {
                    return RedirectToAction(nameof(SelectVenue), new { Organization = _siteContext.UrlSegmentValue });
                }

                venueTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.VenueId == venueEntity.Id), cancellationToken);
            }

            var venueEditModel = GetVenueEditModel(venueEntity, sessionModel.Team.IsNew ? null : new TeamEntity{Id = sessionModel.Team.Id, Name = sessionModel.Team.Name},
                venueTeams.Select(vt => vt.TeamName).Distinct().OrderBy(n => n).ToList());

            venueEditModel.Venue.MapEntityToFormFields(venueEditModel.VenueEntity);
            if (sessionModel.VenueIsSet) venueEditModel.Venue = sessionModel.Venue;
            
            return View(ViewNames.TeamApplication.EditVenue, venueEditModel);
        }

        [HttpPost("edit-venue/{*segments}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVenue([FromForm] VenueEditModel venueEditModel, CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            ViewData["TournamentName"] = sessionModel.TournamentName;

            var venueEntity = new VenueEntity();
            if (!venueEditModel.Venue.IsNew)
            {
                venueEntity = (await _appDb.VenueRepository.GetVenuesAsync(
                    new PredicateExpression(VenueFields.Id == venueEditModel.Venue?.Id),
                    cancellationToken)).FirstOrDefault();

                if (venueEntity == null)
                {
                    return RedirectToAction(nameof(SelectVenue), new { Organization = _siteContext.UrlSegmentValue });
                }
            }

            var venueTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.VenueId == venueEntity.Id), cancellationToken);
            venueEditModel = GetVenueEditModel(venueEntity, null,
                venueTeams.Select(vt => vt.TeamName).Distinct().OrderBy(n => n).ToList());
            
            venueEditModel.Venue.MapEntityToFormFields(venueEditModel.VenueEntity);

            // sync input with new model instance
            if (!await TryUpdateModelAsync(venueEditModel))
            {
                return View(ViewNames.TeamApplication.EditVenue, venueEditModel);
            }

            ModelState.Clear();
            venueEditModel.Venue.MapFormFieldsToEntity(venueEditModel.VenueEntity);

            var geoResponse = new GoogleGeo.GeoResponse {Success = false};
            if (venueEditModel.ShouldAutoUpdateLocation())
            {
                geoResponse = await SetupVenueModelForGeoLocation(venueEditModel, cancellationToken);
            }

            if (!await venueEditModel.ValidateAsync(
                new VenueValidator(venueEditModel.VenueEntity, (geoResponse, venueEditModel.VenuesForDistance)),
                ModelState,
                cancellationToken))
            {
                return View(ViewNames.TeamApplication.EditVenue, venueEditModel);
            }

            sessionModel.Venue.MapEntityToFormFields(venueEditModel.VenueEntity);
            SaveModelToSession(sessionModel);
            sessionModel.VenueIsSet = true;

            return RedirectToAction(nameof(Confirm), new { Organization = _siteContext.UrlSegmentValue });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Confirm(CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });
            SaveModelToSession(sessionModel);

            var roundWithType = (await _appDb.RoundRepository.GetRoundsWithTypeAsync(
                new PredicateExpression(RoundFields.Id == sessionModel.TeamInRound.RoundId), cancellationToken)).FirstOrDefault();

            return View(ViewNames.TeamApplication.Confirm,
                new ApplicationConfirmModel
                {
                    RoundDescription = roundWithType?.Description,
                    RoundTypeDescription = roundWithType?.RoundType?.Description, 
                    SessionModel = sessionModel
                });
        }

        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(bool done, CancellationToken cancellationToken)
        {
            var sessionModel = await GetModelFromSession(cancellationToken);
            if (!sessionModel.IsFromSession) return RedirectToAction(nameof(SelectTeam), new { Organization = _siteContext.UrlSegmentValue });

            var teamInRoundEntity = new TeamInRoundEntity();
            sessionModel.TeamInRound.MapFormFieldsToEntity(teamInRoundEntity);
            teamInRoundEntity.Team = new TeamEntity();
            sessionModel.Team.MapFormFieldsToEntity(teamInRoundEntity.Team);
            teamInRoundEntity.Team.Venue = new VenueEntity();
            sessionModel.Venue.MapFormFieldsToEntity(teamInRoundEntity.Team.Venue);

            try
            {
                var isNewApplication = teamInRoundEntity.IsNew;

                // Adds the current user as team manager, unless she already is team manager
                await AddManagerToTeamEntity(teamInRoundEntity.Team, cancellationToken);
            
                if (await _appDb.GenericRepository.SaveEntityAsync(teamInRoundEntity, true, true, cancellationToken))
                {
                    HttpContext.Session.Remove(TeamApplicationSessionName);
                    TempData.Put<TeamApplicationMessageModel.TeamApplicationMessage>(
                        nameof(TeamApplicationMessageModel.TeamApplicationMessage),
                        new TeamApplicationMessageModel.TeamApplicationMessage
                        {
                            AlertType = SiteAlertTagHelper.AlertType.Success,
                            MessageId = TeamApplicationMessageModel.MessageId.ApplicationSuccess
                        });

                    _teamApplicationEmailTask.Model = new ApplicationEmailViewModel
                    {
                        RegisteredByUserId = GetCurrentUserId(),
                        TeamId = teamInRoundEntity.TeamId,
                        TeamName = teamInRoundEntity.TeamNameForRound,
                        IsNewApplication = isNewApplication,
                        TournamentName = sessionModel.TournamentName,
                        RoundId = teamInRoundEntity.RoundId,
                        OrganizationContext = _siteContext,
                        UrlToEditApplication = Url.Action(nameof(EditTeam), nameof(TeamApplication), new { Organization = _siteContext.UrlSegmentValue, teamId = teamInRoundEntity.TeamId}, Request.Scheme, Request.Host.ToString())
                    };
                    _teamApplicationEmailTask.Subject = _localizer["Registration for team '{0}'", _teamApplicationEmailTask.Model.TeamName].Value;
                    _teamApplicationEmailTask.EmailCultureInfo = CultureInfo.DefaultThreadCurrentUICulture;
                    _teamApplicationEmailTask.Timeout = TimeSpan.FromMinutes(5);
                    _teamApplicationEmailTask.ViewNames = new[] {null, ViewNames.Emails.ConfirmTeamApplicationTxt};
                    _queue.QueueTask(_teamApplicationEmailTask);

                    return RedirectToAction(nameof(List), new { Organization = _siteContext.UrlSegmentValue });
                }

                throw new Exception($"Saving the {nameof(TeamInRoundEntity)} failed.");
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Team application could not be saved.");
                HttpContext.Session.Remove(TeamApplicationSessionName);
                TempData.Put<TeamApplicationMessageModel.TeamApplicationMessage>(
                    nameof(TeamApplicationMessageModel.TeamApplicationMessage),
                    new TeamApplicationMessageModel.TeamApplicationMessage
                    {
                        AlertType = SiteAlertTagHelper.AlertType.Danger,
                        MessageId = TeamApplicationMessageModel.MessageId.ApplicationFailure
                    });
                return RedirectToAction(nameof(List), new { Organization = _siteContext.UrlSegmentValue });
            }
        }

        private IList<long> GetUserClaimTeamIds(IList<string> claimTypes)
        {
            var teamIds = new HashSet<long>();
            if (User.Claims == null)
            {
                return teamIds.ToList();
            }
            
            foreach (var claim in User.Claims.Where(c => claimTypes.Contains(c.Type)))
            {
                if(!long.TryParse(claim.Value, out var teamId)) continue;
                teamIds.Add(teamId);
            }

            return teamIds.ToList();
        }

        private async Task<RoundSelectorComponentModel> GetRoundSelectorComponentModel(TeamEntity team, CancellationToken cancellationToken)
        {
            var tir = new TeamInRoundEntity();

            if (!team.IsNew)
            {
                // The team could already be assigned to a round
                tir = (await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                        new PredicateExpression(TeamInRoundFields.TeamId == team.Id &
                                                RoundFields.TournamentId == _siteContext.ApplicationTournamentId),
                        cancellationToken))
                    .FirstOrDefault();
            }

            return new RoundSelectorComponentModel
            {
                RoundNotSpecifiedKey = null,
                SelectedRoundId = tir?.RoundId,
                ShowSelector = await _appDb.MatchRepository.GetMatchCountAsync(
                    new PredicateExpression(
                        RoundFields.TournamentId == _siteContext.ApplicationTournamentId),
                    cancellationToken) == 0,
                TournamentId = _siteContext.ApplicationTournamentId,
                HtmlFieldPrefix = nameof(TeamEditModel.Round)
            };
        }

        private TeamEditorComponentModel GetTeamEditorComponentModel(TeamEntity teamEntity)
        {
            var teamEditorComponentModel = new TeamEditorComponentModel {HtmlFieldPrefix = nameof(TeamEditModel.Team)};
            teamEditorComponentModel.MapEntityToFormFields(teamEntity);
            return teamEditorComponentModel;
        }

        private VenueEditorComponentModel GetVenueEditorComponentModel(VenueEntity venueEntity)
        {
            var venueEditorComponentModel = new VenueEditorComponentModel
            {
                HtmlFieldPrefix = nameof(VenueEditModel.Venue),
                ShowLatLng = User.IsInRole(League.Identity.Constants.RoleName.SystemManager) ||
                             User.IsInRole(League.Identity.Constants.RoleName.TournamentManager)
            };
            venueEditorComponentModel.MapEntityToFormFields(venueEntity);

            return venueEditorComponentModel;
        }

        private void SaveModelToSession(ApplicationSessionModel model)
        {
            HttpContext.Session.SetString(TeamApplicationSessionName, JsonConvert.SerializeObject(model));
        }

        private async Task<ApplicationSelectTeamModel> GetTeamSelectModel(CancellationToken cancellationToken)
        {
            var teamSelectModel = new ApplicationSelectTeamModel
            {
                TournamentName = (await _appDb.TournamentRepository.GetTournamentAsync(
                    new PredicateExpression(TournamentFields.Id == _siteContext.ApplicationTournamentId), 
                    cancellationToken)).Name
            };
            var managerTeamIds = GetUserClaimTeamIds(new[] {Identity.Constants.ClaimType.ManagesTeam});

            if (managerTeamIds.Any())
            {
                teamSelectModel.TeamsManagedByUser = (await _appDb.TeamRepository.GetLatestTeamTournamentAsync(new PredicateExpression(LatestTeamTournamentFields.TeamId.In(managerTeamIds)),
                    cancellationToken)).OrderByDescending(t => t.TournamentId).ThenBy(t => t.TeamName).ToList();
            }
            if (User.IsInRole(Identity.Constants.RoleName.TournamentManager) || User.IsInRole(Identity.Constants.RoleName.SystemManager))
            {
                teamSelectModel.TeamsManagedByUser = (await _appDb.TeamRepository.GetLatestTeamTournamentAsync(new PredicateExpression(),
                    cancellationToken)).OrderByDescending(t => t.TournamentId).ThenBy(t => t.TeamName).ToList();
            }

            return teamSelectModel;
        }

        private VenueEditModel GetVenueEditModel(VenueEntity venueEntity, TeamEntity teamEntity, IList<string> teamsUsingTheVenue)
        {
            return new VenueEditModel
            {
                TeamsUsingTheVenue = teamsUsingTheVenue,
                TeamId = teamEntity?.Id,
                ForTeamName = teamEntity?.Name,
                Venue = GetVenueEditorComponentModel(venueEntity),
                VenueEntity = venueEntity
            };
        }
        
        private async Task<GoogleGeo.GeoResponse> SetupVenueModelForGeoLocation(VenueEditModel model, CancellationToken cancellationToken)
        {
            var geoResponse = await model.TrySetGeoLocation(_googleConfig.ServiceApiKey,
                _regionInfo.TwoLetterISORegionName, TimeSpan.FromSeconds(10));
            if (!geoResponse.Success)
            {
                _logger.LogError(geoResponse.Exception,
                    "{0} failed. Response status text: {1}",
                    $"{nameof(VenueEditModel)}.{nameof(VenueEditModel.TrySetGeoLocation)}()",
                    geoResponse.StatusText);
            }
            else
            {
                model.VenuesForDistance = await _appDb.VenueRepository.GetVenuesForDistanceAsync(geoResponse.GeoLocation.LocationType == GoogleGeo.LocationType.RoofTop ? .5 : 2.0,
                    geoResponse.GeoLocation.Longitude.TotalDegrees,
                    geoResponse.GeoLocation.Latitude.TotalDegrees, cancellationToken);
            }

            return geoResponse;
        }

        private async Task<ApplicationSessionModel> GetNewSessionModel(CancellationToken cancellationToken)
        {
            // Get the application tournament and its predecessor (if any)
            var teamApplicationTournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _siteContext.ApplicationTournamentId),
                cancellationToken);
            var previousTournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.NextTournamentId == _siteContext.ApplicationTournamentId),
                cancellationToken);

            return new ApplicationSessionModel
            {
                TeamInRound = new League.Models.TeamViewModels.TeamInRoundModel(),
                Team = new TeamEditorComponentModel {HtmlFieldPrefix = nameof(ApplicationSessionModel.Team)},
                Venue = new VenueEditorComponentModel {HtmlFieldPrefix = nameof(ApplicationSessionModel.Venue)},
                TournamentName = teamApplicationTournament.Name,
                PreviousTournamentId = previousTournament?.Id
            };
        }

        private async Task<ApplicationSessionModel> GetModelFromSession(CancellationToken cancellationToken)
        {
            var jsonModel = HttpContext.Session.GetString(TeamApplicationSessionName);
            if (jsonModel == null)
            {
                var m = await GetNewSessionModel(cancellationToken);
            }

            try
            {
                var model = JsonConvert.DeserializeObject<ApplicationSessionModel>(jsonModel);
                model.IsFromSession = true;
                return model;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not restore '{0}' from session", nameof(ApplicationSessionModel));
                return await GetNewSessionModel(cancellationToken);
            }
        }

        private async Task AddManagerToTeamEntity(TeamEntity teamEntity, CancellationToken cancellationToken)
        {
            // A new team cannot have an existing team manager
            if (teamEntity.IsNew)
            {
                var mot = teamEntity.ManagerOfTeams.AddNew();
                mot.Team = teamEntity;
                mot.UserId = GetCurrentUserId();
            }
            else
            {
                // Nothing to do, if the current user is already manager of this team
                var mot = (await _appDb.ManagerOfTeamRepository.GetManagerOfTeamEntitiesAsync(new PredicateExpression(ManagerOfTeamFields.TeamId == teamEntity.Id), 
                    cancellationToken)).FirstOrDefault(u => u.UserId == GetCurrentUserId());
                if (mot == null)
                {
                    mot = teamEntity.ManagerOfTeams.AddNew();
                    mot.TeamId = teamEntity.Id;
                    mot.UserId = GetCurrentUserId();
                }
            }
        }
    }
}