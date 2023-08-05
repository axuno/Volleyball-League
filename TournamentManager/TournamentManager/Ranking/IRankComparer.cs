namespace TournamentManager.Ranking;

internal interface IRankComparer : IComparer<Rank>
{
    Ranking? Ranking { get; set; }
    DateTime UpperDateLimit { get; set; }
}