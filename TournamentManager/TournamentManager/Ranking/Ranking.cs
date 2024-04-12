using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Ranking;

/// <summary>
/// Calculate the ranking table of matches
/// </summary>
public class Ranking
{
    internal readonly IRankComparer RankComparer;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="matchesComplete">The list of completed matches.</param>
    /// <param name="matchesToPlay">The list of matches still to be played.</param>
    /// <param name="rankComparison">The key for the comparer to calculate the ranking table.</param>
    public Ranking(IEnumerable<MatchCompleteRawRow> matchesComplete, IEnumerable<MatchToPlayRawRow> matchesToPlay, RankComparison rankComparison)
    {
        MatchesPlayed = matchesComplete.ToList().AsReadOnly();
        MatchesToPlay = matchesToPlay.ToList().AsReadOnly();
        RankComparer = new RankComparer(rankComparison) {
            UpperDateLimit = DateTime.MaxValue,
            Ranking = this
        };
    }

    /// <summary>
    /// The read-only list of completed matches.
    /// </summary>
    public IList<MatchCompleteRawRow> MatchesPlayed { get; }

    /// <summary>
    /// The read-only list of matches still to be played.
    /// </summary>
    public IList<MatchToPlayRawRow> MatchesToPlay { get; }

    /// <summary>
    /// Get the <see cref="RankingList"/> for the default upper date limit, which
    /// is the maximum match date.
    /// </summary>
    /// <param name="lastUpdatedOn">The date when the matches were last updated.</param>
    /// <returns>Get the <see cref="RankingList"/> for the default upper date limit, which is the maximum match date.</returns>
    public RankingList GetList(out DateTime lastUpdatedOn)
    {
        var upperDateLimit = MatchesPlayed.Count > 0
            ? MatchesPlayed.Max(m => m.MatchDate ?? DateTime.MinValue)
            : MatchesToPlay.Max(m => m.MatchDate ?? DateTime.MaxValue);
        return GetList(upperDateLimit, out lastUpdatedOn);
    }

    /// <summary>
    /// Get the <see cref="RankingList"/> for the given upper date limit.
    /// </summary>
    /// <param name="upperDateLimit">The upper date limit to use for the calculation.</param>
    /// <param name="lastUpdatedOn">The date when the matches were last updated.</param>
    /// <returns>Returns the <see cref="RankingList"/> for the given upper date limit.</returns>
    public RankingList GetList(DateTime upperDateLimit, out DateTime lastUpdatedOn)
    {
        RankComparer.UpperDateLimit = upperDateLimit;
        var rankingList = GetSortedList(GetUnsortedList(upperDateLimit, out lastUpdatedOn));
        rankingList.LastUpdatedOn = lastUpdatedOn;
        rankingList.UpperDateLimit = upperDateLimit;
        return rankingList;
    }

    private RankingList GetSortedList(RankingList rankingList)
    {
        rankingList.Sort(RankComparer);

        for (var i = 0; i < rankingList.Count; i++)
        {
            rankingList[i].Number = i + 1;
        }
        return rankingList;
    }

    private RankingList GetUnsortedList(DateTime upperDateLimit, out DateTime lastUpdatedOn)
    {
        var teamIds = new HashSet<long>();
        MatchesToPlay.ToList().ForEach(m => { teamIds.Add(m.HomeTeamId); teamIds.Add(m.GuestTeamId); });
        MatchesPlayed.ToList().ForEach(m => { teamIds.Add(m.HomeTeamId); teamIds.Add(m.GuestTeamId); });

        return GetUnsortedList(teamIds, upperDateLimit, out lastUpdatedOn);
    }

    /// <summary>
    /// This is the method a <see cref="IRankComparer"/> may call for a ranking of 2 teams.
    /// </summary>
    /// <param name="teamIds">The team IDs to use for the ranking.</param>
    /// <param name="upperDateLimit">The upper date limit to use for the calculation.</param>
    /// <returns>Returns the <see cref="RankingList"/> for the given upper date limit.</returns>
    internal RankingList GetList(IEnumerable<long> teamIds, DateTime upperDateLimit)
    {
        return GetSortedList(GetUnsortedList(teamIds, upperDateLimit, out _));
    }

