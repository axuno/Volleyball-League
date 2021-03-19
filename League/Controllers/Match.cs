using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using League.BackgroundTasks;
using League.Emailing.Creators;
using League.Helpers;
using League.Models.MatchViewModels;
using MailMergeLib.AspNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using PuppeteerSharp.Media;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace League.Controllers
{
    [Route("{organization:MatchingTenant}/[controller]")]
    public class Match : AbstractController
    {
        private readonly ITenantContext _tenantContext;
        private readonly AppDb _appDb;
        private readonly IStringLocalizer<Match> _localizer;
        private readonly IAuthorizationService _authorizationService;
        private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
        private readonly ILogger<Match> _logger;
        private readonly Axuno.BackgroundTask.IBackgroundQueue _queue;
        private readonly SendEmailTask _sendMailTask;
        private readonly RankingUpdateTask _rankingUpdateTask;
        private readonly RazorViewToStringRenderer _razorViewToStringRenderer;
        
        public Match(ITenantContext tenantContext, IStringLocalizer<Match> localizer, IAuthorizationService authorizationService,
            Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, Axuno.BackgroundTask.IBackgroundQueue queue,
            SendEmailTask sendMailTask, RankingUpdateTask rankingUpdateTask, RazorViewToStringRenderer razorViewToStringRenderer, ILogger<Match> logger)
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

        [HttpGet("")]
        public IActionResult Index()
        {
            return Redirect(Url.Action(nameof(Results), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue }));
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Fixtures(CancellationToken cancellationToken)
        {
            var model = new FixturesViewModel(_timeZoneConverter)
            {
                Tournament = await GetPlanTournament(cancellationToken),
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
                _logger.LogCritical($"{nameof(_tenantContext.TournamentContext.MatchPlanTournamentId)} '{_tenantContext.TournamentContext.MatchPlanTournamentId}' does not exist. User ID: '{0}'", GetCurrentUserId());
            }

            return View(ViewNames.Match.Fixtures, model);
        }

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
                _logger.LogCritical(e, $"Error creating {nameof(Calendar)} for user ID '{0}'", GetCurrentUserId());
                return NoContent();
            }
        }

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
                _logger.LogCritical(e, $"Error creating {nameof(PublicCalendar)} for user ID '{0}'", GetCurrentUserId());
                return NoContent();
            }
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> Results(CancellationToken cancellationToken)
        {
            var model = new ResultsViewModel(_timeZoneConverter)
            {
                Tournament =
                    await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchPlanTournamentId),
                        cancellationToken),
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
                _logger.LogCritical($"{nameof(_tenantContext.TournamentContext.MatchPlanTournamentId)} '{_tenantContext.TournamentContext.MatchPlanTournamentId}' does not exist. User ID '{0}'", GetCurrentUserId());
            }
            return View(ViewNames.Match.Results, model);
        }

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

                var permissionValidator = new MatchResultPermissionValidator(match, (_tenantContext, (model.Tournament.IsPlanningMode, model.Round.IsComplete, DateTime.UtcNow)));
                await permissionValidator.CheckAsync(cancellationToken);
                if (permissionValidator.GetFailedFacts().Any())
                {
                    return View(ViewNames.Match.EnterResultNotAllowed,
                        (model.Tournament, permissionValidator.GetFailedFacts().First().Message));
                }
                
                return View(ViewNames.Match.EnterResult, model);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Building {nameof(EnterResultViewModel)} failed for MatchId '{id}'");
                throw;
            }
        }

        [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
        [HttpPost("enter-result/{*segments}")]
        public async Task<IActionResult> EnterResult([FromForm] EnterResultViewModel model, CancellationToken cancellationToken)
        {
            MatchEntity match;
            try
            {
                match = await _appDb.MatchRepository.GetMatchWithSetsAsync(model.Id, cancellationToken);
                if (match == null)
                {
                    return NotFound($"Match id '{model.Id}' not found.");
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

                var permissionValidator = new MatchResultPermissionValidator(match, (_tenantContext, (model.Tournament.IsPlanningMode, model.Round.IsComplete, DateTime.UtcNow)));
                await permissionValidator.CheckAsync(cancellationToken);
                if (permissionValidator.GetFailedFacts().Any())
                {
                    return View(ViewNames.Match.EnterResultNotAllowed,
                        (model.Tournament, permissionValidator.GetFailedFacts().First().Message));
                }

                model.MapFormFieldsToEntity();
                ModelState.Clear();

                if (!await model.ValidateAsync(new MatchResultValidator(model.Match,
                    (_tenantContext, _timeZoneConverter, (model.Round.MatchRule, model.Round.SetRule))), ModelState))
                {
                    return View(ViewNames.Match.EnterResult, model);
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Building {nameof(EnterResultViewModel)} failed for MatchId '{model.Id}'. User ID '{0}'", GetCurrentUserId());
                throw;
            }

            try
            {
                // validation succeeded
                match.CalculateMatchPoints(model.Round.MatchRule);
                
                // save the match entity and sets
                var success = await _appDb.MatchRepository.SaveMatchResultAsync(match, cancellationToken);
                _logger.LogInformation($"Result for match ID {match.Id} was entered by user ID '{0}'", GetCurrentUserId());

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
                _logger.LogCritical(e, $"Building {nameof(EnterResultViewModel)} failed for MatchId '{model.Id}' and  user ID '{0}'", GetCurrentUserId());
                TempData.Put<EnterResultViewModel.MatchResultMessage>(nameof(EnterResultViewModel.MatchResultMessage),
                    new EnterResultViewModel.MatchResultMessage { MatchId = model.Id, ChangeSuccess = false });
            }

            // redirect to results overview, where success message is shown
            return RedirectToAction(nameof(Results), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue });
        }

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
                _logger.LogInformation(msg);
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

        [Authorize(Policy = Authorization.PolicyName.MatchPolicy)]
        [HttpPost("edit-fixture/{*segments}")]
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
                _logger.LogInformation(msg);
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
            match.SetPlannedStart(model.MatchDate.HasValue && model.MatchTime.HasValue
                ? _timeZoneConverter.ToUtc(model.MatchDate.Value.Add(model.MatchTime.Value))
                : default(DateTime?), _tenantContext.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch);
            match.SetVenueId(model.VenueId);
            if (match.IsDirty) match.ChangeSerial += 1;

            ModelState.Clear();

            // Todo: This business logic should rather go into settings
            _tenantContext.TournamentContext.FixtureRuleSet.PlannedMatchTimeMustStayInCurrentLegBoundaries = model.Tournament.IsPlanningMode;

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
                _logger.LogInformation($"Fixture for match id {match.Id} updated successfully for user ID '{0}'", GetCurrentUserId());
                if (fixtureIsChanged) SendFixtureNotification(match.Id);
            }
            catch (Exception e)
            {
                fixtureMessage.ChangeSuccess = false;
                _logger.LogCritical(e, "Fixture update for match id {0} failed for user ID '{1}'", match.Id, GetCurrentUserId());
            }

            // redirect to fixture overview, where success message is shown
            TempData.Put<EditFixtureViewModel.FixtureMessage>(nameof(EditFixtureViewModel.FixtureMessage), fixtureMessage);
            return RedirectToAction(nameof(Fixtures), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue });
        }

        /// <summary>
        /// Fills a new <see cref="MatchEntity"/> with data retrieved from the database.
        /// </summary>
        /// <param name="currentData"></param>
        /// <returns>Returns a <see cref="MatchEntity"/> ready to be processed further.</returns>
        private MatchEntity FillMatchEntity(PlannedMatchRow currentData)
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

        private async Task<PlannedMatchRow> GetPlannedMatchFromDatabase(long matchId, CancellationToken cancellationToken)
        {
            try
            {
                return (await _appDb.MatchRepository.GetPlannedMatchesAsync(
                    new PredicateExpression(PlannedMatchFields.Id == matchId), cancellationToken)).FirstOrDefault();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Error loading match in '{nameof(_appDb.MatchRepository.GetPlannedMatchesAsync)}' for user ID '{0}'", GetCurrentUserId());
                return null;
            }
        }

        private async Task<EditFixtureViewModel> AddDisplayDataToEditFixtureViewModel(EditFixtureViewModel model, CancellationToken cancellationToken)
        {
            try
            {
                // Get all venues and teams for a tournament and select in-memory is 40% faster compared to database selections
                var venuesWithTeams = await _appDb.VenueRepository.GetVenueTeamRowsAsync(new PredicateExpression(VenueTeamFields.TournamentId == _tenantContext.TournamentContext.MatchPlanTournamentId),
                    cancellationToken);
                var allVenues = await _appDb.VenueRepository.GetVenuesAsync(null, cancellationToken);

                // get venue entities of match teams
                var venueIdsOfMatchTeams = venuesWithTeams
                    .Where(vwt => vwt.TeamId ==  model.PlannedMatch.HomeTeamId || vwt.TeamId == model.PlannedMatch.GuestTeamId).Select(vwt => vwt.VenueId).Distinct();
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
                _logger.LogCritical(e, $"Error adding display data to model '{nameof(EditFixtureViewModel)}' for user ID '{GetCurrentUserId()}'");
                return null;
            }
        }

        private async Task<TournamentEntity> GetPlanTournament(CancellationToken cancellationToken)
        {
            var tournament =
                await _appDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchPlanTournamentId), cancellationToken);

            if (tournament != null) return tournament;

            _logger.LogCritical($"{nameof(_tenantContext.TournamentContext.MatchPlanTournamentId)} '{_tenantContext.TournamentContext.MatchPlanTournamentId}' does not exist. User ID '{0}'.", GetCurrentUserId());
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
                _logger.LogCritical("Number of found opponents for a match does not equal 2. User ID '{0}'.", GetCurrentUserId());
            }

            var tournament = await _appDb.TournamentRepository.GetTournamentAsync(
                new PredicateExpression(TournamentFields.Id == _tenantContext.TournamentContext.MatchResultTournamentId), cancellationToken);
            if (tournament == null)
            {
                _logger.LogCritical($"{nameof(_tenantContext.TournamentContext.MatchResultTournamentId)} '{_tenantContext.TournamentContext.MatchPlanTournamentId}' does not exist");
            }

            var roundWithRules =
                await _appDb.RoundRepository.GetRoundWithRulesAsync(match.RoundId, cancellationToken);
            if (roundWithRules == null)
            {
                _logger.LogCritical($"Round with {nameof(match.RoundId)}='{match.RoundId}' does not exist. User ID '{GetCurrentUserId()}'.");
            }

            return tournament != null && roundWithRules != null && teamInRound.Count == 2
                ? new EnterResultViewModel(tournament, roundWithRules, match, roundWithRules.MatchRule, teamInRound,
                    _timeZoneConverter)
                { ReturnUrl = GetReturnUrl()}
                : throw new Exception($"{nameof(EnterResultViewModel)} could not be initiated");
        }


        [HttpGet("[action]/{id:long}")]
        public async Task<IActionResult> ReportSheet(long id, CancellationToken cancellationToken)
        {
            var pathToChromium = Path.Combine(Directory.GetCurrentDirectory(), @"Chromium-Win\chrome.exe");
            MatchReportSheetRow model = null;
            try
            {
                model = await _appDb.MatchRepository.GetMatchReportSheetAsync(_tenantContext.TournamentContext.MatchPlanTournamentId, id, cancellationToken);

                if (model == null) return NotFound();

                if (System.IO.File.Exists(pathToChromium))
                {
                    #region ** Puppeteer PDF generation using Chromium **

                    var options = new PuppeteerSharp.LaunchOptions
                    {
                        Headless = true, Args = new[] {"--no-sandbox", "--disable-gpu", "--disable-extensions"},
                        ExecutablePath = pathToChromium, Timeout = 10000
                    };
                    // Use Puppeteer as a wrapper for the Chromium browser, which can generate PDF from HTML
                    await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options).ConfigureAwait(false);
                    await using var page = await browser.NewPageAsync().ConfigureAwait(false);
                    // page.GoToAsync("url");
                    var html = await _razorViewToStringRenderer.RenderViewToStringAsync(
                        $"~/Views/{nameof(Match)}/{ViewNames.Match.ReportSheet}.cshtml", model);
                    await page.SetContentAsync(html); // Bootstrap 4 is loaded from CDN
                    var contentDisposition = new Microsoft.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                    contentDisposition.SetHttpFileName($"{_localizer["Report Sheet"].Value} {model.Id}.pdf");
                    Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.ContentDisposition] =
                        contentDisposition.ToString();
                    return new FileStreamResult(
                        await page.PdfStreamAsync(new PuppeteerSharp.PdfOptions
                            {Scale = 1.0M, Format = PaperFormat.A4}),
                        "application/pdf");

                    #endregion
                }
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"{nameof(ReportSheet)} failed for match ID '{id}'");
            }
            
            // without Chromium installed or throwing exception: return HTML
            return View(ViewNames.Match.ReportSheet, model);
        }

        private void SendFixtureNotification(long matchId)
        {
            _sendMailTask.SetMessageCreator(new ChangeFixtureCreator
            {
                Parameters =
                {
                    CultureInfo = CultureInfo.DefaultThreadCurrentUICulture ?? CultureInfo.CurrentCulture,
                    ChangedByUserId = GetCurrentUserId(),
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
                    ChangedByUserId = GetCurrentUserId(),
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
            var returnUrl = HttpContext.Request.GetTypedHeaders().Referer.ToString();
            return returnUrl == Url.Action(nameof(Results), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue }, Url.ActionContext.HttpContext.Request.Scheme)
                ? Url.Action(nameof(Results), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue }) // editing a result is exceptional
                : Url.Action(nameof(Fixtures), nameof(Match), new { Organization = _tenantContext.SiteContext.UrlSegmentValue }); // coming from fixtures is normal
        }
    }
}