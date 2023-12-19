using System.Xml.Serialization;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// The class contains all configuration data for a tournament.
/// </summary>
[YAXLib.Attributes.YAXComment("Configuration data for a tournament")]
public class TournamentContext : ITournamentContext
{
    /// <summary>
    /// Gets or sets the <see cref="ITenant"/> this context refers to.
    /// </summary>
    [YAXLib.Attributes.YAXDontSerialize]
    public ITenant? Tenant { get; set; }
        
    /// <summary>
    /// The ID of the tournament which will be used for new teams' applications.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The ID of the tournament which will be used for new teams' applications")]
    public long ApplicationTournamentId { get; set; }

    /// <summary>
    /// Return true, if teams' applications are allowed, otherwise false.
    /// </summary>
    [YAXLib.Attributes.YAXComment("True, if teams' applications are allowed, otherwise false")]
    public bool ApplicationAllowed { get; set; }
        
    /// <summary>
    /// The deadline for new teams' applications.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The deadline for new teams' applications")]
    public DateTime ApplicationDeadline { get; set; }

    /// <summary>
    /// The ID of the tournament which will be used for to display maps.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The ID of the tournament which will be used for to display maps")]
    public long MapTournamentId { get; set; }
        
    /// <summary>
    /// The ID of the tournament which will be used to display team data.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The ID of the tournament which will be used to display team data")]
    public long TeamTournamentId { get; set; }
        
    /// <summary>
    /// The ID of the tournament which will be used to display the match plan.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The ID of the tournament which will be used to display the match plan")]
    public long MatchPlanTournamentId { get; set; }
        
    /// <summary>
    /// The ID of the tournament which will be used to display match results and tables.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The ID of the tournament which will be used to display match results and tables")]
    public long MatchResultTournamentId { get; set; }

    /// <summary>
    /// A set of rules for creating and editing fixtures.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The rules which apply for creating and editing fixtures")]
    public FixtureRuleSet FixtureRuleSet { get; set; } = new();

    /// <summary>
    ///	The max. number of days after RealStart where results may be changed. Negative value means 'unlimited'.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The max. number of days after RealStart where results may be changed. Negative value means 'unlimited'")]
    public int MaxDaysForResultCorrection { get; set; }

    /// <summary>
    /// Rules for team master data
    /// </summary>
    [YAXLib.Attributes.YAXComment("The rules which apply for creating and editing team data")]
    public TeamRules TeamRuleSet { get; set; } = new();

    /// <summary>
    /// Rules for organizing referees.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Rules for organizing referees")]
    public RefereeRules RefereeRuleSet { get; set; } = new();
}

public class FixtureRuleSet
{
    /// <summary>
    /// The time when matches start and end normally (e.g. from 18:00 - 21:00 h)
    /// </summary>
    [YAXLib.Attributes.YAXComment("The time when matches start and end normally (e.g. from 18:00 - 21:00 h)")]
    public RegularMatchStartTime RegularMatchStartTime { get; set; } = new();

    /// <summary>
    /// The duration which is used to generate fixtures and to determine periods
    /// where a venue is occupied. Default is 2 hours.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The duration which is used to generate fixtures and to determine periods where a venue is occupied")]
    public TimeSpan PlannedDurationOfMatch { get; set; } = new(0, 2,0,0);

    /// <summary>
    /// If set to true, when editing a fixture the match time must be set (cannot be null)
    /// </summary>
    [YAXLib.Attributes.YAXComment("If set to true, when editing a fixture the match time must be set")]
    public bool PlannedMatchDateTimeMustBeSet { get; set; } = true;

    /// <summary>
    /// If set to true, the planned match time must no include any dates found in <see cref="TournamentManager.DAL.EntityClasses.ExcludeMatchDateEntity"/> entries.
    /// </summary>
    [YAXLib.Attributes.YAXComment("If set to true, the planned match time must no include any dates found in ExcludeMatchDate table entries")]
    public bool CheckForExcludedMatchDateTime { get; set; } = true;

    /// <summary>
    /// If set to true, the planned match time must stay within the current leg date boundaries.
    /// If false, the planned time must stay with in any leg date boundaries.
    /// </summary>
    [YAXLib.Attributes.YAXComment("If set to true, the planned match time must stay within the current leg date boundaries. If false, the planned time must stay with in any leg date boundaries.")]
    public bool PlannedMatchTimeMustStayInCurrentLegBoundaries { get; set; }

    /// <summary>
    /// If set to true, when editing a fixture the venue must be set (cannot be null)
    /// </summary>
    [YAXLib.Attributes.YAXComment("If set to true, when editing a fixture the venue must be set")]
    public bool PlannedVenueMustBeSet { get; set; } = true;

