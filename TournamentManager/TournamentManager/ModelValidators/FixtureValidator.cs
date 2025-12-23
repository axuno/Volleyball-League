using System.ComponentModel;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class FixtureValidator : AbstractValidator<MatchEntity, (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, PlannedMatchRow PlannedMatch), FixtureValidator.FactId>
{
    private List<TeamEntity> _teamsInMatch = [];
    private readonly FactResult _successResult = new() { Message = string.Empty, Success = true };

    public enum FactId
    {
        PlannedStartIsSet,
        PlannedStartNotExcluded,
        PlannedStartWithinRoundLegs,
        PlannedStartIsFutureDate,
        PlannedStartWithinDesiredTimeRange,
        PlannedStartTeamsAreNotBusy,
        PlannedStartWeekdayIsTeamWeekday,
        PlannedVenueIsSet,
        PlannedVenueNotOccupiedWithOtherMatch,
        PlannedVenueIsRegisteredVenueOfTeam
    }

    /// <summary>
    /// Gets or sets the current date and time for validation comparisons.
    /// </summary>
    public DateTime CurrentDateTimeUtc { get; set; }

    public FixtureValidator(MatchEntity model, (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, PlannedMatchRow PlannedMatch) data, DateTime currentDateTimeUtc) : base(model, data)
    {
        CurrentDateTimeUtc = currentDateTimeUtc;
        CreateFacts();
    }

    private async Task LoadTeamsInMatch(CancellationToken cancellationToken)
    {
        _teamsInMatch = await Data.TenantContext.DbContext.AppDb.TeamRepository.GetTeamEntitiesAsync(
            new(TeamFields.Id == Model.HomeTeamId |
                TeamFields.Id == Model.GuestTeamId),
            cancellationToken).ConfigureAwait(false);
    }

    private void CreateFacts()
    {
        Facts.Add(PlannedStartIsSet());
        Facts.Add(PlannedStartNotExcluded());
        Facts.Add(PlannedStartIsFutureDate());
        Facts.Add(PlannedStartWithinRoundLegs());
        Facts.Add(PlannedStartWithinDesiredTimeRange());
        Facts.Add(PlannedStartTeamsAreNotBusy());
        Facts.Add(PlannedStartWeekdayIsTeamWeekday());
        Facts.Add(PlannedVenueIsSet());
        Facts.Add(PlannedVenueNotOccupiedWithOtherMatch());
        Facts.Add(PlannedVenueIsRegisteredVenueOfTeam());
    }

    private Fact<FactId> PlannedVenueIsRegisteredVenueOfTeam()
    {
        return new()
        {
            Id = FactId.PlannedVenueIsRegisteredVenueOfTeam,
            FieldNames = [nameof(Model.VenueId)],
            Enabled = true,
            Type = FactType.Warning,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            await LoadTeamsInMatch(cancellationToken).ConfigureAwait(false);
            return new()
            {
                Message = FixtureValidatorResource.ResourceManager.GetString(
                    nameof(FactId.PlannedVenueIsRegisteredVenueOfTeam)) ?? string.Empty,
                Success = !Model.VenueId.HasValue || _teamsInMatch.Exists(tim =>
                    !tim.VenueId.HasValue || tim.VenueId == Model.VenueId)
            };
        }
    }

    private Fact<FactId> PlannedVenueNotOccupiedWithOtherMatch()
    {
        return new()
        {
            Id = FactId.PlannedVenueNotOccupiedWithOtherMatch,
            FieldNames = [nameof(Model.VenueId)],
            Type = FactType.Warning,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var otherMatch = Model.VenueId.HasValue
                ? (await Data.TenantContext.DbContext.AppDb.VenueRepository.GetOccupyingMatchesAsync(
                        Model.VenueId.Value, new(Model.PlannedStart, Model.PlannedEnd),
                        Data.TenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken)
                    .ConfigureAwait(false))
                .Find(m => m.Id != Model.Id)
                : null;

            return new()
            {
                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                    nameof(FactId.PlannedVenueNotOccupiedWithOtherMatch)) ?? string.Empty, otherMatch?.HomeTeamNameForRound, otherMatch?.GuestTeamNameForRound),
                Success = otherMatch == null
            };
        }
    }

    private Fact<FactId> PlannedVenueIsSet()
    {
        return new()
        {
            Id = FactId.PlannedVenueIsSet,
            FieldNames = [nameof(Model.VenueId)],
            Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedVenueMustBeSet,
            Type = FactType.Critical,
            CheckAsync = async (cancellationToken) => 
                new()
                {
                    Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedVenueIsSet)) ?? string.Empty,
                    Success = Model.VenueId.HasValue
                              && await Data.TenantContext.DbContext.AppDb.VenueRepository.IsValidVenueIdAsync(Model.VenueId.Value, cancellationToken).ConfigureAwait(false)
                }
        };
    }

    private Fact<FactId> PlannedStartWeekdayIsTeamWeekday()
    {
        return new()
        {
            Id = FactId.PlannedStartWeekdayIsTeamWeekday,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = true,
            Type = FactType.Warning,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            if (!Model.VenueId.HasValue || !Model.PlannedStart.HasValue) return _successResult;
            await LoadTeamsInMatch(cancellationToken).ConfigureAwait(false);
                        
            var homeTeam = _teamsInMatch.Find(m => m.Id == Model.HomeTeamId);
            var guestTeam = _teamsInMatch.Find(m => m.Id == Model.GuestTeamId);
            var plannedStartDayOfWeek = (int?) Model.PlannedStart.Value.DayOfWeek;

            if (homeTeam?.MatchDayOfWeek == null || guestTeam?.MatchDayOfWeek == null) return _successResult;
                    
            // 2 teams using the same venue have a match,
            // and the planned start week day is either the home or guest team weekday
            if (homeTeam.VenueId == guestTeam.VenueId && Model.VenueId == homeTeam.VenueId
                                                      && (plannedStartDayOfWeek == homeTeam.MatchDayOfWeek || plannedStartDayOfWeek == guestTeam.MatchDayOfWeek))
            {
                return _successResult;
            }
                    
            if (plannedStartDayOfWeek !=
                homeTeam.MatchDayOfWeek
                && Model.VenueId == homeTeam.VenueId)
            {
                return new()
                {
                    Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                            nameof(FactId.PlannedStartWeekdayIsTeamWeekday)) ?? string.Empty,
                        Model.PlannedStart?.ToString("dddd"), Data.PlannedMatch.HomeTeamNameForRound),
                    Success = false
                };
            }

            if (plannedStartDayOfWeek !=
                guestTeam.MatchDayOfWeek
                && Model.VenueId == guestTeam.VenueId)
            {
                return new()
                {
                    Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                            nameof(FactId.PlannedStartWeekdayIsTeamWeekday)) ?? string.Empty,
                        Model.PlannedStart?.ToString("dddd"), Data.PlannedMatch.GuestTeamNameForRound),
                    Success = false
                };
            }

            return _successResult;
        }
    }

    private Fact<FactId> PlannedStartTeamsAreNotBusy()
    {
        return new()
        {
            Id = FactId.PlannedStartTeamsAreNotBusy,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var busyTeams = Model.PlannedStart.HasValue
                ? await Data.TenantContext.DbContext.AppDb.MatchRepository.AreTeamsBusyAsync(
                    Model, Data.TenantContext.TournamentContext.FixtureRuleSet.UseOnlyDatePartForTeamFreeBusyTimes,
                    Data.TenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken).ConfigureAwait(false)
                : [];

            return busyTeams.Length > 0
                ? new()
                {
                    Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                            nameof(FactId.PlannedStartTeamsAreNotBusy)) ?? string.Empty,
                        Data.PlannedMatch.HomeTeamId == busyTeams[0]
                            ? Data.PlannedMatch.HomeTeamNameForRound
                            : Data.PlannedMatch.GuestTeamNameForRound),
                    Success = busyTeams.Length == 0
                }
                : _successResult;
        }
    }

    private Fact<FactId> PlannedStartWithinDesiredTimeRange()
    {
        return new()
        {
            Id = FactId.PlannedStartWithinDesiredTimeRange,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = true, // set time range to 24h to always succeed the test
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedStartWithinDesiredTimeRange)) ?? string.Empty, Data.TenantContext.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MinDayTime.ToShortTimeString(), Data.TenantContext.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MaxDayTime.ToShortTimeString()),
                    // regular start time is given in local time
                    Success = !Model.PlannedStart.HasValue || 
                              Data.TimeZoneConverter.ToZonedTime(Model.PlannedStart)?.DateTimeOffset.TimeOfDay >= Data.TenantContext.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MinDayTime &&
                              Data.TimeZoneConverter.ToZonedTime(Model.PlannedStart)?.DateTimeOffset.TimeOfDay <= Data.TenantContext.TournamentContext.FixtureRuleSet.RegularMatchStartTime.MaxDayTime
                })
        };
    }

    private Fact<FactId> PlannedStartWithinRoundLegs()
    {
        return new()
        {
            Id = FactId.PlannedStartWithinRoundLegs,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var round =
                await Data.TenantContext.DbContext.AppDb.RoundRepository.GetRoundWithLegsAsync(Model.RoundId,
                    cancellationToken).ConfigureAwait(false) ??
                throw new InvalidOperationException($"Round Id '{Model.RoundId}' not found.");

            if (!Model.PlannedStart.HasValue || round.RoundLegs.Count == 0) {
                return _successResult;
            }

            round.RoundLegs.Sort((int) RoundLegFieldIndex.SequenceNo, ListSortDirection.Ascending);

            var stayWithinLegDates = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedMatchTimeMustStayInCurrentLegBoundaries;
            var currentLeg = round.RoundLegs.First(rl => rl.SequenceNo == Model.LegSequenceNo);

            // Note: EndDateTime includes the full day
            if ((stayWithinLegDates && currentLeg.ContainsDate(Model.PlannedStart))
                ||
                (!stayWithinLegDates && round.RoundLegs.GetRoundLegForDate(Model.PlannedStart) != null))
            {
                return _successResult;
            }

            var displayPeriods= string.Empty;
            if (stayWithinLegDates)
            {
                displayPeriods =
                    $"{currentLeg.StartDateTime.ToShortDateString()} - {currentLeg.EndDateTime.ToShortDateString()}";
            }
            else
            {
                            
                const string joinWith = ", ";
                displayPeriods = round.RoundLegs.Aggregate(displayPeriods,
                    (current, leg) =>
                        current +
                        $"{Data.TimeZoneConverter.ToZonedTime(leg.StartDateTime)?.DateTimeOffset.DateTime.ToShortDateString()} - {Data.TimeZoneConverter.ToZonedTime(leg.EndDateTime)?.DateTimeOffset.DateTime.ToShortDateString()}{joinWith}");

                displayPeriods = displayPeriods[..^joinWith.Length];
            }

            return new()
            {
                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                    nameof(FactId.PlannedStartWithinRoundLegs)) ?? string.Empty, round.Description, displayPeriods),
                Success = false
            };
        }
    }

    private Fact<FactId> PlannedStartIsFutureDate()
    {
        return new()
        {
            Id = FactId.PlannedStartIsFutureDate,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = true,
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedStartIsFutureDate)) ?? string.Empty,
                    Success = !Model.PlannedStart.HasValue || Model.PlannedStart.Value.Date > CurrentDateTimeUtc.Date
                })
        };
    }

    private Fact<FactId> PlannedStartNotExcluded()
    {
        return new()
        {
            Id = FactId.PlannedStartNotExcluded,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.CheckForExcludedMatchDateTime,
            Type = FactType.Warning,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            if (!Model.PlannedStart.HasValue) return _successResult;
            // MatchEntity.RoundId, MatchEntity.HomeTeamId, MatchEntity.GuestTeam must be set for the check to work
            var excluded =
                await Data.TenantContext.DbContext.AppDb.ExcludedMatchDateRepository.GetExcludedMatchDateAsync(Model,
                    Data.TenantContext.TournamentContext.MatchPlanTournamentId,
                    cancellationToken).ConfigureAwait(false);

            if (excluded == null)
            {
                return _successResult;
            }
                        
            var zonedPlannedStart = Data.TimeZoneConverter.ToZonedTime(Model.PlannedStart)?.DateTimeOffset.DateTime;

            return new()
            {
                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                        nameof(FactId.PlannedStartNotExcluded)) ?? string.Empty,
                    Data.TenantContext.TournamentContext.FixtureRuleSet.UseOnlyDatePartForTeamFreeBusyTimes
                        ? zonedPlannedStart?.ToShortDateString()
                        : $"{zonedPlannedStart?.ToShortDateString()} {zonedPlannedStart?.ToShortTimeString()}",
                    excluded.Reason),
                Success = false
            };
        }
    }

    private Fact<FactId> PlannedStartIsSet()
    {
        return new()
        {
            Id = FactId.PlannedStartIsSet,
            FieldNames = [nameof(Model.PlannedStart)],
            Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedMatchDateTimeMustBeSet,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult {
                    Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedStartIsSet)) ?? string.Empty,
                    Success = Model.PlannedStart.HasValue })
        };
    }
}
