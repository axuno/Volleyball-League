using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class TeamInRoundValidator : AbstractValidator<TeamInRoundEntity, (ITenantContext TenantContext, long TouramentId), TeamInRoundValidator.FactId>
{
    public enum FactId
    {
        RoundBelongsToTournament
    }

    public TeamInRoundValidator(TeamInRoundEntity model, (ITenantContext TenantContext, long TournamentId) data) : base(model, data)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        Facts.Add(RoundBelongsToTournament());
    }

    private Fact<FactId> RoundBelongsToTournament()
    {
        return new()
        {
            Id = FactId.RoundBelongsToTournament,
            FieldNames = [nameof(Model.RoundId)],
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var roundWithTypeList = await Data.TenantContext.DbContext.AppDb.RoundRepository.GetRoundsWithTypeAsync(
                new(RoundFields.TournamentId == Data.TouramentId),
                cancellationToken).ConfigureAwait(false);

            if (roundWithTypeList.Exists(round => round.Id == Model.RoundId))
            {
                return new()
                {
                    Message = TeamInRoundValidatorResource.RoundBelongsToTournament,
                    Success = true
                };
            }
                        
            var tournament =
                await Data.TenantContext.DbContext.AppDb.TournamentRepository.GetTournamentAsync(new(TournamentFields.Id == Data.TouramentId),
                    cancellationToken).ConfigureAwait(false);

            return new()
            {
                Message = string.Format(
                    TeamInRoundValidatorResource.RoundBelongsToTournament ?? string.Empty,
                    tournament?.Description),
                Success = false
            };
        }
    }
}
