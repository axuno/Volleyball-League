using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators
{
    public class FixtureValidator : AbstractValidator<MatchEntity, (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, PlannedMatchRow PlannedMatch), FixtureValidator.FactId>
    {
        private List<TeamEntity> _teamsInMatch = new();
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
                           new PredicateExpression(TeamFields.Id == Model.HomeTeamId |
                                                   TeamFields.Id == Model.GuestTeamId),
                           cancellationToken);
        }

        private void CreateFacts()
        {
            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartIsSet,
                    FieldNames = new []{ nameof(Model.PlannedStart) },
                    Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedMatchDateTimeMustBeSet,
                    Type = FactType.Critical,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult {
                            Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedStartIsSet)) ?? string.Empty,
                            Success = Model.PlannedStart.HasValue })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartNotExcluded,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
                    Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.CheckForExcludedMatchDateTime,
                    Type = FactType.Warning,
                    CheckAsync = async (cancellationToken) => 
                    {
                        if (!Model.PlannedStart.HasValue) return _successResult;
                        // MatchEntity.RoundId, MatchEntity.HomeTeamId, MatchEntity.GuestTeam must be set for the check to work
                        var excluded =
                            await Data.TenantContext.DbContext.AppDb.ExcludedMatchDateRepository.GetExcludedMatchDateAsync(Model,
                                Data.TenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken);

                        if (excluded == null)
                        {
                            return _successResult;
                        }
                        
                        var zonedPlannedStart = Data.TimeZoneConverter.ToZonedTime(Model.PlannedStart)?.DateTimeOffset.DateTime;

                        return new FactResult
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
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartIsFutureDate,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
                    Enabled = true,
                    Type = FactType.Warning,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedStartIsFutureDate)) ?? string.Empty,
                            Success = !Model.PlannedStart.HasValue || Model.PlannedStart.Value.Date > CurrentDateTimeUtc.Date
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartWithinRoundLegs,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
                    Enabled = true,
                    Type = FactType.Error,
                    CheckAsync = async (cancellationToken) =>
                    {
                        var round =
                            await Data.TenantContext.DbContext.AppDb.RoundRepository.GetRoundWithLegsAsync(Model.RoundId,
                                cancellationToken) ?? throw new InvalidOperationException($"Round Id '{Model.RoundId}' not found.");

                        if (!Model.PlannedStart.HasValue || round.RoundLegs.Count == 0) { return _successResult; }

                        round.RoundLegs.Sort((int) RoundLegFieldIndex.SequenceNo, ListSortDirection.Ascending);

                        var stayWithinLegDates = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedMatchTimeMustStayInCurrentLegBoundaries;
                        var currentLeg = round.RoundLegs.First(rl => rl.SequenceNo == Model.LegSequenceNo);

                        if ((stayWithinLegDates &&
                             new DateTimePeriod(currentLeg.StartDateTime, currentLeg.EndDateTime).Contains(
                                 Model.PlannedStart))
                            ||
                            (!stayWithinLegDates && round.RoundLegs.Any(rl =>
                                 new DateTimePeriod(rl.StartDateTime, rl.EndDateTime).Contains(Model.PlannedStart))))
                        {
                            return _successResult;
                        }

                        var displayPeriods= string.Empty;
                        if (stayWithinLegDates)
                        {
                            displayPeriods =
                                $"{currentLeg.StartDateTime.ToShortDateString()} - {currentLeg.StartDateTime.ToShortDateString()}";
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

                        return new FactResult
                        {
                            Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                                nameof(FactId.PlannedStartWithinRoundLegs)) ?? string.Empty, round.Description, displayPeriods),
                            Success = false
                        };
                    }
                }
            );

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartWithinDesiredTimeRange,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
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
                }
            );

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartTeamsAreNotBusy,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
                    Enabled = true,
                    Type = FactType.Error,
                    CheckAsync = async (cancellationToken) =>
                    {
                        var busyTeams = Model.PlannedStart.HasValue
                            ? await Data.TenantContext.DbContext.AppDb.MatchRepository.AreTeamsBusyAsync(
                                Model, Data.TenantContext.TournamentContext.FixtureRuleSet.UseOnlyDatePartForTeamFreeBusyTimes, Data.TenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken)
                            : Array.Empty<long>();

                        return busyTeams.Length > 0
                            ? new FactResult
                            {
                                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                                        nameof(FactId.PlannedStartTeamsAreNotBusy)) ?? string.Empty,
                                    Data.PlannedMatch.HomeTeamId == busyTeams.First()
                                        ? Data.PlannedMatch.HomeTeamNameForRound
                                        : Data.PlannedMatch.GuestTeamNameForRound),
                                Success = busyTeams.Length == 0
                            }
                            : _successResult;
                    }
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedStartWeekdayIsTeamWeekday,
                    FieldNames = new[] { nameof(Model.PlannedStart) },
                    Enabled = true,
                    Type = FactType.Warning,
                    CheckAsync = async (cancellationToken) =>
                    {
                        if (!Model.VenueId.HasValue || !Model.PlannedStart.HasValue) return _successResult;
                        await LoadTeamsInMatch(cancellationToken);
                        
                        var homeTeam = _teamsInMatch.FirstOrDefault(m => m.Id == Model.HomeTeamId);
                        var guestTeam = _teamsInMatch.FirstOrDefault(m => m.Id == Model.GuestTeamId);

                        if (homeTeam?.MatchDayOfWeek == null || guestTeam?.MatchDayOfWeek == null) return _successResult;

                        if ((int?)Model.PlannedStart.Value.DayOfWeek !=
                             homeTeam.MatchDayOfWeek
                             && Model.VenueId == homeTeam.VenueId)
                        {
                            return new FactResult
                            {
                                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                                    nameof(FactId.PlannedStartWeekdayIsTeamWeekday)) ?? string.Empty,
                                    Model.PlannedStart?.ToString("dddd"), Data.PlannedMatch.HomeTeamNameForRound),
                                Success = false
                            };
                        }

                        if ((int?)Model.PlannedStart.Value.DayOfWeek !=
                            guestTeam.MatchDayOfWeek
                            && Model.VenueId == guestTeam.VenueId)
                        {
                            return new FactResult
                            {
                                Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                                        nameof(FactId.PlannedStartWeekdayIsTeamWeekday)) ?? string.Empty,
                                    Model.PlannedStart?.ToString("dddd"), Data.PlannedMatch.GuestTeamNameForRound),
                                Success = false
                            };
                        }

                        return _successResult;
                    }
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedVenueIsSet,
                    FieldNames = new[] { nameof(Model.VenueId) },
                    Enabled = Data.TenantContext.TournamentContext.FixtureRuleSet.PlannedVenueMustBeSet,
                    Type = FactType.Critical,
                    CheckAsync = async (cancellationToken) => 
                        new FactResult
                        {
                            Message = FixtureValidatorResource.ResourceManager.GetString(nameof(FactId.PlannedVenueIsSet)) ?? string.Empty,
                            Success = Model.VenueId.HasValue && await Data.TenantContext.DbContext.AppDb.VenueRepository.IsValidVenueIdAsync(Model.VenueId.Value, cancellationToken)
                        }
                }
            );

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedVenueNotOccupiedWithOtherMatch,
                    FieldNames = new[] { nameof(Model.VenueId) },
                    Type = FactType.Warning,
                    CheckAsync = async (cancellationToken) =>
                    {
                        var otherMatch = Model.VenueId.HasValue
                            ? (await Data.TenantContext.DbContext.AppDb.VenueRepository.GetOccupyingMatchesAsync(
                                Model.VenueId.Value, new DateTimePeriod(Model.PlannedStart, Model.PlannedEnd),
                                Data.TenantContext.TournamentContext.MatchPlanTournamentId, cancellationToken))
                            .FirstOrDefault(m => m.Id != Model.Id)
                            : null;

                        return new FactResult
                        {
                            Message = string.Format(FixtureValidatorResource.ResourceManager.GetString(
                                nameof(FactId.PlannedVenueNotOccupiedWithOtherMatch)) ?? string.Empty, otherMatch?.HomeTeamNameForRound, otherMatch?.GuestTeamNameForRound),
                            Success = otherMatch == null
                        };
                    }
                }
            );

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.PlannedVenueIsRegisteredVenueOfTeam,
                    FieldNames = new[] { nameof(Model.VenueId) },
                    Enabled = true,
                    Type = FactType.Warning,
                    CheckAsync = async (cancellationToken) =>
                    {
                        await LoadTeamsInMatch(cancellationToken);
                        return new FactResult
                        {
                            Message = FixtureValidatorResource.ResourceManager.GetString(
                                nameof(FactId.PlannedVenueIsRegisteredVenueOfTeam)) ?? string.Empty,
                            Success = !Model.VenueId.HasValue || _teamsInMatch.Any(tim =>
                                          !tim.VenueId.HasValue || tim.VenueId == Model.VenueId)
                        };
                    }
                }
            );
        }
    }
}
