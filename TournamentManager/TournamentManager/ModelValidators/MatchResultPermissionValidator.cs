using System;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators
{
    public class MatchResultPermissionValidator : AbstractValidator<MatchEntity, (ITenantContext TenantContext,
        (bool TournamentInPlanMode, bool RoundIsCompleted, DateTime CurrentDateUtc) Criteria), MatchResultPermissionValidator.FactId>
    {
        public enum FactId
        {
            TournamentIsInActiveMode,
            RoundIsStillRunning,
            CurrentDateIsBeforeResultCorrectionDeadline
        }

        public MatchResultPermissionValidator(MatchEntity model,
            (ITenantContext TenantContext, (bool TournamentInPlanMode, bool RoundIsCompleted, DateTime CurrentDateUtc) Criteria) data) : base(model, data)
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
                                nameof(FactId.TournamentIsInActiveMode)) ?? string.Empty,
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
                                nameof(FactId.RoundIsStillRunning)) ?? string.Empty,
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
                                nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)) ?? string.Empty,
                            Success = true
                        };

                        if (!Model.RealStart.HasValue)
                        {
                            return await Task.FromResult(factResult);
                        }

                        // negative values will allow unlimited corrections
                        var maxCorrectionDate = Data.TenantContext.TournamentContext.MaxDaysForResultCorrection >= 0 ? Model.RealStart.Value.AddDays(Data.TenantContext.TournamentContext.MaxDaysForResultCorrection) : DateTime.MaxValue;

                        if (maxCorrectionDate < Data.Criteria.CurrentDateUtc)
                        {
                            factResult.Success = false;
                            factResult.Message = string.Format(MatchResultPermissionValidatorResource.ResourceManager.GetString(
                                nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)) ?? string.Empty, Data.TenantContext.TournamentContext.MaxDaysForResultCorrection);
                        }

                        return await Task.FromResult(factResult);
                    }
                });
        }
    }
}