using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class MatchResultValidator : AbstractValidator<MatchEntity, (ITenantContext TenantContext,
    Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules), MatchResultValidator.FactId>
{
    internal DateTime Today { get; set; } = DateTime.UtcNow; // used for unit tests
    public enum FactId
    {
        RealMatchDateIsSet,
        RealMatchDateTodayOrBefore,
        RealMatchDateWithinRoundLegs,
        RealMatchDateEqualsFixture,
        RealMatchDurationIsPlausible,
        SetsValidatorSuccessful
    }

    public MatchResultValidator(MatchEntity model,
        (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules) data) : base(model, data)
    {
        SetsValidator = new SetsValidator(Model.Sets, (Data.TenantContext, Data.Rules));
        CreateFacts();
    }

    public SetsValidator SetsValidator { get; }

    public void CreateFacts()
    {
        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RealMatchDateIsSet,
                FieldNames = new[] { nameof(Model.RealStart), nameof(Model.RealEnd) },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateIsSet)) ?? string.Empty,
                        Success = Model.RealStart.HasValue && Model.RealEnd.HasValue
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RealMatchDateTodayOrBefore,
                FieldNames = new[] { nameof(Model.RealStart), nameof(Model.RealEnd) },
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateTodayOrBefore)) ?? string.Empty,
                        Success = Model.RealStart?.Date <= Today.Date && Model.RealEnd?.Date <= Today.Date
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RealMatchDateWithinRoundLegs,
                FieldNames = new[] {nameof(Model.RealStart), nameof(Model.RealEnd) },
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
                {
                    var successResult = new FactResult
                    {
                        Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateWithinRoundLegs)) ?? string.Empty,
                        Success = true
                    };

                    var round =
                        await Data.TenantContext.DbContext.AppDb.RoundRepository.GetRoundWithLegsAsync(Model.RoundId,
                            cancellationToken) ?? throw new InvalidOperationException($"Round Id '{Model.RoundId}' not found.");

                    if (!Model.RealStart.HasValue || !Model.RealEnd.HasValue || round.RoundLegs.Count == 0)
                    {
                        return successResult;
                    }

                    if (round.RoundLegs.Any(rl =>
                            new DateTimePeriod(rl.StartDateTime, rl.EndDateTime).Contains(Model.RealStart))
                        &&
                        round.RoundLegs.Any(rl =>
                            new DateTimePeriod(rl.StartDateTime, rl.EndDateTime).Contains(Model.RealEnd)))
                    {
                        return successResult;
                    }

                    var displayPeriods = string.Empty;
                    const string joinWith = ", ";
                    displayPeriods = round.RoundLegs.Aggregate(displayPeriods,
                        (current, leg) =>
                            current +
                            $"{Data.TimeZoneConverter.ToZonedTime(leg.StartDateTime)?.DateTimeOffset.DateTime.ToShortDateString()} - {Data.TimeZoneConverter.ToZonedTime(leg.EndDateTime)?.DateTimeOffset.DateTime.ToShortDateString()}{joinWith}");
                    displayPeriods = displayPeriods[..^joinWith.Length];

                    return new FactResult
                    {
                        Message = string.Format(MatchResultValidatorResource.ResourceManager.GetString(
                                                    nameof(FactId.RealMatchDateWithinRoundLegs)) ??
                                                string.Empty, round.Description, displayPeriods),
                        Success = false
                    };
                }
            }
        );

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.SetsValidatorSuccessful,
                FieldNames = new[] { string.Empty },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = async (cancellationToken) =>
                {
                    if (!Model.IsOverruled)
                    {
                        await SetsValidator.CheckAsync(CancellationToken.None);
                    }
                        
                    return new FactResult
                    {
                        Message = MatchResultValidatorResource.ResourceManager.GetString(
                            nameof(FactId.SetsValidatorSuccessful)) ?? string.Empty,
                        Success = !SetsValidator.GetFailedFacts().Any()
                    };
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RealMatchDateEqualsFixture,
                FieldNames = new[] { nameof(Model.RealStart), nameof(Model.RealEnd) },
                Enabled = true,
                Type = FactType.Warning,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateEqualsFixture)) ?? string.Empty,
                        Success = Model.RealStart.HasValue && Model.RealEnd.HasValue
                                                           && Model.PlannedStart.HasValue
                                                           && Model.RealStart?.Date == Model.PlannedStart?.Date
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                // One ball point lasts for about 33 seconds (average over 1,024 matches with 169,845 ball points in 92,186 minutes)
                // https://volleyball.de/nc/news/details/datum/2011/05/27/zahlenspiele-1025-spiele-dauerten-64-tage-und-26-minuten/
                Id = FactId.RealMatchDurationIsPlausible,
                FieldNames = new[] { nameof(Model.RealStart), nameof(Model.RealEnd) },
                Enabled = true,
                Type = FactType.Warning,
                CheckAsync = (cancellationToken) =>
                {
                    const int minDuration = 33 - 10;
                    const int maxDuration = 33 + 10;
                    var duration = Model.RealStart.HasValue && Model.RealEnd.HasValue ? new DateTimePeriod(Model.RealStart, Model.RealEnd).Duration() : new TimeSpan(0);
                    var totalBallPoints = Model.Sets.GetTotalBallPoints();

                    return Task.FromResult(new FactResult
                    {
                        Message = string.Format(MatchResultValidatorResource.ResourceManager.GetString(
                                                    nameof(FactId.RealMatchDurationIsPlausible)) ??
                                                string.Empty, duration),
                        Success = Model.RealStart.HasValue && Model.RealEnd.HasValue
                                                           && (duration.TotalSeconds >= totalBallPoints * minDuration && duration.TotalSeconds <= totalBallPoints * maxDuration)
                    });
                }
            });
    }
}
