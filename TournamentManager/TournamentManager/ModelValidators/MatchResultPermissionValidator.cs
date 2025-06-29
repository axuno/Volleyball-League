using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

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
        Facts.Add(TournamentIsInActiveMode());
        Facts.Add(RoundIsStillRunning());
        Facts.Add(CurrentDateIsBeforeResultCorrectionDeadline());
    }

    private Fact<FactId> CurrentDateIsBeforeResultCorrectionDeadline()
    {
        return new Fact<FactId>
        {
            Id = FactId.CurrentDateIsBeforeResultCorrectionDeadline,
            FieldNames = [string.Empty],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => FactResult()
        };

        Task<FactResult> FactResult()
        {
            var factResult = new FactResult
            {
                Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                    nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)) ?? string.Empty,
                Success = true
            };

            if (!Model.RealStart.HasValue)
            {
                return Task.FromResult(factResult);
            }

            // negative values will allow unlimited corrections
            var maxCorrectionDate = Data.TenantContext.TournamentContext.MaxDaysForResultCorrection >= 0 ? Model.RealStart.Value.AddDays(Data.TenantContext.TournamentContext.MaxDaysForResultCorrection) : DateTime.MaxValue;

            if (maxCorrectionDate < Data.Criteria.CurrentDateUtc)
            {
                factResult.Success = false;
                factResult.Message = string.Format(MatchResultPermissionValidatorResource.ResourceManager.GetString(
                    nameof(FactId.CurrentDateIsBeforeResultCorrectionDeadline)) ?? string.Empty, Data.TenantContext.TournamentContext.MaxDaysForResultCorrection);
            }

            return Task.FromResult(factResult);
        }
    }

    private Fact<FactId> RoundIsStillRunning()
    {
        return new Fact<FactId>
        {
            Id = FactId.RoundIsStillRunning,
            FieldNames = [string.Empty],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                        nameof(FactId.RoundIsStillRunning)) ?? string.Empty,
                    Success = !Data.Criteria.RoundIsCompleted
                })
        };
    }

    private Fact<FactId> TournamentIsInActiveMode()
    {
        return new Fact<FactId>
        {
            Id = FactId.TournamentIsInActiveMode,
            FieldNames = [string.Empty],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = MatchResultPermissionValidatorResource.ResourceManager.GetString(
                        nameof(FactId.TournamentIsInActiveMode)) ?? string.Empty,
                    Success = !Data.Criteria.TournamentInPlanMode
                })
        };
    }
}
