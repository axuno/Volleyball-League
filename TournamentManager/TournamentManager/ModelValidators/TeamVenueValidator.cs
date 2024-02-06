using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace TournamentManager.ModelValidators;

public class TeamVenueValidator : AbstractValidator<TeamEntity, ITenantContext, TeamVenueValidator.FactId>
{
    public enum FactId
    {
        VenueIsSetIfRequired,
        VenueIsValid
    }

    public TeamVenueValidator(TeamEntity model, ITenantContext tenantContext) : base(model, tenantContext)
    {
        CreateFacts();
    }

    private void CreateFacts()
    {
        Facts.Add(VenueIsSetIfRequired());
        Facts.Add(VenueIsValid());
    }

    private Fact<FactId> VenueIsValid()
    {
        return new Fact<FactId>
        {
            Id = FactId.VenueIsValid,
            FieldNames = new[] {nameof(Model.VenueId)},
            Enabled = Model.VenueId.HasValue,
            Type = FactType.Critical,
            CheckAsync = async (cancellationToken) => new FactResult {
                Message = TeamVenueValidatorResource.ResourceManager.GetString(
                    nameof(FactId.VenueIsValid)) ?? string.Empty,
                Success = Model.VenueId.HasValue &&
                          await Data.DbContext.AppDb.VenueRepository.IsValidVenueIdAsync(Model.VenueId,
                              cancellationToken)
            }
        };
    }

    private Fact<FactId> VenueIsSetIfRequired()
    {
        return new Fact<FactId>
        {
            Id = FactId.VenueIsSetIfRequired,
            FieldNames = new[] {nameof(Model.VenueId)},
            Enabled = true,
            Type = FactType.Critical,
            CheckAsync = (cancellationToken) => Task.FromResult(
                new FactResult
                {
                    Message = TeamVenueValidatorResource.ResourceManager.GetString(
                        nameof(FactId.VenueIsSetIfRequired)) ?? string.Empty,
                    // Venue is set, if required or venue has any value if not required
                    Success = (Model.VenueId.HasValue && Data.TournamentContext.TeamRuleSet.HomeVenue.MustBeSet)
                              || (!Data.TournamentContext.TeamRuleSet.HomeVenue.MustBeSet)
                })
        };
    }
}
