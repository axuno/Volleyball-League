namespace TournamentManager.Ranking;

public class RankingTeamHistory : Dictionary<DateTime, Rank>
{
    public RankingTeamHistory(int capacity)
        : base(capacity)
    {
    }
}
