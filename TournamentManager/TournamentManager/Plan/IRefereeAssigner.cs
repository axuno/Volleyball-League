namespace TournamentManager.Plan;

/// <summary>
/// Interface for referee assigners.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
internal interface IRefereeAssigner<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    /// <summary>
    /// The referee assignment type.
    /// </summary>
    const RefereeType AssignmentType = RefereeType.None;

    /// <summary>
    /// Returns the referee for the match.
    /// </summary>
    /// <param name="match">The match.</param>
    /// <returns>The referee for the match.</returns>
    TR? GetReferee((int Turn, TP Home, TP Guest) match);
}
