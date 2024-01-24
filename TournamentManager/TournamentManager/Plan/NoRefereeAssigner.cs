namespace TournamentManager.Plan;

/// <summary>
/// Assigns the default referee for the match.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
internal class NoRefereeAssigner<TP, TR> : IRefereeAssigner<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    public NoRefereeAssigner(IList<TR>? _)
    {
    }

    /// <summary>
    /// The referee assignment type.
    /// </summary>
    public const RefereeType AssignmentType = RefereeType.None;

    /// <inheritdoc/>
    public TR? GetReferee((int Turn, TP Home, TP Guest) match)
    {
        return default;
    }
}
