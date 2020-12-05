using System;

namespace TournamentManager.Ranking
{
    /// <summary>
    /// The standard <see cref="IRankComparer"/> used with Mixedliga Augsburg.
    /// </summary>
	internal class StandardRankComparer : IRankComparer
	{
        private static bool _directCompareRankingInProgress = false;

        internal StandardRankComparer(Ranking ranking, DateTime upperDateLimit)
        {
            Ranking = ranking;
            UpperDateLimit = upperDateLimit;
        }

        public DateTime UpperDateLimit { get; set; }

        public Ranking Ranking { get; set; }

        public int Compare(Rank x, Rank y)
		{
            if (x == null || y == null) throw new NullReferenceException($"{nameof(Rank)} arguments must not be null");

            // sort down teams with no matches played
            if (x.MatchesPlayed == 0) return 1;
            if (y.MatchesPlayed == 0) return -1;

            // sort down overruled results with no points at all
            if (x.MatchPoints.Home + x.MatchPoints.Guest + x.SetPoints.Home + x.SetPoints.Guest + x.BallPoints.Home + x.BallPoints.Guest == 0)
                return 1;
            if (y.MatchPoints.Home + y.MatchPoints.Guest + y.SetPoints.Home + y.SetPoints.Guest + y.BallPoints.Home + y.BallPoints.Guest == 0)
                return -1;

            if ((x.MatchPoints.Home - x.MatchPoints.Guest) < (y.MatchPoints.Home - y.MatchPoints.Guest))
                return 1;
            else if ((x.MatchPoints.Home - x.MatchPoints.Guest) > (y.MatchPoints.Home - y.MatchPoints.Guest))
                return -1;

            if ((x.SetPoints.Home - x.SetPoints.Guest) < (y.SetPoints.Home - y.SetPoints.Guest))
                return 1;
            else if ((x.SetPoints.Home - x.SetPoints.Guest) > (y.SetPoints.Home - y.SetPoints.Guest))
                return -1;

            if ((x.BallPoints.Home - x.BallPoints.Guest) < (y.BallPoints.Home - y.BallPoints.Guest))
                return 1;
            else if ((x.BallPoints.Home - x.BallPoints.Guest) > (y.BallPoints.Home - y.BallPoints.Guest))
                return -1;

            // avoid an infinite loop
            if (!_directCompareRankingInProgress && x.MatchesPlayed > 0 && y.MatchesPlayed > 0)
            {
                _directCompareRankingInProgress = true;
                var directCompareRanking = Ranking.GetList(new[] { x.TeamId, y.TeamId }, UpperDateLimit);
                _directCompareRankingInProgress = false;

                if (directCompareRanking[0].TeamId == x.TeamId)
                    return -1;
                else
                    return 1;
            }
            else
            {
                // if directCompareRanking is reached twice, both teams must have the same score,
                // so we return a random winner by comparing Team Ids
                return (x.TeamId < y.TeamId ? 1 : -1);
            }
        }
	}
}