﻿using League.Components;
using League.Helpers;
using League.Models.TeamViewModels;
using League.Models.UploadViewModels;
using League.MultiTenancy;
using League.Routing;
using League.TagHelpers;
using Microsoft.Extensions.Logging.Abstractions;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Team : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IAppDb _appDb;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<Team> _logger;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly SignInManager<Identity.ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public Team(ITenantContext tenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, IWebHostEnvironment webHostEnvironment, IAuthorizationService authorizationService, SignInManager<Identity.ApplicationUser> signInManger, ILogger<Team> logger)
    {
        _tenantContext = tenantContext;
        _timeZoneConverter = timeZoneConverter;
        _webHostEnvironment = webHostEnvironment;
        _appDb = tenantContext.DbContext.AppDb;
        _signInManager = signInManger;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect(TenantLink.Action(nameof(List), nameof(Team)) ?? string.Empty);
    }

    [HttpGet("[action]")]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var model = new TeamListModel(_timeZoneConverter)
        {
            Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.TeamTournamentId)
                , cancellationToken),
            RoundsWithTeams = await _appDb.RoundRepository.GetRoundsWithTeamsAsync(
                new PredicateExpression(RoundTeamFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId),
                cancellationToken)
        };

        if (model.Tournament == null)
        {
            _logger.LogError("{TeamTournamentId} '{Td}' does not exist", nameof(_tenantContext.TournamentContext.TeamTournamentId), _tenantContext.TournamentContext.TeamTournamentId);
            return NotFound();
        }

        return View(Views.ViewNames.Team.List, model);
    }

    [Authorize(nameof(Authorization.PolicyName.MyTeamPolicy))]
    [HttpGet("my-team/{id:long?}")]
    public async Task<IActionResult> MyTeam(long? id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Redirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { id = default(long?) })!);

        // make sure that any changes are immediately reflected in the user's application cookie
        var appUser = await _signInManager.UserManager.GetUserAsync(User);
        if (appUser != null)
            await _signInManager.RefreshSignInAsync(appUser);
        else
        {
            await _signInManager.SignOutAsync();
            return Redirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { id = default(long?) })!);
        }
            
        var userTeamIds = GetUserClaimTeamIds();
        // As admin, any selected team will be returned as "my team"
        if (id.HasValue && (await _authorizationService.AuthorizeAsync(User, Authorization.PolicyName.MyTeamAdminPolicy)).Succeeded)
        {
            userTeamIds.Clear();
            userTeamIds.Add(id.Value);
        }
            
        if (id.HasValue && !userTeamIds.Contains(id.Value))
        {
            return Redirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { id = default(long?) })!);
        }

        var teamUserRoundInfos = await _appDb.TeamRepository.GetTeamUserRoundInfosAsync(
            new PredicateExpression(TeamUserRoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId &
                                    (TeamUserRoundFields.IsManager == true | TeamUserRoundFields.IsPlayer == true))
                .AddWithAnd(TeamUserRoundFields.TeamId.In(userTeamIds)),
            cancellationToken);

        if (teamUserRoundInfos.Count == 0)
        {
            return View(Views.ViewNames.Team.MyTeamNotFound, await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.TeamTournamentId)
                , cancellationToken));
        }

        var model = new MyTeamShowModel(_timeZoneConverter)
        {
            Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.TeamTournamentId)
                , cancellationToken),
            TeamUserRoundInfos = teamUserRoundInfos,
            TeamVenueRoundInfos = await _appDb.TeamRepository.GetTeamVenueRoundInfoAsync(
                new PredicateExpression(TeamVenueRoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId)
                    .AddWithAnd(TeamVenueRoundFields.TeamId.In(userTeamIds)), cancellationToken),
            ActiveTeamId = id ?? teamUserRoundInfos.Max(tur => tur.TeamId),
            TeamPhotoStaticFile =
                new TeamPhotoStaticFile(_webHostEnvironment, _tenantContext, new NullLogger<TeamPhotoStaticFile>())
        };
        model.ActiveTeamId = model.TeamVenueRoundInfos.Max(tvr => tvr.TeamId);

        return View(Views.ViewNames.Team.MyTeam, model);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Single(long id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

        var model = new TeamSingleModel(_timeZoneConverter)
        {
            Tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.TeamTournamentId)
                , cancellationToken),
            TeamVenueRoundInfo = (await _appDb.TeamRepository.GetTeamVenueRoundInfoAsync(
                    new PredicateExpression(TeamVenueRoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId &
                                            TeamVenueRoundFields.TeamId == id), cancellationToken))
                .FirstOrDefault(),
            TeamUserRoundInfos = await _appDb.TeamRepository.GetTeamUserRoundInfosAsync(
                new PredicateExpression(TeamUserRoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId &
                                        TeamUserRoundFields.TeamId == id & TeamUserRoundFields.IsManager == true), cancellationToken),
            ShowContactInfos = (await _authorizationService.AuthorizeAsync(User, Authorization.PolicyName.SeeTeamContactsPolicy)).Succeeded
        };

        if (model.TeamVenueRoundInfo == null)
        {
            return NotFound();
        }

        model.PhotoUriInfo = new TeamPhotoStaticFile(_webHostEnvironment, _tenantContext, new NullLogger<TeamPhotoStaticFile>()).GetUriInfo(id)!;

        return View(Views.ViewNames.Team.Single, model);
    }

    [HttpGet("[action]/{teamId:long}")]
    public async Task<IActionResult> Edit(long teamId, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

        var team = await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == teamId), cancellationToken);
        if (team == null)
        {
            return NotFound();
        }

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(team.Id),
                Authorization.TeamOperations.EditTeam)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new { ReturnUrl = TenantLink.Action(nameof(MyTeam), nameof(Team)) }));
        }

        var model = new TeamEditModel
        {
            Round = await GetRoundSelectorComponentModel(team, cancellationToken),
            Team = GetTeamEditorComponentModel(team)
        };

        return PartialView(Views.ViewNames.Team._EditTeamModalPartial, model);
    }

    [HttpPost("[action]/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromForm] TeamEditModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

        TeamEntity? team = null;
        if (model.Team is { IsNew: false })
        {
            team = await _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == model.Team.Id), cancellationToken);
            if (team == null)
            {
                return NotFound();
            }

            if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(team.Id),
                    Authorization.TeamOperations.EditTeam)).Succeeded)
            {
                return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                    new {ReturnUrl = TenantLink.Action(nameof(MyTeam), nameof(Team))}));
            }

            team.TeamInRounds.AddRange(await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
                new PredicateExpression(RoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId & TeamInRoundFields.TeamId == team.Id),
                cancellationToken));

            if (team.TeamInRounds.Count > 1)
                _logger.LogError("Teams ID {TeamId} belongs to {RoundsCount} rounds for tournament ID {TournamentId}", team.Id,
                    team.TeamInRounds.Count, _tenantContext.TournamentContext.TeamTournamentId);
        }

        team ??= new TeamEntity();

        model.TeamEntity = team;
        model.Round = await GetRoundSelectorComponentModel(team, cancellationToken);
        model.Team = GetTeamEditorComponentModel(team);
        // sync input with new model instance
        if (!await TryUpdateModelAsync(model))
        {
            return PartialView(Views.ViewNames.Team._EditTeamModalPartial, model);
        }

        model.MapFormFieldsToEntity();
        ModelState.Clear();

        if (!await model.ValidateAsync(new TeamValidator(model.TeamEntity, _tenantContext), _tenantContext.TournamentContext.TeamTournamentId, ModelState, cancellationToken))
        {
            return PartialView(Views.ViewNames.Team._EditTeamModalPartial, model);
        }

        var tournament = await _appDb.TournamentRepository.GetTournamentAsync(
            new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.TeamTournamentId)
            , cancellationToken);

        // The team name for an active tournament won't be changed
        if (tournament != null && await IsPlanningMode(tournament, cancellationToken))
        {
            foreach (var tir in model.TeamEntity.TeamInRounds)
            {
                tir.TeamNameForRound = team.Name;
            }
        }

        try
        {
            if (await _appDb.GenericRepository.SaveEntityAsync(model.TeamEntity, false, true, cancellationToken))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MyTeamMessageModel.MessageId.TeamDataSuccess });
                return JsonResponseRedirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { id = team.Id}));
            }
        }
        catch (Exception e)
        {
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MyTeamMessageModel.MessageId.TeamDataFailure});
            _logger.LogError(e, "Error saving team id '{TeamId}'", model.Team.IsNew ? "new" : model.Team.Id.ToString());
            return JsonResponseRedirect(TenantLink.Action(nameof(MyTeam), nameof(Team),new { id = team.Id }));
        }

        // We never should come this far
        return PartialView(Views.ViewNames.Team._EditTeamModalPartial, model);
    }

    [HttpGet("[action]/{tid}")]
    public async Task<IActionResult> ChangeVenue(long tid, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

        var teamEntity = await
            _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == tid),
                cancellationToken);
        if (teamEntity == null)
        {
            return NotFound();
        }

        return PartialView(Views.ViewNames.Team._ChangeVenueModalPartial, (TeamId: teamEntity.Id, VenueId: teamEntity.VenueId));
    }

    [HttpGet("[action]/{tid}")]
    public async Task<IActionResult> SelectVenue(long tid, string returnUrl, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return NotFound();

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
            return Redirect(GeneralLink.GetPathByAction(nameof(Error.AccessDenied), nameof(Error),
                new { ReturnUrl = Redirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new {tid})!) })!);
        }

        return PartialView(Views.ViewNames.Team._SelectVenueModalPartial,
            new TeamVenueSelectModel
            {
                TournamentId = _tenantContext.TournamentContext.TeamTournamentId, TeamId = teamEntity.Id, VenueId = teamEntity.VenueId,
                ReturnUrl = (Url.IsLocalUrl(returnUrl)
                    ? returnUrl
                    : TenantLink.Action(nameof(MyTeam), nameof(Team))) ?? string.Empty
            }
        );
    }

    [HttpPost("[action]/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SelectVenue([FromForm][Bind("TeamId, VenueId")] TeamVenueSelectModel model, CancellationToken cancellationToken)
    {
        // model binding is not case-sensitive

        if (!ModelState.IsValid) return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
            new { ReturnUrl = TenantLink.Action(nameof(MyTeam), nameof(Team)) }));

        var teamEntity = await
            _appDb.TeamRepository.GetTeamEntityAsync(new PredicateExpression(TeamFields.Id == model.TeamId),
                cancellationToken);

        if (teamEntity == null)
        {
            TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MyTeamMessageModel.MessageId.TeamDataFailure});
            return JsonResponseRedirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { model.TeamId }));
        }

        if (!(await _authorizationService.AuthorizeAsync(User, new TeamEntity(teamEntity.Id),
                Authorization.TeamOperations.EditTeam)).Succeeded)
        {
            return JsonResponseRedirect(Url.Action(nameof(Error.AccessDenied), nameof(Error),
                new { ReturnUrl = TenantLink.Action(nameof(MyTeam), nameof(Team), new { model.TeamId }) }));
        }

        model.TournamentId = _tenantContext.TournamentContext.TeamTournamentId;
        teamEntity.VenueId = model.VenueId;
        var teamVenueValidator = new TeamVenueValidator(teamEntity, _tenantContext);

        if (!await TeamVenueSelectModel.ValidateAsync(teamVenueValidator, ModelState, cancellationToken))
        {
            return PartialView(Views.ViewNames.Team._SelectVenueModalPartial, model);
        }
            
        TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Danger, MessageId = MyTeamMessageModel.MessageId.VenueSelectFailure });
        try
        {
            teamEntity.VenueId = model.VenueId;
            if(await _appDb.GenericRepository.SaveEntityAsync(teamEntity, false, false, cancellationToken))
            {
                TempData.Put<MyTeamMessageModel.MyTeamMessage>(nameof(MyTeamMessageModel.MyTeamMessage), new MyTeamMessageModel.MyTeamMessage { AlertType = SiteAlertTagHelper.AlertType.Success, MessageId = MyTeamMessageModel.MessageId.VenueSelectSuccess});
            }
            else
            {
                throw new InvalidOperationException("Failed to save team entity");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to save selected venue for team id {TeamId}, venue id {VSenueId}", model.TeamId, model.VenueId);
        }
            
        return JsonResponseRedirect(TenantLink.Action(nameof(MyTeam), nameof(Team), new { model.TeamId }));
    }

    private List<long> GetUserClaimTeamIds()
    {
        var teamIds = new HashSet<long>();
            
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
                                        RoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId), cancellationToken))
            .FirstOrDefault();
        return new RoundSelectorComponentModel
        {
            EnforceExplicitSelection = false,
            SelectedRoundId = tir?.RoundId,
            ShowSelector = await _appDb.MatchRepository.GetMatchCountAsync(
                new PredicateExpression(
                    RoundFields.TournamentId == _tenantContext.TournamentContext.TeamTournamentId),
                cancellationToken) == 0,
            TournamentId = _tenantContext.TournamentContext.TeamTournamentId,
            HtmlFieldPrefix = nameof(TeamEditModel.Round)
        };
    }

    private static TeamEditorComponentModel GetTeamEditorComponentModel(TeamEntity team)
    {
        var model = new TeamEditorComponentModel();
        model.MapEntityToFormFields(team);
        model.HtmlFieldPrefix = nameof(TeamEditModel.Team);
        return model;
    }

    /// <summary>
    /// Determines whether the tournament is in planning mode at the current date/time:
    /// <para/>
    /// Gets the minimum StartDateTime of the RoundLeg entity across all Rounds of a Tournament
    /// and subtracts the number of notification days before the next match from it.
    /// The current date/time must be before this value to be in planning mode.
    /// </summary>
    /// <param name="tournament">The <see cref="TournamentEntity"/> to test for planning mode.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns><see langword="true"/> if the tournament has started on the specified date; otherwise, <see langword="false"/>.</returns>
    private async Task<bool> IsPlanningMode(TournamentEntity tournament, CancellationToken cancellationToken)
    {
        return await _appDb.RoundRepository
            .GetRoundStartAsync(tournament.Id, cancellationToken)
            .ContinueWith(
                t => DateTime.UtcNow < t.Result.AddDays(-_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextMatch),
                TaskContinuationOptions.OnlyOnRanToCompletion);
    }
}
