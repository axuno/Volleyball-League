namespace TournamentManager.Plan;

/// <summary>
/// Assigns the referee to another participant from the round.
/// </summary>
/// <typeparam name="TP">The type of the participant.</typeparam>
/// <typeparam name="TR">The type of the referee.</typeparam>
public class OtherFromRoundRefereeAssigner<TP, TR> : IRefereeAssigner<TP, TR> where TP : struct, IEquatable<TP> where TR : struct, IEquatable<TR>
{
    private readonly ParticipantCombinations<TP, TR> _participantCombinations = new();
    private readonly IList<TR> _referees;

    public OtherFromRoundRefereeAssigner(IList<TR>? referees = null)
    {
        _referees = referees ?? throw new ArgumentNullException(nameof(referees));
    }

    /// <summary>
    /// The referee assignment type.
    /// </summary>
    public const RefereeType AssignmentType = RefereeType.OtherFromRound;

    /// <inheritdoc/>
    public TR? GetReferee((int Turn, TP Home, TP Guest) match)
    {
        var lastMaxRefereeCount = int.MaxValue;
        var lastMaxReferee = _referees[0];

        foreach (var participant in _referees)
        {
            var currentRefereeCount = GetNumOfRefereeCombinations(participant);
            if (currentRefereeCount >= lastMaxRefereeCount ||
                participant.Equals(match.Home) || participant.Equals(match.Guest) ||
                IsLastReferee(participant)) continue;

            lastMaxRefereeCount = currentRefereeCount;
            lastMaxReferee = participant;
        }
        _participantCombinations.Add(new ParticipantCombination<TP, TR>(0, match.Home, match.Guest, (TR?) (object?) lastMaxReferee));

        return lastMaxReferee;

    }

    /// <summary>
    /// Checks whether the <paramref name="referee"/> was referee in the last match.
    /// Always returns false if the match collection is empty.
    /// </summary>
    /// <param name="referee">The referee to check whether it was referee in the last match.</param>
    /// <returns>Return true, if the referee was referee in the last match, false otherwise.</returns>
    private bool IsLastReferee(TR referee)
    {
        return _participantCombinations.Count != 0 && referee.Equals(_participantCombinations[^1].Referee);
    }

    /// <summary>
    /// Calculates the number of matches a referee was assigned referee.
    /// </summary>
    /// <param name="referee">The referee to calculate the number of referee matches.</param>
    /// <returns>The number of referee matches for the participant.</returns>
    private int GetNumOfRefereeCombinations(TR referee)
    {
        return _participantCombinations.Count(match => match.Referee != null && match.Referee.Equals(referee));
    }
}
