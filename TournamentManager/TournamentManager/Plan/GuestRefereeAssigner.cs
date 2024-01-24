namespace TournamentManager.Plan;

/// <summary>
/// Assigns the referee to the guest participant.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
public class GuestRefereeAssigner<TP, TR> : IRefereeAssigner<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    public GuestRefereeAssigner(IList<TR>? _ = null)
    {
    }

    /// <summary>
    /// The referee assignment type.
    /// </summary>
    public const RefereeType AssignmentType = RefereeType.Guest;

    /// <inheritdoc/>
    public TR? GetReferee((int Turn, TP Home, TP Guest) match)
    {
        return (TR?) (object) match.Guest;
    }
}
