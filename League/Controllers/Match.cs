﻿using System.Text;
using League.BackgroundTasks;
using League.Caching;
using League.Emailing.Creators;
using League.Helpers;
using League.Models.MatchViewModels;
using League.MultiTenancy;
using League.Routing;
using League.Views;
using MailMergeLib.AspNet;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace League.Controllers;

/// <summary>
/// The class for handling matches.
/// </summary>
[Route(TenantRouteConstraint.Template + "/[controller]")]
public class Match : AbstractController
{
    private readonly ITenantContext _tenantContext;
    private readonly IAppDb _appDb;
    private readonly IStringLocalizer<Match> _localizer;
    private readonly IAuthorizationService _authorizationService;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<Match> _logger;
    private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
    private readonly SendEmailTask _sendMailTask;
    private readonly RankingUpdateTask _rankingUpdateTask;
    private readonly RazorViewToStringRenderer _razorViewToStringRenderer;
    
    /// <summary>
    /// The controller for handling <see cref="MatchEntity"/>s.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="localizer"></param>
    /// <param name="authorizationService"></param>
    /// <param name="timeZoneConverter"></param>
    /// <param name="queue"></param>
    /// <param name="sendMailTask"></param>
    /// <param name="rankingUpdateTask"></param>
    /// <param name="razorViewToStringRenderer"></param>
    /// <param name="logger"></param>
    public Match(ITenantContext tenantContext, IStringLocalizer<Match> localizer,
        IAuthorizationService authorizationService,
        Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, Axuno.BackgroundTask.IBackgroundQueue queue,
        SendEmailTask sendMailTask, RankingUpdateTask rankingUpdateTask,
        RazorViewToStringRenderer razorViewToStringRenderer, ILogger<Match> logger)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _localizer = localizer;
        _authorizationService = authorizationService;
        _timeZoneConverter = timeZoneConverter;
        _queue = queue;
        _sendMailTask = sendMailTask;
        _rankingUpdateTask = rankingUpdateTask;
        _razorViewToStringRenderer = razorViewToStringRenderer;
        _logger = logger;
    }

    /// <summary>
    /// Redirects to the <see cref="Results"/> method.
    /// </summary>
    /// <returns></returns>
    [HttpGet("")]
    public IActionResult Index()
    {
        return Redirect(TenantLink.Action(nameof(Results), nameof(Match))!);
    }

    /// <summary>
    /// Get the fixtures for <see cref="MatchEntity"/>s.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> Fixtures(CancellationToken cancellationToken)
    {
        var tournament = await GetPlanTournament(cancellationToken);
        if (tournament == null) return NotFound();

        var model = new FixturesViewModel(_timeZoneConverter)
        {
            Tournament = tournament,
            PlannedMatches = await _appDb.MatchRepository.GetPlannedMatchesAsync(
                new PredicateExpression(PlannedMatchFields.TournamentId == _tenantContext.TournamentContext.MatchPlanTournamentId), cancellationToken),
            // try to use the browser cookie
            ActiveRoundId = null,
            // Message to show after a fixture has been updated
            FixtureMessage = TempData.Get<EditFixtureViewModel.FixtureMessage>(nameof(EditFixtureViewModel.FixtureMessage))
        };
        // if called after a fixture has been updated, make sure the active round is set, so that highlighted changes are visible
        if (model.FixtureMessage != null)
        {
            model.ActiveRoundId = model.PlannedMatches.FirstOrDefault(m => m.Id == model.FixtureMessage.MatchId)?.RoundId;
        }

        if (model.Tournament == null)
        {
            _logger.LogCritical("{tournamentId} '{id}' does not exist. User ID: '{currentUser}'", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        }

        return View(ViewNames.Match.Fixtures, model);
    }

    /// <summary>
    /// Gets an iCalendar for a single <see cref="MatchEntity"/>.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> Calendar(long id, CancellationToken cancellationToken)
    {
        var matches = await _appDb.MatchRepository.GetMatchCalendarAsync(_tenantContext.TournamentContext.MatchPlanTournamentId, id, null, null, cancellationToken);
        if (matches.Count != 1) return NoContent();

        try
        {
            var calendar = new TournamentManager.Match.Calendar
            {
                WithAlarms = true,
                Summary = _localizer["Volleyball Match"],
                DescriptionGoogleMapsFormat = _localizer["Venue in Google Maps: https://maps.google.com?q={0},{1}"],
                DescriptionOpponentsFormat = _localizer["Match '{0}' : '{1}'"],
                DescriptionFooter = "\n" + _tenantContext.OrganizationContext.Name + "\n" + _tenantContext.OrganizationContext.HomepageUrl
            };
            var stream = new MemoryStream();
            // RFC5545 sect. 3.4.1: iCal default charset is UTF8.
            // Important: no Byte Order Mark (BOM) for Android, Google, Apple
            var encoding = new UTF8Encoding(false);
            matches.ForEach(m =>
            {
                // convert to local time
                m.PlannedStart = _timeZoneConverter.ToZonedTime(m.PlannedStart)?.DateTimeOffset.DateTime;
                m.PlannedEnd = _timeZoneConverter.ToZonedTime(m.PlannedEnd)?.DateTimeOffset.DateTime;
            });
            calendar.CreateEvents(matches).Serialize(stream, encoding); 
            stream.Seek(0, SeekOrigin.Begin); 
            return File(stream, $"text/calendar; charset={encoding.HeaderName}", $"Match_{matches[0].PlannedStart?.ToString("yyyy-MM-dd") ?? string.Empty}_{Guid.NewGuid():N}.ics");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error creating {calendar} for user ID '{currentUser}'", nameof(Calendar), GetCurrentUserId());
            return NoContent();
        }
    }

    /// <summary>
    /// Gets an iCalendar.
    /// </summary>
    /// <param name="team"></param>
    /// <param name="round"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> PublicCalendar(long? team, long? round, CancellationToken cancellationToken)
    {
        var matches = await _appDb.MatchRepository.GetMatchCalendarAsync(_tenantContext.TournamentContext.MatchPlanTournamentId, null, team, round, cancellationToken);
        if (matches.Count == 0) return NoContent();

        try
        {
            var calendar = new TournamentManager.Match.Calendar
            {
                WithAlarms = true,
                Summary = _localizer["Volleyball Match"],
                DescriptionGoogleMapsFormat = _localizer["Venue in Google Maps: https://maps.google.com?q={0},{1}"],
                DescriptionOpponentsFormat = _localizer["Match '{0}' : '{1}'"],
                DescriptionFooter = "\n" + _tenantContext.OrganizationContext.Name + "\n" + _tenantContext.OrganizationContext.HomepageUrl
            };
            var stream = new MemoryStream();
            // RFC5545 sect. 3.4.1: iCal default charset is UTF8.
            // Important: no Byte Order Mark (BOM) for Android, Google, Apple
            var encoding = new UTF8Encoding(false);
            matches.ForEach(m =>
            {
                // convert to local time
                m.PlannedStart = _timeZoneConverter.ToZonedTime(m.PlannedStart)?.DateTimeOffset.DateTime;
                m.PlannedEnd = _timeZoneConverter.ToZonedTime(m.PlannedEnd)?.DateTimeOffset.DateTime;
            });
            calendar.CreateEvents(matches).Serialize(stream, encoding);
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, $"text/calendar; charset={encoding.HeaderName}", $"Match_{Guid.NewGuid():N}.ics");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error creating {calendar} for user ID '{currentUser}'", nameof(PublicCalendar), GetCurrentUserId());
            return NoContent();
        }
    }

    /// <summary>
    /// Gets the results for all completed matches.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet("[action]")]
    public async Task<IActionResult> Results(CancellationToken cancellationToken)
    {
        var tournament = await GetResultTournament(cancellationToken);
        if (tournament == null) return NotFound();

        var model = new ResultsViewModel(_timeZoneConverter)
        {
            Tournament = tournament,
            CompletedMatches = await _appDb.MatchRepository.GetCompletedMatchesAsync(
                new PredicateExpression(CompletedMatchFields.TournamentId == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken),
            // try to use the browser cookie
            ActiveRoundId = null,
            // Message to show after a result has been entered
            MatchResultMessage = TempData.Get<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage))
        };
        // if called after a fixture has been updated, make sure the active round is set, so that highlighted changes are visible
        if (model.MatchResultMessage != null)
        {
            model.ActiveRoundId = model.CompletedMatches.FirstOrDefault(m => m.Id == model.MatchResultMessage.MatchId)?.RoundId;
        }

        if (model.Tournament == null)
        {
            _logger.LogCritical("{name} '{tournamentId}' does not exist. User ID '{currentUser}'", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        }
        return View(ViewNames.Match.Results, model);
    }

    /// <summary>
    /// Returns a form for entering a match result.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
    [HttpGet("enter-result/{id:long}")]
    public async Task<IActionResult> EnterResult(long id, CancellationToken cancellationToken)
    {
        try
        {
            var match = await _appDb.MatchRepository.GetMatchWithSetsAsync(id, cancellationToken);
            if (match == null)
            {
                return NotFound($"Match id '{id}' not found.");
            }

            if (!(await _authorizationService.AuthorizeAsync(User, match, Authorization.MatchOperations.EnterResult)).Succeeded)
            {
                return Forbid();
            }
                
            var model = await GetEnterResultViewModel(match, cancellationToken);

            var permissionValidator = new MatchResultPermissionValidator(match, (_tenantContext, (model.Tournament!.IsPlanningMode, model.Round!.IsComplete, DateTime.UtcNow)));
            await permissionValidator.CheckAsync(cancellationToken);
            if (permissionValidator.GetFailedFacts().Count != 0)
            {
                return View(ViewNames.Match.EnterResultNotAllowed,
                    (model.Tournament, permissionValidator.GetFailedFacts().First().Message));
            }
                
            return View(ViewNames.Match.EnterResult, model);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Building {model} failed for MatchId '{matchId}'", nameof(EnterResultViewModel), id);
            throw;
        }
    }

    /// <summary>
    /// Processes the post for a match result.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
    [HttpPost("enter-result/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnterResult([FromForm] EnterResultViewModel? model, CancellationToken cancellationToken)
    {
        MatchEntity? match;
        try
        {
            match = await _appDb.MatchRepository.GetMatchWithSetsAsync(model?.Id, cancellationToken);
            if (match == null)
            {
                return NotFound($"Match id '{model?.Id}' not found.");
            }

            if (!(await _authorizationService.AuthorizeAsync(User, match, Authorization.MatchOperations.EnterResult)).Succeeded)
            {
                return Forbid();
            }

            model = await GetEnterResultViewModel(match, cancellationToken);
            // sync input with new model instance
            if (!await TryUpdateModelAsync(model))
            {
                return View(ViewNames.Match.EnterResult, model);
            }

            var permissionValidator = new MatchResultPermissionValidator(match, (_tenantContext, (model.Tournament!.IsPlanningMode, model.Round!.IsComplete, DateTime.UtcNow)));
            await permissionValidator.CheckAsync(cancellationToken);
            if (permissionValidator.GetFailedFacts().Count != 0)
            {
                return View(ViewNames.Match.EnterResultNotAllowed,
                    (model.Tournament, permissionValidator.GetFailedFacts().First().Message));
            }

            model.MapFormFieldsToEntity();
            ModelState.Clear();

            if (!await model.ValidateAsync(new MatchResultValidator(model.Match!,
                    (_tenantContext, _timeZoneConverter, (model.Round.MatchRule, model.Round.SetRule))), ModelState))
            {
                return View(ViewNames.Match.EnterResult, model);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Building {model} failed for MatchId '{modelId}'. User ID '{currentUser}'", nameof(EnterResultViewModel), model?.Id, GetCurrentUserId());
            throw;
        }

        try
        {
            // validation succeeded
            match.CalculateMatchPoints(model.Round.MatchRule);
                
            // save the match entity and sets
            var success = await _appDb.MatchRepository.SaveMatchResultAsync(match, cancellationToken);
            _logger.LogInformation("Result for match ID {matchId} was entered by user ID '{currentUser}'", match.Id, GetCurrentUserId());

            TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = success });

            if (success)
            {
                // This preserves OutOfSyncExceptions. We only read what was successfully saved without a re-fetch.
                match.Fields.MarkAsFetched();
                match.Sets.ToList().ForEach(s => s.Fields.MarkAsFetched());
                SendResultNotification(match);
                UpdateRanking(match.RoundId);
            }
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Building {enterResultViewModel} failed for MatchId '{model.Id}' and  user ID '{currentUser}'", nameof(EnterResultViewModel), model.Id, GetCurrentUserId());
            TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = false });
        }

        // redirect to results overview, where success message is shown
        return Redirect(TenantLink.Action(nameof(Results), nameof(Match))!);
    }

    /// <summary>
    /// Gets a form for editing a fixture.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
    [HttpGet("edit-fixture/{id:long}")]
    public async Task<IActionResult> EditFixture(long id, CancellationToken cancellationToken)
    {
        var model = new EditFixtureViewModel(await GetPlannedMatchFromDatabase(id, cancellationToken), _timeZoneConverter)
        {
            Tournament = await GetPlanTournament(cancellationToken)
        };

        if (model.PlannedMatch == null || model.Tournament == null)
        {
            var msg = $"No data for fixture id '{model.Id}'. User ID '{GetCurrentUserId()}'.";
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogInformation(msg);
#pragma warning restore CA2254 // Template should be a static expression
            return NotFound(msg);
        }

        if (!(await _authorizationService.AuthorizeAsync(User,
                new MatchEntity
                {
                    HomeTeamId = model.PlannedMatch.HomeTeamId, GuestTeamId = model.PlannedMatch.GuestTeamId,
                    VenueId = model.PlannedMatch.VenueId, OrigVenueId = model.PlannedMatch.OrigVenueId
                }, Authorization.MatchOperations.ChangeFixture)).Succeeded)
        {
            return Forbid();
        }

        return View(ViewNames.Match.EditFixture, await AddDisplayDataToEditFixtureViewModel(model, cancellationToken));
    }

    /// <summary>
    /// Processes the posted form of edited fixture.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
    [HttpPost("edit-fixture/{*segments}")]
    [ValidateAntiForgeryToken]  
    public async Task<IActionResult> EditFixture([FromForm] EditFixtureViewModel model, CancellationToken cancellationToken)
    {
        // [FromBody] => 'content-type': 'application/json'
        // [FromForm] => 'content-type': 'application/x-www-form-urlencoded'

        model = new EditFixtureViewModel(await GetPlannedMatchFromDatabase(model.Id, cancellationToken), _timeZoneConverter)
        {
            Tournament = await GetPlanTournament(cancellationToken)
        };

        if (model.PlannedMatch == null || model.Tournament == null)
        {
            var msg = $"No data for fixture id '{model.Id}'. User ID '{GetCurrentUserId()}'";
#pragma warning disable CA2254 // Template should be a static expression
            _logger.LogInformation(msg);
#pragma warning restore CA2254 // Template should be a static expression
            return NotFound(msg);
        }

        if (!(await _authorizationService.AuthorizeAsync(User,
                new MatchEntity
                {
                    HomeTeamId = model.PlannedMatch.HomeTeamId,
                    GuestTeamId = model.PlannedMatch.GuestTeamId,
                    VenueId = model.PlannedMatch.VenueId,
                    OrigVenueId = model.PlannedMatch.OrigVenueId
                }, Authorization.MatchOperations.ChangeFixture)).Succeeded)
        {
            return Forbid();
        }

        // sync input with new model instance
        if (!await TryUpdateModelAsync(model))
        {
            return View(ViewNames.Match.EditFixture, await AddDisplayDataToEditFixtureViewModel(model, cancellationToken));
        }
            
        // create a new MatchEntity for validation
        var match = FillMatchEntity(model.PlannedMatch);
        match.SetPlannedStart(model is { MatchDate: not null, MatchTime: not null }
            ? _timeZoneConverter.ToUtc(model.MatchDate.Value.Add(model.MatchTime.Value.ToTimeSpan()))
            : default(DateTime?), _tenantContext.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch);
        match.SetVenueId(model.VenueId);
        if (match.IsDirty) match.ChangeSerial += 1;

        ModelState.Clear();

        // Todo: This business logic should rather go into settings
        //_tenantContext.TournamentContext.FixtureRuleSet.PlannedMatchTimeMustStayInCurrentLegBoundaries = model.Tournament.IsPlanningMode;

        if (!await model.ValidateAsync(
                new FixtureValidator(match, (_tenantContext, _timeZoneConverter, model.PlannedMatch), DateTime.UtcNow),
                ModelState))
        {
            return View(ViewNames.Match.EditFixture, await AddDisplayDataToEditFixtureViewModel(model, cancellationToken));
        }

        var fixtureIsChanged = match.IsDirty;

        var fixtureMessage = new EditFixtureViewModel.FixtureMessage {MatchId = model.Id, ChangeSuccess = false};
        // save the match entity
        try
        {
            fixtureMessage.ChangeSuccess = await _appDb.GenericRepository.SaveEntityAsync(match, false, false, cancellationToken);
            _logger.LogInformation("Fixture for match id {matchId} updated successfully for user ID '{currentUser}'", match.Id, GetCurrentUserId());
            if (fixtureIsChanged && !model.Tournament.IsPlanningMode) SendFixtureNotification(match.Id);
        }
        catch (Exception e)
        {
            fixtureMessage.ChangeSuccess = false;
            _logger.LogCritical(e, "Fixture update for match id {matchId} failed for user ID '{currentUser}'", match.Id, GetCurrentUserId());
        }

        // redirect to fixture overview, where success message is shown
        TempData.Put<EditFixtureViewModel.FixtureMessage>(nameof(EditFixtureViewModel.FixtureMessage), fixtureMessage);
        return Redirect(TenantLink.Action(nameof(Fixtures), nameof(Match))!);
    }

    /// <summary>
    /// Fills a new <see cref="MatchEntity"/> with data retrieved from the database.
    /// </summary>
    /// <param name="currentData"></param>
    /// <returns>Returns a <see cref="MatchEntity"/> ready to be processed further.</returns>
    private static MatchEntity FillMatchEntity(PlannedMatchRow currentData)
    {
        // set current values
        var match = new MatchEntity
        {
            IsNew = false,
            Id = currentData.Id,
            HomeTeamId = currentData.HomeTeamId,
            GuestTeamId = currentData.GuestTeamId,
            PlannedStart = currentData.PlannedStart,
            PlannedEnd = currentData.PlannedEnd,
            OrigPlannedStart = currentData.OrigPlannedStart,
            OrigPlannedEnd = currentData.OrigPlannedEnd,
            VenueId = currentData.VenueId,
            OrigVenueId = currentData.OrigVenueId,
            RoundId = currentData.RoundId,
            LegSequenceNo = currentData.RoundLegSequenceNo,
            ChangeSerial = currentData.ChangeSerial,
            IsDirty = false // flag as unchanged
        };

        return match;
    }

    private async Task<PlannedMatchRow?> GetPlannedMatchFromDatabase(long matchId, CancellationToken cancellationToken)
    {
        try
        {
            return (await _appDb.MatchRepository.GetPlannedMatchesAsync(
                new PredicateExpression(PlannedMatchFields.Id == matchId), cancellationToken)).FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error loading match in '{methodName}' for user ID '{currentUser}'", nameof(_appDb.MatchRepository.GetPlannedMatchesAsync), GetCurrentUserId());
            return null;
        }
    }

    private async Task<EditFixtureViewModel?> AddDisplayDataToEditFixtureViewModel(EditFixtureViewModel model, CancellationToken cancellationToken)
    {
        try
        {
            // Get all venues and teams for a tournament and select in-memory is 40% faster compared to database selections
            var venuesWithTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.TournamentId == _tenantContext.TournamentContext.MatchPlanTournamentId),
                cancellationToken);
            var allVenues = await _appDb.VenueRepository.GetVenuesAsync(new PredicateExpression(), cancellationToken);

            // get venue entities of match teams
            var venueIdsOfMatchTeams = venuesWithTeams
                .Where(vwt => vwt.TeamId ==  model.PlannedMatch?.HomeTeamId || vwt.TeamId == model.PlannedMatch?.GuestTeamId).Select(vwt => vwt.VenueId).Distinct();
            model.VenuesOfMatchTeams = allVenues.Where(v => venueIdsOfMatchTeams.Contains(v.Id)).ToList();

            // get venue entities of current tournament
            var activeVenueIds = venuesWithTeams.Select(vwt => vwt.VenueId).Distinct();
            model.ActiveVenues = allVenues.Where(v => activeVenueIds.Except(venueIdsOfMatchTeams).Contains(v.Id)).ToList();

            // get remaining venues (currently inactive)
            model.UnusedVenues = allVenues.Where(v => !activeVenueIds.Contains(v.Id)).OrderBy(v => v.City).ThenBy(v => v.Name).ToList();

            return model;
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "Error adding display data to model '{model}' for user ID '{currentUser}'", nameof(EditFixtureViewModel), GetCurrentUserId());
            return null;
        }
    }

    private async Task<TournamentEntity?> GetPlanTournament(CancellationToken cancellationToken)
    {
        var tournament =
            await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchPlanTournamentId), cancellationToken);

        if (tournament != null) return tournament;

        _logger.LogCritical("{name} '{id}' does not exist. User ID '{currentUser}'.", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        return null;
    }

    private async Task<TournamentEntity?> GetResultTournament(CancellationToken cancellationToken)
    {
        var tournament =
            await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken);

        if (tournament != null) return tournament;

        _logger.LogCritical("{name} '{id}' does not exist. User ID '{currentUser}'.", nameof(_tenantContext.TournamentContext.MatchResultTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        return null;
    }

    private async Task<EnterResultViewModel> GetEnterResultViewModel(MatchEntity match, CancellationToken cancellationToken)
    {
        match.Sets.RemovedEntitiesTracker = new EntityCollection<SetEntity>();
        var teamInRound = await _appDb.TeamInRoundRepository.GetTeamInRoundAsync(
            new PredicateExpression(TeamInRoundFields.TeamId.In(match.HomeTeamId, match.GuestTeamId).And(TeamInRoundFields.RoundId == match.RoundId)),
            cancellationToken);
        if (teamInRound.Count != 2)
        {
            _logger.LogCritical("Number of found opponents for a match does not equal 2. User ID '{currentUser}'.", GetCurrentUserId());
        }

        var tournament = await _appDb.TournamentRepository.GetTournamentAsync(
            new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken);
        if (tournament == null)
        {
            _logger.LogCritical("{name} '{id}' does not exist", nameof(_tenantContext.TournamentContext.MatchResultTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId);
        }

        var roundWithRules =
            await _appDb.RoundRepository.GetRoundWithRulesAsync(match.RoundId, cancellationToken);
        if (roundWithRules == null)
        {
            _logger.LogCritical("Round with {roundId}='{id}' does not exist. User ID '{currentUser}'.", _tenantContext.TournamentContext.MatchPlanTournamentId, match.RoundId, GetCurrentUserId());
        }

        return tournament != null && roundWithRules != null && teamInRound.Count == 2
            ? new EnterResultViewModel(tournament, roundWithRules, match, roundWithRules.MatchRule, teamInRound,
                    _timeZoneConverter)
                { ReturnUrl = GetReturnUrl()}
            : throw new InvalidOperationException($"{nameof(EnterResultViewModel)} could not be initiated");
    }

    /// <summary>
    /// Gets a match report sheet suitable for a printout,
    /// if the match has not already been played.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cache">The cache injected from services.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>A match report sheet suitable for a printout, if the match has not already been played.</returns>
    [HttpGet("[action]/{id:long}")]
    public async Task<IActionResult> ReportSheet(long id, [FromServices] ReportSheetCache cache, CancellationToken cancellationToken)
    {
        MatchReportSheetRow? model = null;
            
        try
        {
            model = await _appDb.MatchRepository.GetMatchReportSheetAsync(_tenantContext.TournamentContext.MatchPlanTournamentId, id, cancellationToken);
            
            if (model == null) return NotFound();

            var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName($"{_localizer["Report Sheet"].Value}_{model.Id}.pdf");
            Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition] =
                contentDisposition.ToString();

            var html = await _razorViewToStringRenderer.RenderViewToStringAsync(
                $"~/Views/{nameof(Match)}/{ViewNames.Match.ReportSheet}.cshtml", model);

            //var cache = new ReportSheetCache(_tenantContext, _configuration, _webHostEnvironment);
            var stream = await cache.GetOrCreatePdf(model, html, cancellationToken);
            _logger.LogInformation("PDF file returned for tenant '{Tenant}' and match id '{MatchId}'", _tenantContext.Identifier, id);
            return new FileStreamResult(stream, "application/pdf");
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, "{method} failed for match ID '{matchId}'", nameof(ReportSheet), id);
        }
            
        // Not able to render report sheet as PDF: return HTML
        Response.Clear();
        return View(ViewNames.Match.ReportSheet, model);
    }
    
    private void SendFixtureNotification(long matchId)
    {
        _sendMailTask.SetMessageCreator(new ChangeFixtureCreator
        {
            Parameters =
            {
                CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                ChangedByUserId = GetCurrentUserId() ?? throw new InvalidOperationException("Current user ID must not be null"),
                MatchId = matchId,
            }
        });

        _queue.QueueTask(_sendMailTask);
    }

    private void SendResultNotification(in MatchEntity match)
    {
        // Todo: Should we check whether an existing result was changed?
            
        _sendMailTask.SetMessageCreator(new ResultEnteredCreator
        {
            Parameters =
            {
                CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                ChangedByUserId = GetCurrentUserId() ?? throw new InvalidOperationException("Current user ID must not be null"),
                Match = match,
            }
        });

        _queue.QueueTask(_sendMailTask);
    }

    private void UpdateRanking(in long roundId)
    {
        _rankingUpdateTask.TenantContext = _tenantContext;
        _rankingUpdateTask.RoundId = roundId;
        _rankingUpdateTask.Timeout = TimeSpan.FromMinutes(2);
        _queue.QueueTask(_rankingUpdateTask);
    }

    /// <summary>
    /// Compares the fully qualified URLs of the referer and the <see cref="Results"/> action.
    /// </summary>
    /// <returns>Returns the <see cref="Results"/> action uri when coming from results table, else the <see cref="Fixtures"/> uri.</returns>
    private string GetReturnUrl()
    {
        var returnUrl = HttpContext.Request.GetTypedHeaders().Referer?.ToString();
        return (returnUrl == TenantLink.Action(nameof(Results), nameof(Match))
            ? TenantLink.Action(nameof(Results), nameof(Match)) // editing a result is exceptional
            : TenantLink.Action(nameof(Fixtures), nameof(Match))) ?? string.Empty; // coming from fixtures is normal
    }
}
