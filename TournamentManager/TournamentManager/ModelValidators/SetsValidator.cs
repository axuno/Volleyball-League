using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.Match;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class SetsValidator : AbstractValidator<IList<SetEntity>, (ITenantContext TenantContext,
    (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules), SetsValidator.FactId>
{
    public enum FactId
    {
        AllSetsAreValid,
        MixAndMaxOfSetsPlayed,
        BestOfMixAndMaxOfSetsPlayed,
        BestOfRequiredTieBreakPlayed,
        BestOfNoMatchAfterBestOfReached
    }

    public SetsValidator(IList<SetEntity> model,
        (ITenantContext TenantContext, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules) data) : base(model, data)
    {
        CreateFacts();
    }

    public List<(long Id, int SequenceNo, SingleSetValidator.FactId FactId, string ErrorMessage)> SingleSetErrors { get; } = new();

    private void CreateFacts()
    {
        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.MixAndMaxOfSetsPlayed,
                FieldNames = new[] { nameof(MatchEntity.Sets) },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = string.Format(
                            SetsValidatorResource.ResourceManager.GetString(
                                nameof(FactId.MixAndMaxOfSetsPlayed)) ?? string.Empty, Data.Rules.MatchRule.NumOfSets),
                        Success = Data.Rules.MatchRule.BestOf || Model.Count == Data.Rules.MatchRule.NumOfSets
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.BestOfMixAndMaxOfSetsPlayed,
                FieldNames = new[] { nameof(MatchEntity.Sets) },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = string.Format(
                            SetsValidatorResource.ResourceManager.GetString(
                                nameof(FactId.BestOfMixAndMaxOfSetsPlayed)) ?? string.Empty, Data.Rules.MatchRule.NumOfSets, Data.Rules.MatchRule.MaxNumOfSets()),
                        Success = !Data.Rules.MatchRule.BestOf || Model.Count >= Data.Rules.MatchRule.NumOfSets && Model.Count <= Data.Rules.MatchRule.MaxNumOfSets()
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.AllSetsAreValid,
                FieldNames = new[] { nameof(MatchEntity.Sets) },
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = async (cancellationToken) =>
                {
                    foreach (var set in Model)
                    {
                        var singleSetValidator =
                            new SingleSetValidator(set, (Data.TenantContext, Data.Rules.SetRule));
                        await singleSetValidator.CheckAsync(cancellationToken);
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
                        Success = !SingleSetErrors.Any()
                    };
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.BestOfRequiredTieBreakPlayed,
                FieldNames = new[] { nameof(MatchEntity.Sets) },
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
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
                        factResult.Success = Model.Last().IsTieBreak;
                    }

                    return await Task.FromResult(factResult);
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.BestOfNoMatchAfterBestOfReached,
                FieldNames = new[] { nameof(MatchEntity.Sets) },
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
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
                    for (var i=0; i < Model.Count; i++)
                    {
                        if (Model[i].HomeBallPoints < Model[i].GuestBallPoints)
                        {
                            numOfWins.Guest++;
                        } else if (Model[i].HomeBallPoints > Model[i].GuestBallPoints)
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

                    return await Task.FromResult(factResult);
                }
            });
    }
}