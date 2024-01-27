using Microsoft.AspNetCore.Mvc.ModelBinding;
using TournamentManager.ModelValidators;

namespace League.Models.TeamViewModels;

public class TeamVenueSelectModel
{
    [BindNever]
    public long TournamentId { get; set; }
    [HiddenInput]
    public long TeamId { get; set; }
    [HiddenInput]
    public long? VenueId { get; set; }
    [HiddenInput] 
    public string ReturnUrl { get; set; } = string.Empty;

    public static async Task<bool> ValidateAsync(TeamVenueValidator teamValidator, ModelStateDictionary modelState, CancellationToken cancellationToken)
    {
        var fact = await teamValidator.CheckAsync(TeamVenueValidator.FactId.VenueIsSetIfRequired, cancellationToken);
        if (fact is { IsChecked: true, Success: false }) modelState.AddModelError(fact.FieldNames.First(), fact.Message);

        fact = await teamValidator.CheckAsync(TeamVenueValidator.FactId.VenueIsValid, cancellationToken);
        if (fact is { IsChecked: true, Success: false }) modelState.AddModelError(fact.FieldNames.First(), fact.Message);

        return modelState.IsValid;
    }
}
