using System;
using System.Collections.Generic;
using TournamentManager.DAL.EntityClasses;

namespace League.Components;

public class VenueSelectorComponentModel
{
    [Flags]
    public enum Criteria
    {
        /// <summary>
        /// Show all venues.
        /// </summary>
        AllVenues = 0,
        /// <summary>
        /// Show a selectable row for "venue not specified".
        /// </summary>
        NotSpecified = 1,
        /// <summary>
        /// Show the venue of the teams from team list parameter.
        /// </summary>
        VenuesOfTeams = 2,
        /// <summary>
        /// Show venues active for the tournament from tournament parameter.
        /// </summary>
        Active = 4,
        /// <summary>
        /// Show venues which are not in use in the tournament from tournament parameter.
        /// </summary>
        Unused = 8
    }

    /// <summary>
    /// Get or set the <see cref="Filter"/> <see cref="Criteria"/> for venues.
    /// </summary>
    public Criteria Filter { get; set; } = 0;

    /// <summary>
    /// Get or set the <see cref="Group"/> <see cref="Criteria"/> for venues.
    /// </summary>
    public Criteria Group { get; set; } = 0;

    /// <summary>
    /// This is the key for a "not specified" venue. If this value is null, "not specified" cannot be selected
    /// </summary>
    public long? VenueNotSpecifiedKey { get; set; } = null;

    /// <summary>
    /// The list of all venues.
    /// </summary>
    public List<VenueEntity> AllVenues { get; set; }

    /// <summary>
    /// The list of venues belonging to a list of team ids that was a parameter for this component.
    /// </summary>
    public List<VenueEntity> VenuesOfTeams { get; set; }

    /// <summary>
    /// The list of active venues to select for this fixture.
    /// </summary>
    public List<VenueEntity> ActiveVenues { get; set; }

    /// <summary>
    /// The list of unused venues to select for this fixture.
    /// </summary>
    public List<VenueEntity> UnusedVenues { get; set; }
}