    private RankingList GetUnsortedList(IEnumerable<long> teamIds, DateTime upperDateLimit, out DateTime lastUpdatedOn)
    {
        var teamIdList = teamIds.ToList();
        lastUpdatedOn = GetLastUpdatedOn(teamIdList);

        var rankingList = new RankingList { UpperDateLimit = upperDateLimit, LastUpdatedOn = lastUpdatedOn };

        foreach (var teamId in teamIdList)
        {
            var rank = CalculateRank(teamId, upperDateLimit);
            rankingList.Add(rank);
        }

        return rankingList;
    }

    private DateTime GetLastUpdatedOn(ICollection<long> teamIdList)
    {
        var lastUpdatedOn = DateTime.UtcNow;
        if (MatchesPlayed.Any())
            lastUpdatedOn = MatchesPlayed
                .Where(m => teamIdList.Contains(m.HomeTeamId) || teamIdList.Contains(m.GuestTeamId))
                .Max(m => m.ModifiedOn);
        else if (MatchesToPlay.Any())
            lastUpdatedOn = MatchesToPlay
                .Where(m => teamIdList.Contains(m.HomeTeamId) || teamIdList.Contains(m.GuestTeamId))
                .Max(m => m.ModifiedOn);

        return lastUpdatedOn;
    }

    private Rank CalculateRank(long teamId, DateTime upperDateLimit)
    {
        var rank = new Rank { Number = -1, TeamId = teamId };

        foreach (var match in GetMatchesPlayedForTeam(teamId, upperDateLimit))
        {
            UpdateRankStatistics(rank, match, teamId);
        }

        return rank;
    }

    private IEnumerable<MatchCompleteRawRow> GetMatchesPlayedForTeam(long teamId, DateTime upperDateLimit)
    {
        return MatchesPlayed
            .Where(m => (m.HomeTeamId == teamId || m.GuestTeamId == teamId) &&
                        m.MatchDate.HasValue &&
                        m.MatchDate.Value.Date <= upperDateLimit.Date);
    }

    private void UpdateRankStatistics(Rank rank, MatchCompleteRawRow match, long teamId)
    {
        var isHomeTeam = match.HomeTeamId == teamId;

        var homeMatchPoints = isHomeTeam ? match.HomeMatchPoints ?? 0 : match.GuestMatchPoints ?? 0;
        var guestMatchPoints = isHomeTeam ? match.GuestMatchPoints ?? 0 : match.HomeMatchPoints ?? 0;
        var homeSetPoints = isHomeTeam ? match.HomeSetPoints ?? 0 : match.GuestSetPoints ?? 0;
        var guestSetPoints = isHomeTeam ? match.GuestSetPoints ?? 0 : match.HomeSetPoints ?? 0;
        var homeBallPoints = isHomeTeam ? match.HomeBallPoints ?? 0 : match.GuestBallPoints ?? 0;
        var guestBallPoints = isHomeTeam ? match.GuestBallPoints ?? 0 : match.HomeBallPoints ?? 0;

        rank.MatchPoints.Home += homeMatchPoints;
        rank.MatchPoints.Guest += guestMatchPoints;
        rank.SetPoints.Home += homeSetPoints;
        rank.SetPoints.Guest += guestSetPoints;
        rank.BallPoints.Home += homeBallPoints;
        rank.BallPoints.Guest += guestBallPoints;

        rank.MatchesWon.Home += homeMatchPoints > guestMatchPoints ? 1 : 0;
        rank.MatchesWon.Guest += homeMatchPoints < guestMatchPoints ? 1 : 0;
        rank.SetsWon.Home += homeSetPoints > guestSetPoints ? 1 : 0;
        rank.SetsWon.Guest += guestSetPoints > homeSetPoints ? 1 : 0;
    }

    /// <summary>
    /// Gets the <see cref="RankingHistory"/> for each match date.
    /// </summary>
    /// <returns>Returns the <see cref="RankingHistory"/> for each match date.</returns>
    public RankingHistory GetRankingHistory()
    {
        return new RankingHistory(this);
    }

    /// <summary>
    /// Gets the list of match dates for all completed matches.
    /// </summary>
    /// <returns>Returns the list of match dates for all completed matches.</returns>
    public List<DateTime> GetMatchDays()
    {
        var matchDates = MatchesPlayed.Where(m => m.MatchDate.HasValue).Select(m => m.MatchDate!.Value.Date).Distinct().ToList();
        matchDates.Sort();
        return matchDates;
    }
}
