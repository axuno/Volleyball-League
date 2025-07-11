﻿using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public sealed class SingleSetValidator : AbstractValidator<SetEntity, (ITenantContext TenantContext,
    SetRuleEntity SetRule), SingleSetValidator.FactId>
{
    public enum FactId
    {
        BallPointsNotNegative,
        SetPointsAreValid,
        TieIsAllowed,

        NumOfPointsToWinReached,

        RegularWinReachedWithOnePointAhead,
        RegularWinReachedWithTwoPlusPointsAhead,

        TieBreakWinReachedWithOnePointAhead,
        TieBreakWinReachedWithTwoPlusPointsAhead
    }

    private static readonly Dictionary<MatchValidationMode, Dictionary<FactId, bool>> ModeConfiguration = new() {
        {
            MatchValidationMode.Default, new Dictionary<FactId, bool> {
                { FactId.BallPointsNotNegative, true },
                { FactId.SetPointsAreValid, false },
                { FactId.TieIsAllowed, true },
                { FactId.NumOfPointsToWinReached, true },
                { FactId.RegularWinReachedWithOnePointAhead, true },
                { FactId.RegularWinReachedWithTwoPlusPointsAhead, true },
                { FactId.TieBreakWinReachedWithOnePointAhead, true },
                { FactId.TieBreakWinReachedWithTwoPlusPointsAhead, true }
            }
        },
        {
            MatchValidationMode.Overrule, new Dictionary<FactId, bool> {
                { FactId.BallPointsNotNegative, true },
                { FactId.SetPointsAreValid, true },
                { FactId.TieIsAllowed, false },
                { FactId.NumOfPointsToWinReached, false },
                { FactId.RegularWinReachedWithOnePointAhead, false },
                { FactId.RegularWinReachedWithTwoPlusPointsAhead, false },
                { FactId.TieBreakWinReachedWithOnePointAhead, false },
                { FactId.TieBreakWinReachedWithTwoPlusPointsAhead, false }
            }
        }
    };

    public SingleSetValidator(SetEntity model,
        (ITenantContext TenantContext, SetRuleEntity SetRule) data, MatchValidationMode validationMode) : base(model, data)
    {
        CreateFacts();
        ConfigureFacts(ModeConfiguration[validationMode]);
    }

    private void CreateFacts()
    {
        // facts for all

        Facts.Add(BallPointsNotNegative());
        Facts.Add(SetPointsAreValid());
        Facts.Add(TieIsAllowed());
        Facts.Add(NumOfPointsToWinReached());
        
        // facts for regular sets

        Facts.Add(RegularWinReachedWithOnePointAhead());
        Facts.Add(RegularWinReachedWithTwoPlusPointsAhead());

        // facts for tie-break sets

        Facts.Add(TieBreakWinReachedWithOnePointAhead());
        Facts.Add(TieBreakWinReachedWithTwoPlusPointsAhead());
    }

    private Fact<FactId> TieBreakWinReachedWithTwoPlusPointsAhead()
    {
        return new Fact<FactId>
        {
            Id = FactId.TieBreakWinReachedWithTwoPlusPointsAhead,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = SingleSetValidatorResource.ResourceManager.GetString(
                    nameof(FactId.TieBreakWinReachedWithTwoPlusPointsAhead)) ?? string.Empty,
                Success = true
            };

            if (Model.IsTieBreak && Data.SetRule.PointsDiffToWinTiebreak > 1)
            {
                // Home team reached required points to win
                if (Model.HomeBallPoints == Data.SetRule.NumOfPointsToWinTiebreak &&
                    Model.HomeBallPoints > Model.GuestBallPoints)
                {
                    factResult.Success = Model.HomeBallPoints - Model.GuestBallPoints >=
                                         Data.SetRule.PointsDiffToWinTiebreak;
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextAtLeast,
                        Data.SetRule.PointsDiffToWinTiebreak,
                        string.Empty,
                        Data.SetRule.NumOfPointsToWinTiebreak);
                    return Task.FromResult(factResult);
                }

                // Guest team reached required points to win
                if (Model.GuestBallPoints == Data.SetRule.NumOfPointsToWinTiebreak &&
                    Model.GuestBallPoints > Model.HomeBallPoints)
                {
                    factResult.Success = Model.GuestBallPoints - Model.HomeBallPoints >=
                                         Data.SetRule.PointsDiffToWinTiebreak;
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextAtLeast,
                        Data.SetRule.PointsDiffToWinTiebreak,
                        string.Empty,
                        Data.SetRule.NumOfPointsToWinTiebreak);

                    return Task.FromResult(factResult);
                }

                // Minimum required ball points are exceeded
                if (Model.HomeBallPoints > Data.SetRule.NumOfPointsToWinTiebreak ||
                    Model.GuestBallPoints > Data.SetRule.NumOfPointsToWinTiebreak)
                {
                    factResult.Success = Math.Abs(Model.GuestBallPoints - Model.HomeBallPoints) ==
                                         Data.SetRule.PointsDiffToWinTiebreak;
                    // The difference in ball points must be {0} {1} if a team has {2} {3} ballpoints
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextExactly,
                        Data.SetRule.PointsDiffToWinTiebreak,
                        SingleSetValidatorResource.TextMoreThan,
                        Data.SetRule.NumOfPointsToWinTiebreak);
                }
            }
            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> TieBreakWinReachedWithOnePointAhead()
    {
        return new Fact<FactId>
        {
            Id = FactId.TieBreakWinReachedWithOnePointAhead,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = string.Format(
                    SingleSetValidatorResource.ResourceManager.GetString(
                        nameof(FactId.TieBreakWinReachedWithOnePointAhead)) ?? string.Empty,
                    Data.SetRule.NumOfPointsToWinTiebreak, Data.SetRule.PointsDiffToWinTiebreak),
                Success = true
            };

            // separate rule for regular set
            if (Model.IsTieBreak && Data.SetRule.PointsDiffToWinTiebreak == 1)
            {
                factResult.Success =
                    // One team exactly reached required points to win,
                    // and the other must have less the needed difference
                    (Model.HomeBallPoints == Data.SetRule.NumOfPointsToWinTiebreak
                     && Model.GuestBallPoints < Model.HomeBallPoints)
                    ||
                    (Model.GuestBallPoints == Data.SetRule.NumOfPointsToWinTiebreak
                     && Model.HomeBallPoints < Model.GuestBallPoints);
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> RegularWinReachedWithTwoPlusPointsAhead()
    {
        return new Fact<FactId>
        {
            Id = FactId.RegularWinReachedWithTwoPlusPointsAhead,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = SingleSetValidatorResource.ResourceManager.GetString(
                    nameof(FactId.RegularWinReachedWithTwoPlusPointsAhead)) ?? string.Empty,
                Success = true
            };

            // separate rule for tie-break
            if (!Model.IsTieBreak && Data.SetRule.PointsDiffToWinRegular > 1)
            {
                // Home team reached required points to win
                if (Model.HomeBallPoints == Data.SetRule.NumOfPointsToWinRegular &&
                    Model.HomeBallPoints > Model.GuestBallPoints)
                {
                    factResult.Success = Model.HomeBallPoints - Model.GuestBallPoints >=
                                         Data.SetRule.PointsDiffToWinRegular;
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextAtLeast,
                        Data.SetRule.PointsDiffToWinRegular,
                        string.Empty,
                        Data.SetRule.NumOfPointsToWinRegular);
                    return Task.FromResult(factResult);
                }

                // Guest team reached required points to win
                if (Model.GuestBallPoints == Data.SetRule.NumOfPointsToWinRegular &&
                    Model.GuestBallPoints > Model.HomeBallPoints)
                {
                    factResult.Success = Model.GuestBallPoints - Model.HomeBallPoints >=
                                         Data.SetRule.PointsDiffToWinRegular;
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextAtLeast,
                        Data.SetRule.PointsDiffToWinRegular,
                        string.Empty,
                        Data.SetRule.NumOfPointsToWinRegular);

                    return Task.FromResult(factResult);
                }

                // Minimum required ball points are exceeded
                if (Model.HomeBallPoints > Data.SetRule.NumOfPointsToWinRegular ||
                    Model.GuestBallPoints > Data.SetRule.NumOfPointsToWinRegular)
                {
                    factResult.Success = Math.Abs(Model.GuestBallPoints - Model.HomeBallPoints) ==
                                         Data.SetRule.PointsDiffToWinRegular;
                    factResult.Message = string.Format(factResult.Message,
                        SingleSetValidatorResource.TextExactly,
                        Data.SetRule.PointsDiffToWinRegular,
                        SingleSetValidatorResource.TextMoreThan,
                        Data.SetRule.NumOfPointsToWinRegular);
                }
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> RegularWinReachedWithOnePointAhead()
    {
        return new Fact<FactId>
        {
            Id = FactId.RegularWinReachedWithOnePointAhead,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = string.Format(
                    SingleSetValidatorResource.ResourceManager.GetString(
                        nameof(FactId.RegularWinReachedWithOnePointAhead)) ?? string.Empty,
                    Data.SetRule.NumOfPointsToWinRegular, Data.SetRule.PointsDiffToWinRegular),
                Success = true
            };

            // separate rule for tie-break
            if (!Model.IsTieBreak && Data.SetRule.PointsDiffToWinRegular == 1)
            {
                factResult.Success =
                    // One team exactly reached required points to win,
                    // and the other must have less the needed difference
                    (Model.HomeBallPoints == Data.SetRule.NumOfPointsToWinRegular
                     && Model.GuestBallPoints < Model.HomeBallPoints)
                    ||
                    (Model.GuestBallPoints == Data.SetRule.NumOfPointsToWinRegular
                     && Model.HomeBallPoints < Model.GuestBallPoints);
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> NumOfPointsToWinReached()
    {
        return new Fact<FactId>
        {
            Id = FactId.NumOfPointsToWinReached,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => Task.FromResult(
                Model.IsTieBreak
                    ? new FactResult
                    {
                        Message = string.Format(
                            SingleSetValidatorResource.ResourceManager.GetString(
                                nameof(FactId.NumOfPointsToWinReached)) ?? string.Empty,
                            Data.SetRule.NumOfPointsToWinTiebreak),
                        Success = Model.HomeBallPoints >= Data.SetRule.NumOfPointsToWinTiebreak ||
                                  Model.GuestBallPoints >= Data.SetRule.NumOfPointsToWinTiebreak
                    }
                    : new FactResult
                    {
                        Message = string.Format(
                            SingleSetValidatorResource.ResourceManager.GetString(
                                nameof(FactId.NumOfPointsToWinReached)) ?? string.Empty,
                            Data.SetRule.NumOfPointsToWinRegular),
                        Success = Model.HomeBallPoints >= Data.SetRule.NumOfPointsToWinRegular ||
                                  Model.GuestBallPoints >= Data.SetRule.NumOfPointsToWinRegular
                    })
        };
    }

    private Fact<FactId> TieIsAllowed()
    {
        return new Fact<FactId>
        {
            Id = FactId.TieIsAllowed,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Error,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message =
                    SingleSetValidatorResource.ResourceManager.GetString(
                        nameof(FactId.TieIsAllowed)) ?? string.Empty,
                Success = true
            };

            if (Model.HomeBallPoints == Model.GuestBallPoints)
            {
                if (Model.IsTieBreak)
                {
                    factResult.Success = Data.SetRule.PointsDiffToWinTiebreak == 0
                                         && (Model.HomeBallPoints > 0 || Model.GuestBallPoints > 0);
                }
                else
                {
                    factResult.Success = Data.SetRule.PointsDiffToWinRegular == 0
                                         && (Model.HomeBallPoints > 0 || Model.GuestBallPoints > 0);
                }
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> BallPointsNotNegative()
    {
        return new Fact<FactId>
        {
            Id = FactId.BallPointsNotNegative,
            FieldNames = [nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)],
            Enabled = false,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = SingleSetValidatorResource.ResourceManager.GetString(
                        nameof(FactId.BallPointsNotNegative)) ?? string.Empty,
                    Success = Model is { HomeBallPoints: >= 0, GuestBallPoints: >= 0 }
                })
        };
    }

    private Fact<FactId> SetPointsAreValid()
    {
        return new Fact<FactId>
        {
            Id = FactId.SetPointsAreValid,
            FieldNames = [nameof(Model.HomeSetPoints), nameof(Model.GuestSetPoints)],
            Enabled = false,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) =>
            {
                var allowed = new List<int?>
                {
                    Data.SetRule.PointsSetLost,
                    Data.SetRule.PointsSetWon,
                    Data.SetRule.PointsSetTie

                }.Distinct().ToList();

                return Task.FromResult(
                    new FactResult
                    {
                        Message = string.Format(SingleSetValidatorResource.ResourceManager.GetString(
                                                    nameof(FactId.SetPointsAreValid)) ??
                                                string.Empty, string.Join(',', allowed)),
                        Success = allowed.Contains(Model.HomeSetPoints) && allowed.Contains(Model.GuestSetPoints)
                    });
            }
        };
    }
}
