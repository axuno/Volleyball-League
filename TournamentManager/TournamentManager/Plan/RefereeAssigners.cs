namespace TournamentManager.Plan;

/// <summary>
/// Keeps a list of referee assigners.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
internal static class RefereeAssigners<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    /// <summary>
    /// Returns the referee assigner for the referee assignment type.
    /// </summary>
    /// <param name="refereeAssignmentType">The <see cref="RefereeType"/>.</param>
    /// <param name="referees">The list of referees (optional).</param>
    /// <returns>The <see cref="IRefereeAssigner{TP, TR}"/> for the referee assignment type.</returns>
    public static IRefereeAssigner<TP, TR> GetRefereeAssigner(RefereeType refereeAssignmentType, IList<TR>? referees = null)
    {
        return refereeAssignmentType switch {
            RefereeType.None => new NoRefereeAssigner<TP, TR>(referees),
            RefereeType.Home => new HomeRefereeAssigner<TP, TR>(referees),
            RefereeType.Guest => new GuestRefereeAssigner<TP, TR>(referees),
            RefereeType.OtherFromRound => new OtherFromRoundRefereeAssigner<TP, TR>(referees),
            _ => throw new ArgumentOutOfRangeException(nameof(refereeAssignmentType), refereeAssignmentType, null),
        };
    }
}
