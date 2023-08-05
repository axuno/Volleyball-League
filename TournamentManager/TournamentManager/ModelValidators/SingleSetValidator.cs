using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class SingleSetValidator : AbstractValidator<SetEntity, (ITenantContext TenantContext,
    SetRuleEntity SetRule), SingleSetValidator.FactId>
{
    public enum FactId
    {
        BallPointsNotNegative,
        TieIsAllowed,

        NumOfPointsToWinReached,

        RegularWinReachedWithOnePointAhead,
        RegularWinReachedWithTwoPlusPointsAhead,

        TieBreakWinReachedWithOnePointAhead,
        TieBreakWinReachedWithTwoPlusPointsAhead
    }

    public SingleSetValidator(SetEntity model,
        (ITenantContext TenantContext, SetRuleEntity SetRule) data) : base(model, data)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        // facts for all

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.BallPointsNotNegative,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Critical,
                CheckAsync = (cancellationToken) => Task.FromResult(
                    new FactResult
                    {
                        Message = SingleSetValidatorResource.ResourceManager.GetString(
                            nameof(FactId.BallPointsNotNegative)) ?? string.Empty,
                        Success = Model.HomeBallPoints >= 0 && Model.GuestBallPoints >= 0
                    })
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.TieIsAllowed,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
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

                    return await Task.FromResult(factResult);
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.NumOfPointsToWinReached,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
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
            });

        // facts for regular sets

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RegularWinReachedWithOnePointAhead,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
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

                    return await Task.FromResult(factResult);
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.RegularWinReachedWithTwoPlusPointsAhead,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = (cancellationToken) =>
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
                                SingleSetValidatorResource.TextExactly,
                                Data.SetRule.PointsDiffToWinRegular,
                                SingleSetValidatorResource.TextMoreThan,
                                Data.SetRule.NumOfPointsToWinRegular);
                        }
                    }

                    return Task.FromResult(factResult);
                }
            });

        // facts for tie-break sets

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.TieBreakWinReachedWithOnePointAhead,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = async (cancellationToken) =>
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

                    return await Task.FromResult(factResult);
                }
            });

        Facts.Add(
            new Fact<FactId>
            {
                Id = FactId.TieBreakWinReachedWithTwoPlusPointsAhead,
                FieldNames = new[] {nameof(Model.HomeBallPoints), nameof(Model.GuestBallPoints)},
                Enabled = true,
                Type = FactType.Error,
                CheckAsync = (cancellationToken) =>
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
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
                            factResult.Message = string.Format(factResult.Message ?? string.Empty,
                                SingleSetValidatorResource.TextExactly,
                                Data.SetRule.PointsDiffToWinTiebreak,
                                SingleSetValidatorResource.TextMoreThan,
                                Data.SetRule.NumOfPointsToWinTiebreak);
                        }
                    }
                    return Task.FromResult(factResult);
                }
            });
    }
}