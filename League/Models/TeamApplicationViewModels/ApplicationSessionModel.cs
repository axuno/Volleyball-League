﻿using League.Components;

namespace League.Models.TeamApplicationViewModels;

public class ApplicationSessionModel
{
    public enum TeamVenueSetKind
    {
        NotSet = 0,
        NoVenue,
        ExistingVenue,
        NewVenue
    }

    /// <summary>
    /// The state of the model will be set to <see langword="true"/> whenever it is restored from a session.
    /// </summary>
    public bool IsFromSession { get; set; } = false;

    public bool TeamInRoundIsSet { get; set; } = false;
    public TeamViewModels.TeamInRoundModel? TeamInRound { get; set; }
        
    public bool TeamIsSet { get; set; } = false;
    public TeamEditorComponentModel? Team { get; set; }

    public TeamVenueSetKind VenueIsSet { get; set; } = TeamVenueSetKind.NotSet;
    public VenueEditorComponentModel? Venue { get; set; }

    /// <summary>
    /// The name of the application tournament
    /// </summary>
    public string? TournamentName { get; set; }
        
    /// <summary>
    /// The ID of the predecessor of the application tournament.
    /// </summary>
    public long? PreviousTournamentId { get; set; }
}
