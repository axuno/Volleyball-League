using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public sealed class MatchResultValidator : AbstractValidator<MatchEntity, (ITenantContext TenantContext,
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
        MatchPointsAreValid,
        SetsValidatorSuccessful
    }

    private static readonly Dictionary<MatchValidationMode, Dictionary<FactId, bool>> ModeConfiguration = new() {
        {
            MatchValidationMode.Default, new Dictionary<FactId, bool> {
                { FactId.RealMatchDateIsSet, true},
                { FactId.RealMatchDateTodayOrBefore, true},
                { FactId.RealMatchDateWithinRoundLegs, true},
                { FactId.RealMatchDateEqualsFixture, true},
                { FactId.RealMatchDurationIsPlausible, true},
                { FactId.MatchPointsAreValid, false},
                { FactId.SetsValidatorSuccessful, true}
            }
        },
        {
            MatchValidationMode.Overrule, new Dictionary<FactId, bool> {
                { FactId.RealMatchDateIsSet, true},
                { FactId.RealMatchDateTodayOrBefore, true},
                { FactId.RealMatchDateWithinRoundLegs, false},
                { FactId.RealMatchDateEqualsFixture, false},
                { FactId.RealMatchDurationIsPlausible, true},
                { FactId.MatchPointsAreValid, true},
                { FactId.SetsValidatorSuccessful, true}
            }
        }
    };

    public MatchResultValidator(MatchEntity model,
        (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules) data, MatchValidationMode validationMode) : base(model, data)
    {
        SetsValidator = new SetsValidator(Model.Sets, (Data.TenantContext, Data.Rules), validationMode);
        CreateFacts();
        ConfigureFacts(ModeConfiguration[validationMode]);
    }

    public SetsValidator SetsValidator { get; }

    private void CreateFacts()
    {
        Facts.Add(RealMatchDateIsSet());
        Facts.Add(RealMatchDateTodayOrBefore());
        Facts.Add(RealMatchDateWithinRoundLegs());
        Facts.Add(RealMatchDateEqualsFixture());
        Facts.Add(RealMatchDurationIsPlausible());
        Facts.Add(MatchPointsAreValid());
        Facts.Add(SetsValidatorSuccessful());
    }

    private Fact<FactId> RealMatchDurationIsPlausible()
    {
        return new Fact<FactId>
        {
            // One ball point lasts for about 33 seconds (average over 1,024 matches with 169,845 ball points in 92,186 minutes)
            // https://volleyball.de/nc/news/details/datum/2011/05/27/zahlenspiele-1025-spiele-dauerten-64-tage-und-26-minuten/
            Id = FactId.RealMatchDurationIsPlausible,
            FieldNames = [nameof(Model.RealStart), nameof(Model.RealEnd)],
            Enabled = false,
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            const int minDuration = 33 - 10;
            const int maxDuration = 33 + 10;
            var duration = Model is { RealStart: not null, RealEnd: not null } ? new DateTimePeriod(Model.RealStart, Model.RealEnd).Duration() : new TimeSpan(0);
            var totalBallPoints = Model.Sets.GetTotalBallPoints();

            return Task.FromResult(new FactResult
            {
                Message = string.Format(MatchResultValidatorResource.ResourceManager.GetString(
                                            nameof(FactId.RealMatchDurationIsPlausible)) ??
                                        string.Empty, duration),
                Success = Model is { RealStart: not null, RealEnd: not null }
                          && (duration.TotalSeconds >= totalBallPoints * minDuration && duration.TotalSeconds <= totalBallPoints * maxDuration)
            });
        }
    }

    private Fact<FactId> RealMatchDateEqualsFixture()
    {
        return new Fact<FactId>
        {
            Id = FactId.RealMatchDateEqualsFixture,
            FieldNames = [nameof(Model.RealStart), nameof(Model.RealEnd)],
            Enabled = false,
            Type = FactType.Warning,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateEqualsFixture)) ?? string.Empty,
                    Success = Model.RealStart.HasValue && Model is { RealEnd: not null, PlannedStart: not null }
                                                       && Model.RealStart?.Date == Model.PlannedStart?.Date
                })
        };
    }

    private Fact<FactId> SetsValidatorSuccessful()
    {
        return new Fact<FactId>
        {
            Id = FactId.SetsValidatorSuccessful,
            FieldNames = [string.Empty],
            Enabled = false,
            Type = FactType.Critical,
            CheckAsync = async (cancellationToken) => await FactResult().ConfigureAwait(false)
        };

        async Task<FactResult> FactResult()
        {
            await SetsValidator.CheckAsync(CancellationToken.None).ConfigureAwait(false);

            return new FactResult
            {
                Message = MatchResultValidatorResource.ResourceManager.GetString(
                    nameof(FactId.SetsValidatorSuccessful)) ?? string.Empty,
                Success = SetsValidator.GetFailedFacts().Count == 0
            };
        }
    }

    private Fact<FactId> RealMatchDateWithinRoundLegs()
    {
        return new Fact<FactId>
        {
            Id = FactId.RealMatchDateWithinRoundLegs,
            FieldNames = [nameof(Model.RealStart), nameof(Model.RealEnd)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var successResult = new FactResult
            {
                Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateWithinRoundLegs)) ?? string.Empty,
                Success = true
            };

            var round =
                await Data.TenantContext.DbContext.AppDb.RoundRepository
                    .GetRoundWithLegsAsync(Model.RoundId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Round Id '{Model.RoundId}' not found.");

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

    private Fact<FactId> RealMatchDateTodayOrBefore()
    {
        return new Fact<FactId>
        {
            Id = FactId.RealMatchDateTodayOrBefore,
            FieldNames = [nameof(Model.RealStart), nameof(Model.RealEnd)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateTodayOrBefore)) ?? string.Empty,
                    Success = Model.RealStart?.Date <= Today.Date && Model.RealEnd?.Date <= Today.Date
                })
        };
    }

    private Fact<FactId> RealMatchDateIsSet()
    {
        return new Fact<FactId>
        {
            Id = FactId.RealMatchDateIsSet,
            FieldNames = [nameof(Model.RealStart), nameof(Model.RealEnd)],
            Enabled = false,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = MatchResultValidatorResource.ResourceManager.GetString(nameof(FactId.RealMatchDateIsSet)) ?? string.Empty,
                    Success = Model is { RealStart: not null, RealEnd: not null }
                })
        };
    }

    private Fact<FactId> MatchPointsAreValid()
    {
        return new Fact<FactId>
        {
            Id = FactId.MatchPointsAreValid,
            FieldNames = [nameof(Model.HomePoints), nameof(Model.GuestPoints)],
            Enabled = false,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) =>
            {
                var allowed = new List<int?>
                {
                    Data.Rules.MatchRule.PointsMatchLost,
                    Data.Rules.MatchRule.PointsMatchTie,
                    Data.Rules.MatchRule.PointsMatchWon,
                    Data.Rules.MatchRule.PointsMatchLostAfterTieBreak,
                    Data.Rules.MatchRule.PointsMatchWonAfterTieBreak
                }.Distinct().ToList();

                return Task.FromResult(
                    new FactResult
                    {
                        Message = string.Format(MatchResultValidatorResource.ResourceManager.GetString(
                                                    nameof(FactId.MatchPointsAreValid)) ??
                                                string.Empty, string.Join(',', allowed)),
                        Success = allowed.Contains(Model.HomePoints) && allowed.Contains(Model.GuestPoints)
                    });
            }
        };
    }
}
