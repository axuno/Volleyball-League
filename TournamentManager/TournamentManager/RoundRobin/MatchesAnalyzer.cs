namespace TournamentManager.RoundRobin;

internal class MatchesAnalyzer<TP>  where TP : IEquatable<TP>
{
    /// <summary>
    /// Gets the total number of home/guest matches for each participant.
    /// </summary>
    /// <param name="matches"></param>
    /// <returns>The total number of home/guest matches for each participant.</returns>
    internal static Dictionary<TP, (int HomeCount, int GuestCount)> GetHomeGuestCount(IList<(int Turn, TP Home, TP Guest)> matches)
    {
        var result = new Dictionary<TP, (int HomeCount, int GuestCount)>();

        foreach (var match in matches)
        {
            result.TryAdd(match.Home, (0, 0));
            result.TryAdd(match.Guest, (0, 0));

            result[match.Home] = (result[match.Home].HomeCount + 1, result[match.Home].GuestCount);
            result[match.Guest] = (result[match.Guest].HomeCount, result[match.Guest].GuestCount + 1);
        }

        return result;
    }

    /// <summary>
    /// Gets the participants with their number of unbalanced home/guest matches.
    /// For odd number of participants, home/guest matches must be the same.
    /// For even number of participants, the home/guest difference may be 1
    /// (e.g. for 6 participants: 2 home, 3 guest matches)
    /// </summary>
    /// <param name="matches"></param>
    /// <returns>The participants with their number of unbalanced home/guest matches.</returns>
    public static Dictionary<TP, (int HomeCount, int GuestCount)> GetUnbalancedHomeGuestCount(
        IList<(int Turn, TP Home, TP Guest)> matches)
    {
        // Get the total number of home/guest matches for all participants
        var homeGuestCount = GetHomeGuestCount(matches);
        var unbalancedHomeGuestCount = new Dictionary<TP, (int HomeCount, int GuestCount)>();

        foreach (var hgc in homeGuestCount)
        {
            var minCount = (int) Math.Floor((homeGuestCount.Keys.Count - 1) / 2.0);
            var maxCount = (int) Math.Ceiling((homeGuestCount.Keys.Count - 1) / 2.0 + .1);
            if (hgc.Value.HomeCount < minCount || hgc.Value.GuestCount > maxCount || hgc.Value.GuestCount < minCount || hgc.Value.HomeCount > maxCount)
            {
                unbalancedHomeGuestCount.Add(hgc.Key, hgc.Value);
            }
        }

        return unbalancedHomeGuestCount;
    }

    /// <summary>
    /// Gets the maximum number of consecutive home/guest matches for the given participant.
    /// </summary>
    /// <param name="participant"></param>
    /// <param name="matches"></param>
    /// <returns>The maximum number of consecutive home/guest matches for the given participant.</returns>
    public static (int HomeCount, int GuestCount) GetMaxConsecutiveHomeGuestCount(TP participant, IList<(int Turn, TP Home, TP Guest)> matches)
    {
        var homeCount = GetLastConsecutiveCounts(participant, true, matches).Max();
        var guestCount = GetLastConsecutiveCounts(participant, false, matches).Max();
        return (homeCount, guestCount);
    }

    /// <summary>
    /// Gets the number of most recent consecutive home/guest matches for the given participant.
    /// </summary>
    /// <param name="participant"></param>
    /// <param name="forHome"></param>
    /// <param name="matches"></param>
    /// <returns>The list of most recent consecutive home/guest matches for the given participant.</returns>
    public static IEnumerable<int> GetLastConsecutiveCounts(TP participant, bool forHome, IList<(int Turn, TP Home, TP Guest)> matches)
    {
        using var e = matches.Reverse().GetEnumerator();
        for (var more = e.MoveNext(); more;)
        {
            var first = forHome ? e.Current.Home : e.Current.Guest;
            if (first.Equals(participant))
            {
                var count = 1;
                while (more && e.MoveNext())
                {
                    first = forHome ? e.Current.Home : e.Current.Guest;
                    var second = forHome ? e.Current.Guest : e.Current.Home;

                    if (first.Equals(participant))
                    {
                        count++;
                    }

                    if (second.Equals(participant))
                    {
                        break;
                    }
                }
                yield return count;
            }
            else
            {
                yield return 0;
            }
            more = e.MoveNext();
        }
    }
}

