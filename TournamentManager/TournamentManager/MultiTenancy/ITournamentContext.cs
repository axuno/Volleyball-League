namespace TournamentManager.MultiTenancy;

/// <summary>
/// The class contains all configuration data for a tournament.
/// </summary>
public interface ITournamentContext
{
    /// <summary>
    /// Gets or sets the <see cref="ITenant"/> this context refers to.
    /// </summary>
    ITenant? Tenant { get; set; }
        
    /// <summary>
    /// The ID of the tournament which will be used for new teams' applications.
    /// </summary>
    long ApplicationTournamentId { get; set; }

    /// <summary>
    /// Return true, if teams' applications are allowed, otherwise false.
    /// </summary>
    bool ApplicationAllowed { get; set; }

    /// <summary>
    /// The deadline for new teams' applications.
    /// </summary>
    DateTime ApplicationDeadline { get; set; }

    /// <summary>
    /// The ID of the tournament which will be used for to display maps.
    /// </summary>
    long MapTournamentId { get; set; }

    /// <summary>
    /// The ID of the tournament which will be used to display team data.
    /// </summary>
    long TeamTournamentId { get; set; }

    /// <summary>
    /// The ID of the tournament which will be used to display the match plan.
    /// </summary>
    long MatchPlanTournamentId { get; set; }

    /// <summary>
    /// The ID of the tournament which will be used to display match results and tables.
    /// </summary>
    long MatchResultTournamentId { get; set; }

    /// <summary>
    /// A set of rules for creating and editing fixtures.
    /// </summary>
    FixtureRuleSet FixtureRuleSet { get; set; }

    /// <summary>
    ///	The max. number of days after RealStart where results may be changed. Negative value means 'unlimited'.
    /// </summary>
    int MaxDaysForResultCorrection { get; set; }

    /// <summary>
    /// Rules for team master data
    /// </summary>
    TeamRules TeamRuleSet { get; set; }
}