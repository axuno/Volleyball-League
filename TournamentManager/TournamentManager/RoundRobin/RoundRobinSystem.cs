namespace TournamentManager.RoundRobin;

// Inspired by Nikolay Kostov's suggestion
// on https://stackoverflow.com/questions/1293058/round-robin-tournament-algorithm-in-c-sharp
/// <summary>
/// This generic class calculates all matches for a group of participants.
/// The round robin system is applied, i.e. all participants in the group play each other.
/// Then the number of home/guest matches is analyzed and optimized.
/// </summary>
/// <typeparam name="TP">The <see langword="struct"/> participant type.</typeparam>
internal class RoundRobinSystem<TP> : IRoundRobinSystem<TP> where TP : struct, IEquatable<TP>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RoundRobinSystem{TP}"/> class.
    /// </summary>
    /// <param name="participants">The collection of participants.</param>
    public RoundRobinSystem(ICollection<TP> participants)
    {
        Participants = participants;
    }

    /// <inheritdoc/>
    public ICollection<TP> Participants { get; }


    /// <summary>
    /// Generates a list of matches using the round-robin system with any number of participants.
    /// </summary>
    /// <returns>A list of matches represented as <see cref="ValueTuple"/>s of turn and participants.</returns>
    public IList<(int Turn, TP Home, TP Guest)> GenerateMatches()
    {
        /*
        Round robin match calculations are as follows.
        Example with 5 participants (which total in 10 matches):
                +-   A -+    -+ -+
                |       |     |  |
             +- | +- B -+ -+  |  |
             |  | |        |  |  |
          +- |  | |  C -+ -+  | -+
          |  |  | |     |     |
          |  |  | +- D -+ -+ -+
          |  |  |          |
          +- +- +-   E    -+
        */

        // Create a new list of participants that we can modify.
        var participants = new List<TP?>();
        Participants.ToList().ForEach(p => participants.Add(p));

        // If the number of participants is odd, add a default value to make it even.
        if (participants.Count % 2 != 0)
        {
            participants.Add(default);
        }

        // Calculate the number of turns required to ensure that
        // each participant plays every other participant exactly once.
        var numTurns = participants.Count - 1;

        // Calculate the number of participants required for each turn.
        var halfSize = participants.Count / 2;

        // Create a working list of participants for each turn.
        // The first participant is always the first participant and is not included in the list.
        // The other participants are the remaining participants, split into two halves.
        var pList = new List<TP?>();
        // Add the list except the first participant.
        pList.AddRange(participants.Skip(1));

        var pListSize = pList.Count;

        // Create a list of matches.
        var matches = new List<(int Turn, TP Home, TP Guest)>();

        // Generate the matches for each turn.
        for (var turn = 0; turn < numTurns; turn++)
        {
            // Add the match between the first participant and the participant for the turn.
            var secondParticipant = pList[turn % pListSize];
            if (!secondParticipant.Equals(default))
            {
                // Alternate home/guest matches.
                matches.Add(turn % 2 == 0
                    ? (turn + 1, (TP) participants[0]!, (TP) secondParticipant)
                    : (turn + 1, (TP) secondParticipant, (TP) participants[0]!));
            }

            // Add the matches between the other participants.
            for (var idx = 1; idx < halfSize; idx++)
            {
                var firstParticipant = pList[(turn + idx) % pListSize];
                secondParticipant = pList[(turn + pListSize - idx) % pListSize];
                if (!firstParticipant.Equals(default) && !secondParticipant.Equals(default))
                    // Alternate home/guest matches.
                    matches.Add(idx % 2 == 0
                        ? (turn + 1, (TP) firstParticipant, (TP) secondParticipant)
                        : (turn + 1, (TP) secondParticipant, (TP) firstParticipant));
            }
        }

        FixUnbalancedHomeGuestCountsInLastTurn(matches, numTurns, Participants);

        return matches;
    }

    /// <summary>
    /// After the last turn some participants have one more home match than guest matches.
    /// This happens in the last turn of the round-robin system for an odd number of participants.
    /// The method fixes the home/guest counts in the last turn of the matches list.
    /// <para/>
    /// Note: For odd numbers of participants, consecutive home/guest matches should be 1 for all.
    ///       This is currently only the case for 3 und 5 participants.
    /// </summary>
    /// <param name="matches"></param>
    /// <param name="lastTurn"></param>
    /// <param name="participants"></param>
    /// <returns></returns>
    private static void FixUnbalancedHomeGuestCountsInLastTurn(
        IList<(int Turn, TP Home, TP Guest)> matches, int lastTurn, ICollection<TP> participants)
    {
        // Only odd number of participants require an adjustment
        if (participants.Count % 2 == 0) return;

        // It's enough to check the counts for home matches:
        // If the home count of a participant is bigger than the guest count,
        // the opponents guest count is at the same time bigger than the home count.
        var tooBigHomeCount = MatchesAnalyzer<TP>.GetUnbalancedHomeGuestCount(matches)
            .Where(c => c.Value.HomeCount > c.Value.GuestCount)
            .ToDictionary(hgc => hgc.Key, hgc => hgc.Value);

        for (var i = 0; i < matches.Count; i++)
        {
            if (matches[i].Turn != lastTurn) continue;
            if (tooBigHomeCount.ContainsKey(matches[i].Home))
            {
                matches[i] = (matches[i].Turn, matches[i].Guest, matches[i].Home);
            }
        }
    }
}
