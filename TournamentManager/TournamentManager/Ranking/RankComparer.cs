namespace TournamentManager.Ranking;
internal partial class RankComparer : IRankComparer
{
    // if true, a recursive call has started
    private bool _directCompareRankingInProgress = false;

    /// <summary>
    /// Required CTOR for <see cref="Activator.CreateInstance(Type)"/> used by <see cref="Ranking"/>.
    /// </summary>
    public RankComparer(RankComparison rankComparison)
    {
        SetDefinitions(rankComparison);
    }

    public delegate bool ComparisonDelegate<in TR1, in TR2, out TO>(Rank x, Rank y, out int result);

    private List<ComparisonDelegate<Rank, Rank, int>> Comparisons { get; set; } = null!;

    /// <inheritdoc/>
    /// <exception cref="NullReferenceException"></exception>
    public int Compare(Rank? x, Rank? y)
    {
        CheckForNull();
        System.Diagnostics.Debug.Assert(x != null && y != null, @"Rank arguments must not be null");

        foreach (var comparisonDelegate in Comparisons)
        {
            if (comparisonDelegate.Invoke(x, y, out var result)) return result;
        }

        throw new InvalidOperationException(@"List of comparisons is missing the final comparison");
    }

    /// <inheritdoc/>
    public string Description { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public DateTime UpperDateLimit { get; set; } = DateTime.MaxValue;

    /// <inheritdoc/>
    public Ranking? Ranking { get; set; }

    private void CheckForNull()
    {
        if (Ranking is null) throw new InvalidOperationException("Ranking property must not be null");
    }

    /// <summary>
    /// Teams which have no matches played (yet) go to the end of the ranking table.
    /// </summary>
    private static bool SortDownTeamsWithNoMatchesPlayed(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.MatchesPlayed == 0)
        {
            result = 1;
            return true;
        }

        if (y.MatchesPlayed == 0)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Matches which have no points at all (i.e. that have been overruled this way)
    /// go to the end of the ranking table.
    /// </summary>
    private static bool SortDownMatchesWithNoPoints(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.MatchPoints.Home + x.MatchPoints.Guest + x.SetPoints.Home + x.SetPoints.Guest + x.BallPoints.Home + x.BallPoints.Guest == 0)
        {
            result = 1;
            return true;
        }

        if (y.MatchPoints.Home + y.MatchPoints.Guest + y.SetPoints.Home + y.SetPoints.Guest + y.BallPoints.Home + y.BallPoints.Guest == 0)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The match points won decide.
    /// </summary>
    private static bool MatchPointsWonDecide(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.MatchPoints.Home < y.MatchPoints.Home)
        {
            result = 1;
            return true;
        }

        if (x.MatchPoints.Home > y.MatchPoints.Home)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The match point difference (i.e. home points minus guest points) decides.
    /// </summary>
    private static bool MatchPointsDifferenceDecides(Rank x, Rank y, out int result)
    {
        result = 0;

        if ((x.MatchPoints.Home - x.MatchPoints.Guest) < (y.MatchPoints.Home - y.MatchPoints.Guest))
        {
            result = 1;
            return true;
        }

        if ((x.MatchPoints.Home - x.MatchPoints.Guest) > (y.MatchPoints.Home - y.MatchPoints.Guest))
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The number of matches won decides.
    /// </summary>
    private static bool MatchesWonDecide(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.MatchesWon.Home < y.MatchesWon.Home)
        {
            result = 1;
            return true;
        }

        if (x.MatchesWon.Home > y.MatchesWon.Home)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The set points ratio (i.e. home set points divided by guest set points) decides
    /// </summary>
    private static bool SetPointsRatioDecides(Rank x, Rank y, out int result)
    {
        result = 0;

        if (((float?) x.SetPoints.Home / x.SetPoints.Guest) < ((float?) y.SetPoints.Home / y.SetPoints.Guest))
        {
            result = 1;
            return true;
        }

        if (((float?) x.SetPoints.Home / x.SetPoints.Guest) > ((float?) y.SetPoints.Home / y.SetPoints.Guest))
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The set point difference (i.e. home points minus guest points) decides.
    /// </summary>
    private static bool SetPointsDifferenceDecides(Rank x, Rank y, out int result)
    {
        result = 0;

        if ((x.SetPoints.Home - x.SetPoints.Guest) < (y.SetPoints.Home - y.SetPoints.Guest))
        {
            result = 1;
            return true;
        }

        if ((x.SetPoints.Home - x.SetPoints.Guest) > (y.SetPoints.Home - y.SetPoints.Guest))
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The number of sets won decides.
    /// </summary>
    private static bool SetsWonDecide(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.SetsWon.Home < y.SetsWon.Home)
        {
            result = 1;
            return true;
        }

        if (x.SetsWon.Home > y.SetsWon.Home)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The ball points ratio (i.e. home ball points divided by guest ball points) decides
    /// </summary>
    private static bool BallPointsRatioDecides(Rank x, Rank y, out int result)
    {
        result = 0;

        if (((float?) x.BallPoints.Home / x.BallPoints.Guest) < ((float?) y.BallPoints.Home / y.BallPoints.Guest))
        {
            result = 1;
            return true;
        }

        if (((float?) x.BallPoints.Home / x.BallPoints.Guest) > ((float?) y.BallPoints.Home / y.BallPoints.Guest))
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The ball point difference (i.e. home points minus guest points) decides.
    /// </summary>
    private static bool BallPointsDifferenceDecides(Rank x, Rank y, out int result)
    {
        result = 0;

        if ((x.BallPoints.Home - x.BallPoints.Guest) < (y.BallPoints.Home - y.BallPoints.Guest))
        {
            result = 1;
            return true;
        }

        if ((x.BallPoints.Home - x.BallPoints.Guest) > (y.BallPoints.Home - y.BallPoints.Guest))
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The number of ball points won decides.
    /// </summary>
    private static bool BallPointsWonDecide(Rank x, Rank y, out int result)
    {
        result = 0;

        if (x.BallPoints.Home < y.BallPoints.Home)
        {
            result = 1;
            return true;
        }

        if (x.BallPoints.Home > y.BallPoints.Home)
        {
            result = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// The matches that two neighboring have played against each other decide.
    /// This must be the last method of the decision list, because <b>it always return <see langword="true"/></b>.
    /// </summary>
    private bool DirectComparisonOfTeamsDecides(Rank x, Rank y, out int result)
    {
        // avoid an infinite loop
        if (!_directCompareRankingInProgress && x.MatchesPlayed > 0 && y.MatchesPlayed > 0)
        {
            _directCompareRankingInProgress = true;
            var directCompareRanking = Ranking!.GetList(new[] { x.TeamId, y.TeamId }, UpperDateLimit);
            _directCompareRankingInProgress = false;

            if (directCompareRanking[0].TeamId == x.TeamId)
            {
                result = -1;
                return true;
            }
            result = 1;
            return true;
        }

        // if directCompareRanking is reached twice, both teams must have the same score,
        // so we return the team with the higher TeamId (a random alternative)
        result = x.TeamId < y.TeamId ? 1 : -1;
        return true;
    }
}
