using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public sealed class SetsValidator : AbstractValidator<IList<SetEntity>, (ITenantContext TenantContext,
    (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules), SetsValidator.FactId>
{
    public enum FactId
    {
        AllSetsAreValid,
        MinAndMaxOfSetsPlayed,
        BestOfMinAndMaxOfSetsPlayed,
        BestOfRequiredTieBreakPlayed,
        BestOfNoMatchAfterBestOfReached
    }

    private readonly MatchValidationMode _validationMode;

    private static readonly Dictionary<MatchValidationMode, Dictionary<FactId, bool>> ModeConfiguration = new() {
        {
            MatchValidationMode.Default, new Dictionary<FactId, bool> {
                { FactId.AllSetsAreValid, true },
                { FactId.MinAndMaxOfSetsPlayed, true },
                { FactId.BestOfMinAndMaxOfSetsPlayed, true },
                { FactId.BestOfRequiredTieBreakPlayed, true },
                { FactId.BestOfNoMatchAfterBestOfReached, true }
            }
        },
        {
            MatchValidationMode.Overrule, new Dictionary<FactId, bool> {
                { FactId.AllSetsAreValid, true },
                { FactId.MinAndMaxOfSetsPlayed, false },
                { FactId.BestOfMinAndMaxOfSetsPlayed, false },
                { FactId.BestOfRequiredTieBreakPlayed, false },
                { FactId.BestOfNoMatchAfterBestOfReached, false }
            }
        }
    };

    public SetsValidator(IList<SetEntity> model,
        (ITenantContext TenantContext, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules) data, MatchValidationMode validationMode) : base(model, data)
    {
        _validationMode = validationMode;
        CreateFacts();
        ConfigureFacts(ModeConfiguration[validationMode]);
    }

    public List<(long Id, int SequenceNo, SingleSetValidator.FactId FactId, string ErrorMessage)> SingleSetErrors { get; } =
        [];

    private void CreateFacts()
    {
        Facts.Add(AllSetsAreValid());
        Facts.Add(MinAndMaxOfSetsPlayed());
        Facts.Add(BestOfMinAndMaxOfSetsPlayed());
        Facts.Add(BestOfRequiredTieBreakPlayed());
        Facts.Add(BestOfNoMatchAfterBestOfReached());
    }

    private Fact<FactId> BestOfNoMatchAfterBestOfReached()
    {
        return new Fact<FactId>
        {
            Id = FactId.BestOfNoMatchAfterBestOfReached,
            FieldNames = [nameof(MatchEntity.Sets)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = async (cancellationToken) => await FactResult().ConfigureAwait(false)
        };

        async Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = string.Format(SetsValidatorResource.ResourceManager.GetString(
                        nameof(FactId.BestOfNoMatchAfterBestOfReached)) ?? string.Empty,
                    Data.Rules.MatchRule.NumOfSets),
                Success = true
            };

            var numOfWins = new PointResult(0, 0);
            var bestOfReachedButSetsFollow = false;
            for (var i = 0; i < Model.Count; i++)
            {
                if (Model[i].HomeBallPoints < Model[i].GuestBallPoints)
                {
                    numOfWins.Guest++;
                }
                else if (Model[i].HomeBallPoints > Model[i].GuestBallPoints)
                {
                    numOfWins.Home++;
                }

                if ((numOfWins.Home == Data.Rules.MatchRule.NumOfSets ||
                     numOfWins.Guest == Data.Rules.MatchRule.NumOfSets) && i + 1 < Model.Count)
                {
                    bestOfReachedButSetsFollow = true;
                    break;
                }
            }

            if (Data.Rules.MatchRule.BestOf)
            {
                factResult.Success = !bestOfReachedButSetsFollow;
            }

            return await Task.FromResult(factResult).ConfigureAwait(false);
        }
    }

    private Fact<FactId> BestOfRequiredTieBreakPlayed()
    {
        return new Fact<FactId>
        {
            Id = FactId.BestOfRequiredTieBreakPlayed,
            FieldNames = [nameof(MatchEntity.Sets)],
            Enabled = true,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = string.Format(SetsValidatorResource.ResourceManager.GetString(
                        nameof(FactId.BestOfRequiredTieBreakPlayed)) ?? string.Empty,
                    Data.Rules.MatchRule.NumOfSets, Data.Rules.MatchRule.MaxNumOfSets()),
                Success = true
            };

            if (Data.Rules.MatchRule.BestOf && Data.Rules.MatchRule.MaxNumOfSets() == Model.Count)
            {
                factResult.Success = Model[^1].IsTieBreak;
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> AllSetsAreValid()
    {
        return new Fact<FactId>
        {
            Id = FactId.AllSetsAreValid,
            FieldNames = [nameof(MatchEntity.Sets)],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            foreach (var set in Model)
            {
                var singleSetValidator =
                    new SingleSetValidator(set, (Data.TenantContext, Data.Rules.SetRule), _validationMode);
                await singleSetValidator.CheckAsync(cancellationToken).ConfigureAwait(false);
                var errorFact = singleSetValidator.GetFailedFacts().FirstOrDefault();
                if (errorFact != null)
                {
                    SingleSetErrors.Add((set.Id, set.SequenceNo, errorFact.Id, errorFact.Message));
                }
            }

            return new FactResult
            {
                Message = SetsValidatorResource.ResourceManager.GetString(
                    nameof(FactId.AllSetsAreValid)) ?? string.Empty,
                Success = SingleSetErrors.Count == 0
            };
        }
    }

    private Fact<FactId> BestOfMinAndMaxOfSetsPlayed()
    {
        return new Fact<FactId>
        {
            Id = FactId.BestOfMinAndMaxOfSetsPlayed,
            FieldNames = [nameof(MatchEntity.Sets)],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = string.Format(
                        SetsValidatorResource.ResourceManager.GetString(
                            nameof(FactId.BestOfMinAndMaxOfSetsPlayed)) ?? string.Empty, Data.Rules.MatchRule.NumOfSets, Data.Rules.MatchRule.MaxNumOfSets()),
                    Success = !Data.Rules.MatchRule.BestOf || Model.Count >= Data.Rules.MatchRule.NumOfSets && Model.Count <= Data.Rules.MatchRule.MaxNumOfSets()
                })
        };
    }

    private Fact<FactId> MinAndMaxOfSetsPlayed()
    {
        return new Fact<FactId>
        {
            Id = FactId.MinAndMaxOfSetsPlayed,
            FieldNames = [nameof(MatchEntity.Sets)],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = string.Format(
                        SetsValidatorResource.ResourceManager.GetString(
                            nameof(FactId.MinAndMaxOfSetsPlayed)) ?? string.Empty, Data.Rules.MatchRule.NumOfSets),
                    Success = Data.Rules.MatchRule.BestOf || Model.Count == Data.Rules.MatchRule.NumOfSets
                })
        };
    }
}
