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
        return new Fact<FactId>
        {
            Id = FactId.RoundBelongsToTournament,
            FieldNames = new[] {nameof(Model.RoundId)},
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = FactResult
        };

        async Task<FactResult> FactResult(CancellationToken cancellationToken)
        {
            var roundWithTypeList = await Data.TenantContext.DbContext.AppDb.RoundRepository.GetRoundsWithTypeAsync(
                new PredicateExpression(RoundFields.TournamentId == Data.TouramentId),
                cancellationToken);

            if (roundWithTypeList.Any(round => round.Id == Model.RoundId))
            {
                return new FactResult
                {
                    Message = TeamInRoundValidatorResource.RoundBelongsToTournament,
                    Success = true
                };
            }
                        
            var tournament =
                await Data.TenantContext.DbContext.AppDb.TournamentRepository.GetTournamentAsync(new PredicateExpression(TournamentFields.Id == Data.TouramentId),
                    cancellationToken);

            return new FactResult
            {
                Message = string.Format(
                    TeamInRoundValidatorResource.RoundBelongsToTournament ?? string.Empty,
                    tournament?.Description),
                Success = false
            };
        }
    }
}
