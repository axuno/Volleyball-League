namespace TournamentManager.RoundRobin;

/// <summary>
/// This generic class optimizes the round-robin list of matches for a group of teams.
/// Gives best results between 3 and 9 teams, while for bigger numbers of teams the results
/// created by the <see cref="RoundRobinSystem{TP}"/> class are already good.
/// </summary>
/// <typeparam name="TP"></typeparam>
/// <typeparam name="TR"></typeparam>
internal class RoundRobinOptimizer<TP, TR> where TP : IEquatable<TP> where TR : IEquatable<TR>
{
    private readonly ICollection<TP> _participants;
    // The original list of matches of the round-robin list
    private readonly ICollection<(int Turn, TP Home, TP Guest)> _roundRobinMatches;
    // The working copy of the matches of the round-robin list
    private readonly List<(int Turn, TP Home, TP Guest)> _workingMatches = new();
    // The turns that have been processed
    private readonly List<int> _turnsProcessed = new();
    // The optimized list of matches
    private readonly List<(int Turn, TP Home, TP Guest)> _result = new();

    /// <summary>
    /// Creates a new instance of the <see cref="RoundRobinOptimizer{TP,TR}"/> class.
    /// </summary>
    /// <param name="participants"></param>
    /// <param name="roundRobinMatches"></param>
    public RoundRobinOptimizer(ICollection<TP> participants, ICollection<(int Turn, TP Home, TP Guest)> roundRobinMatches)
    {
        _participants = participants;
        _roundRobinMatches = roundRobinMatches;
    }

    /// <summary>
    /// Optimizes the list of matches for home/guest matches.
    /// Each participant will have home/guest matches balanced as much as mathematically possible.
    /// Each participant will have only one or two consecutive home/guest matches.
    /// </summary>
    /// <returns></returns>
    public IList<(int, TP Home, TP Guest)> OptimizeHomeGuestMatches()
    {
        _result.Clear();
        _workingMatches.Clear();
        _workingMatches.AddRange(_roundRobinMatches);
        _turnsProcessed.Clear();

        return OptimizeMatchSequences();
    }

    private IList<(int Turn, TP Home, TP Guest)> OptimizeMatchSequences()
    {
        var turns = _roundRobinMatches.Select(m => m.Turn).Distinct().ToList();
        
        // initialize with the first turn
        _turnsProcessed.Add(0);
        _result.AddRange(_workingMatches.Where(t => t.Turn == 0));
        
        while (_turnsProcessed.Count < turns.Count)
        {
            var turnId = _turnsProcessed.Last();

            var nextTurnId = 
                GetTurnOfHomeTeamsNotPlayingEachOther(turnId, _turnsProcessed, _workingMatches) 
                    ?? GetTurnWithMaxHomeTeamsNotPlayingEachOther(turnId, _turnsProcessed, _workingMatches)
                    ?? GetTurnWithLeastConsecutiveHomeOrGuest(turnId, _turnsProcessed, _workingMatches)
                    ?? turns.First(t => !_turnsProcessed.Contains(t));

            //AvoidConsecutiveHomeAwayMatches(nextTurnId);

            var nextTurnMatches = _workingMatches.Where(t => t.Turn == nextTurnId);
            _result.AddRange(nextTurnMatches);
            _turnsProcessed.Add(nextTurnId);
        }
        
        return _result;
    }

    /// <summary>
    /// Gets the turn number of the next turn containing all home teams not playing each other.
    /// </summary>
    /// <param name="lastTurn"></param>
    /// <param name="excludedTurns"></param>
    /// <param name="matches"></param>
    /// <returns>The turn number, if the criteria could be fulfilled or null otherwise.</returns>
    private static int? GetTurnOfHomeTeamsNotPlayingEachOther(int lastTurn, IList<int> excludedTurns, IList<(int Turn, TP Home, TP Guest)> matches)
    {
return null;
        var numTurns = matches.Select(m => m.Turn).Distinct().Count();
        var homeTeams = matches.Where(t => t.Turn == lastTurn).Select(t => t.Home).ToList();
        for (var i = 0; i < numTurns; i++)
        {
            var currentTurn = i;
            if (excludedTurns.Contains(currentTurn)) continue;
            var currentTurnMatches = matches.Where(t => t.Turn == currentTurn).ToList();
            if (currentTurnMatches.All(t => (homeTeams.Contains(t.Home) || homeTeams.Contains(t.Guest)) && !(homeTeams.Contains(t.Home) && homeTeams.Contains(t.Guest))))
            {
                return currentTurn;
            }
        }
        return null;
    }

    private static int? GetTurnWithLeastConsecutiveHomeOrGuest(int lastTurn, IList<int> excludedTurns, IList<(int Turn, TP Home, TP Guest)> matches)
    {
        (int? Turn, int Count) minConsecutive = (null, int.MaxValue);

        var numTurns = matches.Select(m => m.Turn).Distinct().Count();

        var lastTurnParticipants = matches.Where(t => t.Turn == lastTurn).Select(t => t.Home).ToList();
        lastTurnParticipants.AddRange(matches.Where(t => t.Turn == lastTurn).Select(t => t.Guest));

        for (var i = 0; i < numTurns; i++)
        {
            var currentTurn = i;
            if (excludedTurns.Contains(currentTurn)) continue;
            var currentTurnMatches = matches.Where(t => t.Turn == currentTurn).ToList();

            var currentTotal = 0;
            foreach (var lastTurnParticipant in lastTurnParticipants)
            {
                var c = MatchesAnalyzer<TP>.GetLastConsecutiveCounts(lastTurnParticipant, true, currentTurnMatches);
                currentTotal += c.First();
                c = MatchesAnalyzer<TP>.GetLastConsecutiveCounts(lastTurnParticipant, false, currentTurnMatches);
                currentTotal += c.First();
            }

            if (currentTotal < minConsecutive.Count) minConsecutive = (currentTurn, currentTotal);
        }

        return minConsecutive.Turn;
    }

