namespace TournamentManager.Plan;

/// <summary>
/// Specifies the referee type for a match.
/// </summary>
public enum RefereeType
{
    /// <summary>
    /// By default, no referee is assigned.
    /// </summary>
    None = 0,
    /// <summary>
    /// The home participant is the referee.
    /// </summary>
    Home,
    /// <summary>
    /// The guest participant is the referee.
    /// </summary>
    Guest,
    /// <summary>
    /// 
    /// </summary>
    OtherOfGroup
}
