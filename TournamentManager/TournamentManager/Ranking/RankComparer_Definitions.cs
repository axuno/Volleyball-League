namespace TournamentManager.Ranking;
internal partial class RankComparer
{
    public void SetDefinitions(RankComparison comparerType)
    {
        switch (comparerType)
        {
            case RankComparison.LegacyRankComparison:
                SetLegacyRankComparer();
               break;
            case RankComparison.TwoWinningSetsRankComparison:
                SetTwoWinningSetsRankComparer();
                break;
            case RankComparison.ThreeWinningSetsRankComparison:
                SetThreeWinningSetsRankComparer();
                break;
            case RankComparison.HroThreeWinningSetsRankComparison:
                SetHroThreeWinningSetsRankComparer();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(comparerType), comparerType, null);
        }
    }

    /// <summary>
    /// The rules Augsburg volleyball leagues until August 2023
    /// for 2 or 3 winning set matches (depending on season)
    /// </summary>
    private void SetLegacyRankComparer()
    {
        Description =
            """
            To determine the ranking in rounds with matches over 2 or 3 winning sets, the following will be compared in sequence:
            a) The difference of match points (won minus lost)
            b) The difference of set points (won minus lost)
            c) The difference of ball points (won minus lost)
            d) The direct comparison of the matches of teams which are equal in the difference of ball points won and lost.
            """;
        Comparisons =
        [
            SortDownTeamsWithNoMatchesPlayed,
            SortDownMatchesWithNoPoints,
            MatchPointsDifferenceDecides, // a)
            SetPointsDifferenceDecides, // b)
            BallPointsDifferenceDecides, // c)
            DirectComparisonOfTeamsDecides
        ];
    }

    /// <summary>
    /// The rules of Bavarian Volleyball Association (BVV) as of 2020-06-19
    /// for 2 winning set matches.
    /// </summary>
    private void SetTwoWinningSetsRankComparer()
    {
        Description =
            """
            To determine the ranking in rounds with matches over 2 winning sets, the following will be compared in sequence.
            The winner in a 2:0 or 2:1 scores two match points, the loser no match point. Only plus points are awarded.
            a) The match points won
            b) The difference of set points (won minus lost)
            c) The number of sets won
            d) The difference of ball points (won minus lost)
            e) The number of ball points won
            f) The direct comparison of the matches of teams which are equal in "ball points won".
            """;
        Comparisons =
        [
            SortDownTeamsWithNoMatchesPlayed,
            SortDownMatchesWithNoPoints,
            MatchPointsWonDecide, // a)
            SetPointsDifferenceDecides, // b)
            SetsWonDecide, // c)
            BallPointsDifferenceDecides, // d)
            BallPointsWonDecide, // e)
            DirectComparisonOfTeamsDecides
        ];
    }

    /// <summary>
    /// The rules of German Volleyball National League (DVV), in use since season 2013/14
    /// for 3 winning set matches, and applying the 3-point rule.
    /// </summary>
    private void SetThreeWinningSetsRankComparer()
    {
        Description =
            """
            To determine the ranking in rounds with matches over 3 winning sets, the following will be compared in sequence.
            The winner in a 3:0 or 3:1 scores three match points, the loser scores no match point.
            The winner in a 3:2 scores 2 match points, the loser scores one match point.
            Only plus points are awarded.
            a) The match points won
            b) The number of matches won
            c) The ratio of sets won divided by sets lost
            d) The ratio of ball points won divided by ballpoints lost
            e) The direct comparison of the matches of teams which are equal in "ball points ratio".
            """;

        Comparisons =
        [
            SortDownTeamsWithNoMatchesPlayed,
            SortDownMatchesWithNoPoints,
            MatchPointsWonDecide, // a)
            MatchesWonDecide, // b)
            SetPointsRatioDecides, // c)
            BallPointsRatioDecides, // d)
            DirectComparisonOfTeamsDecides
        ];
    }

    /// <summary>
    /// The rules of the Volleyball-Stadtliga Rostock, starting with season 2023/24
    /// for 3 winning set matches, and applying the 3-point rule.
    /// </summary>
    private void SetHroThreeWinningSetsRankComparer()
    {
        /*
        Regeln für die Tabellenermittlung der Stadtliga Rostock:

        Zur Ermittlung der Rangfolge von Spielen werden 3 Gewinnsätze gespielt. Dabei erhält
        * der Gewinner bei einem 3:0 oder 3:1 drei Punkte, der Verlierer keinen Punkt.
        * der Gewinner bei einem 3:2 zwei Punkte und der Verlierer einen Punkt.
        Es werden nur Pluspunkte vergeben.
        Über die Rangfolge von zwei oder mehr Mannschaften entscheidet in absteigender Priorität
        a) die Anzahl der Punkte,
        b) die Satzdifferenz, indem die Anzahl der verlorenen Sätze von der Anzahl der gewonnenen Sätze subtrahiert wird,
        c) bei gleicher Satzdifferenz die Anzahl der gewonnenen Sätze,
        d) die Ballpunktdifferenz, indem die Anzahl der verlorenen Bälle von der Anzahl der gewonnenen Bälle subtrahiert wird,
        e) bei gleicher Balldifferenz die Anzahl der gewonnenen Bälle.
        f) der direkte Vergleich zwischen den Mannschaften
        */
        Description =
            """
            To determine the ranking in rounds with matches over 3 winning sets, the following will be compared in sequence.
            The winner in a 3:0 or 3:1 scores three match points, the loser scores no match point.
            The winner in a 3:2 scores 2 match points, the loser scores one match point.
            Only plus points are awarded.
            a) The match points won
            b) The difference of set points (won minus lost)
            c) The number of sets won
            d) The difference of ball points (won minus lost)
            e) The number of ball points won
            f) The direct comparison of the matches of teams which are equal in "ball points won".
            """;

        Comparisons =
        [
            SortDownTeamsWithNoMatchesPlayed,
            SortDownMatchesWithNoPoints,
            MatchPointsWonDecide, // a)
            SetPointsDifferenceDecides, // b)
            SetsWonDecide, // c)
            BallPointsDifferenceDecides, // d)
            BallPointsWonDecide, // e)
            DirectComparisonOfTeamsDecides
        ];
    }
}
