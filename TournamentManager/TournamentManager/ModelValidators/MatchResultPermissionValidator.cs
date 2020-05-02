using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Data;

namespace TournamentManager.ModelValidators
{
    public class MatchResultPermissionValidator : AbstractValidator<MatchEntity, (OrganizationContext OrganizationContext,
        (bool TournamentInPlanMode, bool RoundIsCompleted, DateTime CurrentDateUtc) Criteria), MatchResultPermissionValidator.FactId>
    {
        public enum FactId
        {
            TournamentIsInActiveMode,
            RoundIsStillRunning,
            CurrentDateIsBeforeResultCorrectionDeadline
        }

        public MatchResultPermissionValidator(MatchEntity model,
            (OrganizationContext OrganizationContext, (bool TournamentInPlanMode, bool RoundIsCompleted, DateTime CurrentDateUtc) Criteria) data) : base(model, data)
        {
            CreateFacts();
        }

        private void CreateFacts()
        {
            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.TournamentIsInActiveMode,
                    FieldNames = new[] {string.Empty},
                    Enabled = true,
                    Type = FactType.Critical,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                                nameof(FactId.TournamentIsInActiveMode)),
                            Success = !Data.Criteria.TournamentInPlanMode
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.RoundIsStillRunning,
                    FieldNames = new[] { string.Empty },
                    Enabled = true,
                    Type = FactType.Critical,
                    CheckAsync = (cancellationToken) => Task.FromResult(
                        new FactResult
                        {
                            Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                                nameof(FactId.RoundIsStillRunning)),
                            Success = !Data.Criteria.RoundIsCompleted
                        })
                });

            Facts.Add(
                new Fact<FactId>
                {
                    Id = FactId.CurrentDateIsBeforeResultCorrectionDeadline,
                    FieldNames = new[] { string.Empty },
                    Enabled = true,
                    Type = FactType.Critical,
                    CheckAsync = async (cancellationToken) =>
                    {
                        var factResult = new FactResult
                        {
                            Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                                nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)),
                            Success = true
                        };

                        if (!Model.RealStart.HasValue)
                        {
                            return await Task.FromResult(factResult);
                        }

                        var maxCorrectionDate = Model.RealStart.Value.AddDays(Data.OrganizationContext.MaxDaysForResultCorrection);

                        if (maxCorrectionDate < Data.Criteria.CurrentDateUtc)
                        {
                            factResult.Success = false;
                            factResult.Message = string.Format(MatchResultPermissionValidatorResource.ResourceManager.GetString(
                                nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)) ?? string.Empty, Data.OrganizationContext.MaxDaysForResultCorrection);
                        }

                        return await Task.FromResult(factResult);
                    }
                });
        }
    }
}