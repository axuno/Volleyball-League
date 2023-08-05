using Microsoft.AspNetCore.Mvc.ModelBinding;

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
}
