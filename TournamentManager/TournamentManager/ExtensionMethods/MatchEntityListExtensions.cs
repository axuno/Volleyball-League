using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// Extension methods for <see cref="MatchEntity"/> <see cref="IList{T}"/>s
/// </summary>
public static class MatchEntityListExtensions
{
    /// <summary>
    /// Returns the previous matches relative to the <paramref name="currentIndex"/> for the given <paramref name="teamIds"/>.
    /// </summary>
    /// <param name="matches"></param>
    /// <param name="currentIndex">Starts searching for matches BEFORE the index.</param>
    /// <param name="teamIds">The <see cref="MatchEntity.HomeTeamId"/> or <see cref="MatchEntity.GuestTeamId"/> to search.</param>
    /// <param name="includeUndefinedStartOrVenue">Set to <c>true</c> to include with missing <see cref="MatchEntity.PlannedStart"/> or <see cref="MatchEntity.PlannedEnd"/> or <see cref="MatchEntity.VenueId"/> </param>
    /// <returns>The <see cref="IEnumerable{T}"/> of indexes for previous matches relative to the <paramref name="currentIndex"/> for the given <paramref name="teamIds"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IEnumerable<int> GetPreviousMatches(this IList<MatchEntity> matches, int currentIndex, long[] teamIds, bool includeUndefinedStartOrVenue)
    {
        if (currentIndex < 0 || currentIndex >= matches.Count)
            throw new ArgumentOutOfRangeException(nameof(currentIndex), currentIndex, @$"Index must be less than {matches.Count - 1}.");

        var index = currentIndex;
        while (index > 0)
        {
            index--;
            var current = matches[index];
            if ((teamIds.Contains(current.HomeTeamId) || teamIds.Contains(current.GuestTeamId)) &&
                (includeUndefinedStartOrVenue || current is
                    { PlannedStart: not null, PlannedEnd: not null, VenueId: not null }))
                yield return index;
        }
    }

    /// <summary>
    /// Returns the next matches relative to the <paramref name="currentIndex"/> for the given <paramref name="teamIds"/>.
    /// </summary>
    /// <param name="matches"></param>
    /// <param name="currentIndex">Starts searching for matches AFTER the index.</param>
    /// <param name="teamIds">The <see cref="MatchEntity.HomeTeamId"/> or <see cref="MatchEntity.GuestTeamId"/> to search.</param>
    /// <param name="includeUndefinedStartOrVenue">Set to <c>true</c> to include with missing <see cref="MatchEntity.PlannedStart"/> or <see cref="MatchEntity.PlannedEnd"/> or <see cref="MatchEntity.VenueId"/> </param>
    /// <returns>The <see cref="IEnumerable{T}"/> of indexes for next matches relative to the <paramref name="currentIndex"/> for the given <paramref name="teamIds"/></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static IEnumerable<int> GetNextMatches(this IList<MatchEntity> matches, int currentIndex, long[] teamIds, bool includeUndefinedStartOrVenue)
    {
        if (currentIndex < 0 || currentIndex >= matches.Count)
            throw new ArgumentOutOfRangeException(nameof(currentIndex), currentIndex, @$"Index must be less than {matches.Count - 1}.");

        var index = currentIndex;
        while (index < matches.Count - 1)
        {
            index++;
            var current = matches[index];
            if ((teamIds.Contains(current.HomeTeamId) || teamIds.Contains(current.GuestTeamId)) &&
                (includeUndefinedStartOrVenue || current is
                    { PlannedStart: not null, PlannedEnd: not null, VenueId: not null }))
                yield return index;
        }
    }
}
