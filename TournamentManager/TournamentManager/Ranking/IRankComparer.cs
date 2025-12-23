namespace TournamentManager.Ranking;

/// <summary>
/// The interface for rank comparers.
/// </summary>
internal interface IRankComparer : IComparer<Rank>
{
    /// <summary>
    /// The <see cref="TournamentManager.Ranking.RankComparison"/> used by the comparer.
    /// </summary>
    RankComparison RankComparison { get; }

    /// <summary>
    /// The description how the comparer works.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// A reference to the class to calculate the ranking table of matches.
    /// Used by <see cref="IRankComparer"/>s to compare the match results of 2 neighboring teams.
    /// </summary>
    Ranking Ranking { get; set; }

    /// <summary>
    /// The maximum <see cref="DateTime"/> of match dates that are included in the ranking table.
    /// Used by <see cref="IRankComparer"/>s to compare the match results of 2 neighboring teams.
    /// </summary>
    DateTime UpperDateLimit { get; set; }
}
