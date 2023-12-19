namespace TournamentManager.Plan;

/// <summary>
/// Assigns the referee to the home participant.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
internal class HomeRefereeAssigner<TP, TR> : IRefereeAssigner<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    public HomeRefereeAssigner(IList<TR>? _)
    {
    }

    /// <summary>
    /// The referee assignment type.
    /// </summary>
    public const RefereeType AssignmentType = RefereeType.Home;

    /// <inheritdoc/>
    public TR? GetReferee((int Turn, TP Home, TP Guest) match)
    {
        return (TR?) (object) match.Home;
    }
}
