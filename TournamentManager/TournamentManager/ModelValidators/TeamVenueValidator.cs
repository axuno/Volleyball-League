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
        return new()
        {
            Id = FactId.VenueIsValid,
            FieldNames = [nameof(Model.VenueId)],
            Enabled = Model.VenueId.HasValue,
            Type = FactType.Critical,
            CheckAsync = async (cancellationToken) => new()
            {
                Message = TeamVenueValidatorResource.ResourceManager.GetString(
                    nameof(FactId.VenueIsValid)) ?? string.Empty,
                Success = Model.VenueId.HasValue &&
                          await Data.DbContext.AppDb.VenueRepository.IsValidVenueIdAsync(Model.VenueId,
                              cancellationToken).ConfigureAwait(false)
            }
        };
    }

    private Fact<FactId> VenueIsSetIfRequired()
    {
        return new()
        {
            Id = FactId.VenueIsSetIfRequired,
            FieldNames = [nameof(Model.VenueId)],
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