    /// <summary>
    /// If true, when checking whether teams already have a match at a certain moment,
    /// only the date will be used (i.e. only 1 match per calendar date).
    /// </summary>
    [YAXLib.Attributes.YAXComment("If true, when checking whether teams already have a match at a certain moment, only the date will be used (i.e. only 1 match per calendar date)")]
    public bool UseOnlyDatePartForTeamFreeBusyTimes { get; set; } = false;
}

/// <summary>
/// The time when matches should normally start (e.g. from 18:00 - 21:00 h).
/// To disable checks, set MinDayTime to 00:00:00 and MaxDayTime to 23:59:59
/// </summary>
public class RegularMatchStartTime
{
    /// <summary>
    /// Earliest start time for a match.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Earliest start time for a match")]
    public TimeSpan MinDayTime { get; set; } = new(0,18,0,0);
    /// <summary>
    /// Latest start time for a match.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Latest start time for a match")]
    public TimeSpan MaxDayTime { get; set; } = new(0, 21,0,0);
}

/// <summary>
/// Rules for teams' master data
/// </summary>
public class TeamRules
{
    /// <summary>
    /// Rules for teams' home match time
    /// </summary>
    [YAXLib.Attributes.YAXComment("Rules for the HomeMatchTime of a team")]
    public HomeMatchTime HomeMatchTime { get; set; } = new();

    /// <summary>
    /// Rules for the <see cref="HomeVenue"/> of a team.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Rules for the HomeVenue of a team.")]
    public HomeVenue HomeVenue { get; set; } = new();
}

/// <summary>
/// Rules for organizing referees.
/// </summary>
public class RefereeRules
{
    /// <summary>
    /// Rules for teams' home match time
    /// </summary>
    [YAXLib.Attributes.YAXComment("Rules for organizing referees")]
    public Plan.RefereeType RefereeType { get; set; } = Plan.RefereeType.None;
}

/// <summary>
/// Rules for the <see cref="HomeMatchTime"/> of a team.
/// </summary>
[YAXLib.Attributes.YAXComment("Rules for the HomeMatchTime of a team")]
public class HomeMatchTime
{
    /// <summary>
    /// If <see langword="true"/>, <see cref="HomeMatchTime"/> will be shown on team forms.
    /// If <see langword="false"/>, <see cref="IsEditable"/>, <see cref="DaysOfWeekRange"/> and <see cref="ErrorIfNotInDaysOfWeekRange"/> are irrelevant.
    /// </summary>
    [YAXLib.Attributes.YAXComment("If true, HomeMatchTime will be shown on team forms. If false, IsEditable, DaysOfWeekRange and ErrorIfNotInDaysOfWeekRange are irrelevant.")]
    public bool IsEditable { get; set; } = true;
        
    /// <summary>
    /// If <see langword="true"/>, the <see cref="HomeMatchTime"/> must be set, i.e. cannot be null/unspecified.
    /// </summary>
    [YAXLib.Attributes.YAXComment("If true, the HomeMatchTime must be set, i.e. cannot be null/unspecified")]
    public bool MustBeSet { get; set; } = true;
        
    /// <summary>
    /// Allowed days of a week
    /// </summary>
    [YAXLib.Attributes.YAXComment("Allowed days of a week")]
    [XmlArrayItem(nameof(DaysOfWeekRange))]
    public List<DayOfWeek> DaysOfWeekRange { get; set; } = new(new[]{DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday});
        
    /// <summary>
    /// If <see langword="true"/> entries not in <see cref="DaysOfWeekRange"/> are errors (else: warning)
    /// </summary>
    [YAXLib.Attributes.YAXComment("If true, entries not in 'DaysOfWeekRange' are errors (else: warning)")]
    public bool ErrorIfNotInDaysOfWeekRange { get; set; } = false;
}

/// <summary>
/// Rules for the <see cref="HomeVenue"/> of a team.
/// </summary>
[YAXLib.Attributes.YAXComment("Rules for the HomeVenue of a team.")]
public class HomeVenue
{
    /// <summary>
    /// If <see langword="true"/>, the <see cref="HomeVenue"/> must be set, i.e. cannot be null/unspecified.
    /// If <see langword="false"/>, when auto-creating fixtures the team will only have away-matches (is always the guest team).
    /// </summary>
    [YAXLib.Attributes.YAXComment("""
                                  If true, the HomeVenue must be set, i.e. cannot be null/unspecified.
                                  If false, when auto-creating fixtures the team will only have away-matches (i.e. is always the guest team).
                                  """)]
    public bool MustBeSet { get; set; } = true;
}
