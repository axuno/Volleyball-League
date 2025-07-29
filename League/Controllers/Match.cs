using System.Text;
using League.BackgroundTasks;
using League.Caching;
using League.Emailing;
using League.Emailing.Creators;
using League.Helpers;
using League.Models.MatchViewModels;
using League.MultiTenancy;
using League.Routing;
using League.Views;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.HtmlToPdfConverter;
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
    private readonly IServiceProvider _serviceProvider;
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
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public Match(ITenantContext tenantContext, IStringLocalizer<Match> localizer,
        IAuthorizationService authorizationService, IServiceProvider serviceProvider,
        ILogger<Match> logger)
    {
        _tenantContext = tenantContext;
        _appDb = tenantContext.DbContext.AppDb;
        _localizer = localizer;
        _authorizationService = authorizationService;
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Get required services from the service provider to stay below the 7 parameter limit of SonarCloud
        _timeZoneConverter = serviceProvider.GetRequiredService<Axuno.Tools.DateAndTime.TimeZoneConverter>();
        _queue = serviceProvider.GetRequiredService<Axuno.BackgroundTask.IBackgroundQueue>();
        _sendMailTask = serviceProvider.GetRequiredService<SendEmailTask>();
        _rankingUpdateTask = serviceProvider.GetRequiredService<RankingUpdateTask>();
        _razorViewToStringRenderer = serviceProvider.GetRequiredService<RazorViewToStringRenderer>();
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
            model.ActiveRoundId = model.PlannedMatches.Find(m => m.Id == model.FixtureMessage.MatchId)?.RoundId;
        }

        if (model.Tournament == null)
        {
            _logger.LogError("{TournamentId} '{Id}' does not exist. User ID: '{CurrentUser}'", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
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
        if (!ModelState.IsValid) return NoContent();

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

            // match date/times have DateTimeKind.Unspecified but are in UTC
            calendar.CreateEvents(matches, "UTC").Serialize(stream); // stream is UTF8 without BOM by default
            stream.Seek(0, SeekOrigin.Begin);

            var filename = $"Match_{matches[0].PlannedStart?.ToString("yyyy-MM-dd") ?? string.Empty}_{Guid.NewGuid():N}.ics";
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{filename}\"");
            Response.Headers.Append("X-Content-Type-Options", "nosniff");

            // Content-Disposition is only used with FileStreamResult(...), not with File(...)
            return new FileStreamResult(stream, "text/calendar; charset=utf-8");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating {Calendar} for user ID '{CurrentUser}'", nameof(Calendar), GetCurrentUserId());
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
        if (!ModelState.IsValid) return NoContent();

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

            // match date/times have DateTimeKind.Unspecified but are in UTC
            calendar.CreateEvents(matches, "UTC").Serialize(stream); // stream is UTF8 without BOM by default
            stream.Seek(0, SeekOrigin.Begin);
            return File(stream, $"text/calendar; charset={Encoding.UTF8.WebName}", $"Match_{Guid.NewGuid():N}.ics");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating {Calendar} for user ID '{CurrentUser}'", nameof(PublicCalendar), GetCurrentUserId());
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
            model.ActiveRoundId = model.CompletedMatches.Find(m => m.Id == model.MatchResultMessage.MatchId)?.RoundId;
        }

        if (model.Tournament == null)
        {
            _logger.LogError("{Name} '{TournamentId}' does not exist. User ID '{CurrentUser}'", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        }
        return View(ViewNames.Match.Results, model);
    }

    /// <summary>
    /// Prepares the view for entering a match result in advanced mode.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    //[Authorize(Policy = Authorization.PolicyName.OverruleResultPolicy)]
    [HttpGet("overrule-result/{id:long}")]
    public async Task<IActionResult> OverruleResult(long id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return Forbid();

        var match = await _appDb.MatchRepository.GetMatchWithSetsAsync(id, cancellationToken);
        if (!(await _authorizationService.AuthorizeAsync(User, match, Authorization.MatchOperations.OverruleResult)).Succeeded)
        {
            return Forbid();
        }

        TempData.Put<string>(nameof(OverruleResult), true.ToString());
        return Redirect(TenantLink.Action(nameof(EnterResult), nameof(Match), new { id })!);
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
        if (!ModelState.IsValid) return Forbid();

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

            var overrule = TempData.Get<string>(nameof(OverruleResult)) == true.ToString();
            var userCanOverrule =
                (await _authorizationService.AuthorizeAsync(User, match, Authorization.MatchOperations.OverruleResult))
                .Succeeded;

            if (overrule && !userCanOverrule)
            {
                return Forbid();
            }

            var model = await GetEnterResultViewModel(match, cancellationToken);
            model.IsOverruling = overrule;

            var permissionValidator = new MatchResultPermissionValidator(match,
                (_tenantContext,
                    (await IsPlanningMode(model.Tournament!, cancellationToken), model.Round!.IsComplete,
                        DateTime.UtcNow)));
            if (userCanOverrule)
            {
                // Disable the deadline check, if the user is allowed to overrule results
                permissionValidator.Facts
                    .First(f =>
                        f.Id == MatchResultPermissionValidator.FactId.CurrentDateIsBeforeResultCorrectionDeadline)
                    .Enabled = false;
            }

            await permissionValidator.CheckAsync(cancellationToken);
            if (permissionValidator.GetFailedFacts().Count != 0)
            {
                return View(ViewNames.Match.EnterResultNotAllowed,
                    (model.Tournament, permissionValidator.GetFailedFacts()[0].Message));
            }
                
            return View(ViewNames.Match.EnterResult, model);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Building {Model} failed for MatchId '{MatchId}'", nameof(EnterResultViewModel), id);
            return Forbid();
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
        if (!ModelState.IsValid) return NotFound();

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

            var isAuthorizedToOverrule =
                (await _authorizationService.AuthorizeAsync(User, match, Authorization.MatchOperations.OverruleResult))
                .Succeeded;
            if (model.IsOverruling && !isAuthorizedToOverrule)
            {
                return Forbid();
            }

            // sync input with new model instance
            if (!await TryUpdateModelAsync(model))
            {
                return View(ViewNames.Match.EnterResult, model);
            }

            // Check for active tournament and round, deadline for corrections
            var permissionValidator = new MatchResultPermissionValidator(match,
                (_tenantContext,
                    (await IsPlanningMode(model.Tournament!, cancellationToken), model.Round!.IsComplete,
                        DateTime.UtcNow)));
            // Disable the deadline check, if the user is allowed to overrule results
            if (isAuthorizedToOverrule)
                permissionValidator.Facts.First(f =>
                        f.Id == MatchResultPermissionValidator.FactId.CurrentDateIsBeforeResultCorrectionDeadline)
                    .Enabled = false;
            await permissionValidator.CheckAsync(cancellationToken);
            if (permissionValidator.GetFailedFacts().Count != 0)
            {
                return View(ViewNames.Match.EnterResultNotAllowed,
                    (model.Tournament, permissionValidator.GetFailedFacts()[0].Message));
            }

            model.MapFormFieldsToEntity();
            ModelState.Clear();

            if (!await model.ValidateAsync(new MatchResultValidator(model.Match!,
                    (_tenantContext, _timeZoneConverter, (model.Round.MatchRule, model.Round.SetRule)),
                    model.IsOverruling ? MatchValidationMode.Overrule : MatchValidationMode.Default), ModelState))
            {
                return View(ViewNames.Match.EnterResult, model);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Building {Model} failed for MatchId '{ModelId}'. User ID '{CurrentUser}'", nameof(EnterResultViewModel), model?.Id, GetCurrentUserId());
            return Forbid();
        }

        try
        {
            // Validation succeeded, calculate match points if not overruled.
            // Overruled match points are already set in the model.
            if (!model.IsOverruling) match.CalculateMatchPoints(model.Round.MatchRule);
                
            // save the match entity and sets
            var success = await _appDb.MatchRepository.SaveMatchResultAsync(match, cancellationToken);
            _logger.LogInformation("Result for match ID {MatchId} was entered by user ID '{CurrentUser}'", match.Id, GetCurrentUserId());

            TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = success });

            if (success)
            {
                // This preserves OutOfSyncExceptions. We only read what was successfully saved without a re-fetch.
                match.Fields.MarkAsFetched();
                match.Sets.ToList().ForEach(s => s.Fields.MarkAsFetched());
                SendResultNotification(match, false);
                UpdateRanking(match.RoundId, false);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Building {EnterResultViewModel} failed for MatchId '{ModelId}' and  user ID '{CurrentUserId}'", nameof(EnterResultViewModel), model.Id, GetCurrentUserId());
            TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = false });
        }

        // redirect to results overview, where success message is shown
        return Redirect(TenantLink.Action(nameof(Results), nameof(Match))!);
    }

    /// <summary>
    /// Processes the post for a match result.
    /// </summary>
    /// <param name="model"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [Authorize(Policy = Authorization.PolicyName.OverruleResultPolicy)]
    [HttpPost("remove-result/{*segments}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveResult([FromForm] EnterResultViewModel? model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || model?.Id is null) return NotFound();

        try
        {
            var result =  await _appDb.MatchRepository.DeleteMatchResultAsync(model.Id.Value, cancellationToken);
            if (result.Success)
            {
                var match = result.Match!;
                // Set the remark to the user's input when removing a result
                match.Remarks = model.Remarks;

                SendResultNotification(match, true);
                UpdateRanking(match.RoundId, true);
            }

            // redirect to fixture overview, where success message is shown
            TempData.Put<EditFixtureViewModel.FixtureMessage>(nameof(EditFixtureViewModel.FixtureMessage), new EditFixtureViewModel.FixtureMessage { MatchId = model.Id.Value, ChangeSuccess = result.Success });
            return Redirect(TenantLink.Action(nameof(Fixtures), nameof(Match))!);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Deleting result for {Model} failed for MatchId '{ModelId}'. User ID '{CurrentUser}'", nameof(EnterResultViewModel), model.Id, GetCurrentUserId());
            TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = false });
            // redirect to results overview, where success message is shown
            return Redirect(TenantLink.Action(nameof(Results), nameof(Match))!);
        }
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
        if (!ModelState.IsValid) return NotFound();

        var model = new EditFixtureViewModel(await GetPlannedMatchFromDatabase(id, cancellationToken), _timeZoneConverter)
        {
            Tournament = await GetPlanTournament(cancellationToken)
        };

        if (model.PlannedMatch == null || model.Tournament == null)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("No data for fixture id '{ModelId}'. User ID '{CurrentUserId}'.", model.Id, userId);
            return NotFound($"No data for fixture id '{model.Id}'. User ID '{userId}'.");
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

        if (!ModelState.IsValid) return NotFound();

        model = new EditFixtureViewModel(await GetPlannedMatchFromDatabase(model.Id, cancellationToken), _timeZoneConverter)
        {
            Tournament = await GetPlanTournament(cancellationToken)
        };

        if (model.PlannedMatch == null || model.Tournament == null)
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("No data for fixture id '{ModelId}'. User ID '{CurrentUserId}'", model.Id, userId);
            return NotFound($"No data for fixture id '{model.Id}'. User ID '{userId}'");
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
            _logger.LogInformation("Fixture for match id {MatchId} updated successfully for user ID '{CurrentUserId}'", match.Id, GetCurrentUserId());
            if (fixtureIsChanged && !await IsPlanningMode(model.Tournament, cancellationToken)) SendFixtureNotification(match.Id);
        }
        catch (Exception e)
        {
            fixtureMessage.ChangeSuccess = false;
            _logger.LogError(e, "Fixture update for match id {MatchId} failed for user ID '{CurrentUserId}'", match.Id, GetCurrentUserId());
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
            _logger.LogError(e, "Error loading match in '{MethodName}' for user ID '{CurrentUserId}'", nameof(_appDb.MatchRepository.GetPlannedMatchesAsync), GetCurrentUserId());
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
            _logger.LogError(e, "Error adding display data to model '{Model}' for user ID '{CurrentUserId}'", nameof(EditFixtureViewModel), GetCurrentUserId());
            return null;
        }
    }

    private async Task<TournamentEntity?> GetPlanTournament(CancellationToken cancellationToken)
    {
        var tournament =
            await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchPlanTournamentId), cancellationToken);

        if (tournament != null) return tournament;

        _logger.LogError("{Name} '{Id}' does not exist. User ID '{CurrentUserId}'.", nameof(_tenantContext.TournamentContext.MatchPlanTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
        return null;
    }

    private async Task<TournamentEntity?> GetResultTournament(CancellationToken cancellationToken)
    {
        var tournament =
            await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken);

        if (tournament != null) return tournament;

        _logger.LogError("{Name} '{Id}' does not exist. User ID '{CurrentUserId}'.", nameof(_tenantContext.TournamentContext.MatchResultTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId, GetCurrentUserId());
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
            _logger.LogError("Number of found opponents for a match does not equal 2. User ID '{CurrentUserId}'.", GetCurrentUserId());
        }

        var tournament = await _appDb.TournamentRepository.GetTournamentAsync(
            new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken);
        if (tournament == null)
        {
            _logger.LogError("{Name} '{Id}' does not exist", nameof(_tenantContext.TournamentContext.MatchResultTournamentId), _tenantContext.TournamentContext.MatchPlanTournamentId);
        }

        var roundWithRules =
            await _appDb.RoundRepository.GetRoundWithRulesAsync(match.RoundId, cancellationToken);
        if (roundWithRules == null)
        {
            _logger.LogError("Round with {RoundId}='{Id}' does not exist. User ID '{CurrentUserId}'.", _tenantContext.TournamentContext.MatchPlanTournamentId, match.RoundId, GetCurrentUserId());
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
    /// <param name="cancellationToken"></param>
    /// <returns>A match report sheet suitable for a printout, if the match has not already been played.</returns>
    [HttpGet("[action]/{id:long}")]
    public async Task<IActionResult> ReportSheet(long id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        
        MatchReportSheetRow? model = null;
        var cache = _serviceProvider.GetRequiredService<ReportSheetCache>();
        cache.UsePuppeteer = false;
        cache.BrowserKind = BrowserKind.Chromium;
            
        try
        {
            model = await _appDb.MatchRepository.GetMatchReportSheetAsync(_tenantContext.TournamentContext.MatchPlanTournamentId, id, cancellationToken);
            
            if (model == null) return NotFound();

            id = model.Id;
            var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            contentDisposition.SetHttpFileName($"{_localizer["Report Sheet"].Value}_{model.Id}.pdf");
            Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition] =
                contentDisposition.ToString();
            var html = await _razorViewToStringRenderer.RenderViewToStringAsync(
                $"~/Views/{nameof(Match)}/{ViewNames.Match.ReportSheet}.cshtml", model);

            var stream = await cache.GetOrCreatePdf(model, html, cancellationToken);

            if (stream != Stream.Null) // Returning Stream.Null would create an empty page in the web browser
            {
                _logger.LogInformation("PDF file returned for tenant '{Tenant}' and match id '{MatchId}'", _tenantContext.Identifier, id);
                return new FileStreamResult(stream, "application/pdf");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{Method} failed for match ID '{MatchId}'", nameof(ReportSheet), id);
        }
            
        // Not able to render report sheet as PDF: return HTML
        Response.Clear();
        _logger.LogError("HTML content instead of PDF returned for tenant '{Tenant}' and match id '{MatchId}'", _tenantContext.Identifier, id);
        return View(ViewNames.Match.ReportSheet, model);
    }

    private void SendFixtureNotification(long matchId)
    {
        var smt = _sendMailTask.CreateNewInstance();
        smt.SetMessageCreator(new ChangeFixtureCreator
        {
            Parameters =
            {
                CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                ChangedByUserId = GetCurrentUserId() ?? throw new InvalidOperationException("Current user ID must not be null"),
                MatchId = matchId,
            }
        });

        _queue.QueueTask(smt);
    }

    private void SendResultNotification(in MatchEntity match, bool isResultRemoved)
    {
        var smt = _sendMailTask.CreateNewInstance();
        smt.SetMessageCreator(new ResultEnteredCreator
        {
            Parameters =
            {
                CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                ChangedByUserId = GetCurrentUserId() ?? throw new InvalidOperationException("Current user ID must not be null"),
                Match = match,
                IsResultRemoved = isResultRemoved
            }
        });

        _queue.QueueTask(smt);
    }

    private void UpdateRanking(in long roundId, bool forceUpdate)
    {
        _rankingUpdateTask.TenantContext = _tenantContext;
        _rankingUpdateTask.RoundId = roundId;
        _rankingUpdateTask.Timeout = TimeSpan.FromMinutes(2);
        _rankingUpdateTask.EnforceUpdate = forceUpdate;
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
            .GetTournamentStartAsync(tournament.Id, cancellationToken)
            .ContinueWith(
                t => DateTime.UtcNow < t.Result.AddDays(-_tenantContext.SiteContext.MatchNotifications.DaysBeforeNextMatch),
                TaskContinuationOptions.OnlyOnRanToCompletion);
    }
}