    /// <summary>
    /// Gets the turn number of the next turn with the maximum number of home teams not playing each other.
    /// </summary>
    /// <param name="lastTurn"></param>
    /// <param name="excludedTurns"></param>
    /// <param name="matches"></param>
    /// <returns>The turn number, if the criteria could be fulfilled, or null otherwise.</returns>
    private static int? GetTurnWithMaxHomeTeamsNotPlayingEachOther(int lastTurn, IList<int> excludedTurns, IList<(int Turn, TP Home, TP Guest)> matches)
    {
        var max = new KeyValuePair<int?, int>(null, -1);

        var numTurns = matches.Select(m => m.Turn).Distinct().Count();
        var homeTeams = matches.Where(t => t.Turn == lastTurn).Select(t => t.Home).ToList();
        for (var i = 0; i < numTurns; i++)
        {
            var currentTurn = i;
            if (excludedTurns.Contains(currentTurn)) continue;
            var currentTurnMatches = matches.Where(t => t.Turn == currentTurn).ToList();

            // try to find a turn with the maximum number of matches containing home teams
            // not playing each other
            for (var k = homeTeams.Count; k > 0; k--)
            {
                if (currentTurnMatches.Count(t =>
                         homeTeams.Contains(t.Guest)) != k) 
                    continue;

                /*if (currentTurnMatches.Count(t =>
                        (homeTeams.Contains(t.Home) || homeTeams.Contains(t.Guest)) 
                        && !(homeTeams.Contains(t.Home) && homeTeams.Contains(t.Guest))) != k) 
                    continue;*/
                if (k > max.Value) max = new KeyValuePair<int?, int>(currentTurn, k);
            }
        }
        return max.Key;
    }

    /// <summary>
    /// Avoids consecutive home/guest matches for the same team.
    /// </summary>
    /// <param name="nextTurnId">The turn containing the matches to optimize.</param>
    private void AvoidConsecutiveHomeAwayMatches(int nextTurnId)
    {
        var consecutive = GetParticipantsLastConsecutiveCounts(_result);

        for (var i = 0; i < _workingMatches.Count; i++)
        {
            var match = _workingMatches[i];
            if (match.Turn != nextTurnId) continue;
            if (_turnsProcessed.Count == 1) continue;

            if (consecutive[match.Home].HomeCount >= 2 || consecutive[match.Guest].GuestCount >= 2)
            {
                // swap home/guest
                (match.Home, match.Guest) = (match.Guest, match.Home);
                _workingMatches[i] = match;
            } 
            else if (consecutive[match.Home].HomeCount > consecutive[match.Guest].HomeCount)
            {
                // swap home/guest
                (match.Home, match.Guest) = (match.Guest, match.Home);
                _workingMatches[i] = match;
            } 
            else if (consecutive[match.Home].GuestCount > consecutive[match.Guest].GuestCount)
            {
                // home/guest stays unchanged
            }
            else if (consecutive[match.Home].HomeCount == consecutive[match.Guest].HomeCount
                       && IsConsecutiveHomeOrGuestMatch(match))
            {
                // swap home/guest
                (match.Home, match.Guest) = (match.Guest, match.Home);
                _workingMatches[i] = match;
            }
        }
    }

    /// <summary>
    /// Checks whether either the home or the guest team of the current match has played in the previous turn.
    /// </summary>
    /// <param name="match"></param>
    /// <returns></returns>
    private bool IsConsecutiveHomeOrGuestMatch((int Turn, TP Home, TP Guest) match)
    {
        // Get all matches of the last turn
        var searchTurnMatches = _workingMatches.Where(t => t.Turn == _turnsProcessed.Last()).ToList();

        if (searchTurnMatches.Any(t => t.Home.Equals(match.Home)) ||
            searchTurnMatches.Any(t => t.Guest.Equals(match.Guest)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the number of most recent consecutive home/guest matches for each participant.
    /// </summary>
    /// <param name="matches"></param>
    /// <returns>The list of most recent consecutive home/guest matches for each participant.</returns>
    private Dictionary<TP, (int HomeCount, int GuestCount)> GetParticipantsLastConsecutiveCounts(IList<(int Turn, TP Home, TP Guest)> matches)
    {
        var result = new Dictionary<TP, (int HomeCount, int GuestCount)>();

        foreach (var participant in _participants)
        {
            var homeCount = MatchesAnalyzer<TP>.GetLastConsecutiveCounts(participant, true, matches).FirstOrDefault();
            var guestCount = MatchesAnalyzer<TP>.GetLastConsecutiveCounts(participant, false, matches).FirstOrDefault();
            
            result.TryAdd(participant, (homeCount, guestCount));
        }

        return result;
    }
}

