using System.Collections.Generic;
using System.Linq;
using TournamentManager;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.RankingViewModels;

public class AllTimeTeamModel
{
    private readonly List<RoundLegPeriodRow> _roundLegPeriods;

    public AllTimeTeamModel(List<RankingListRow> rankingList, List<RoundLegPeriodRow> roundLegPeriods)
    {
        RankingList = rankingList;
        _roundLegPeriods = roundLegPeriods;
    }

    public long? SelectedTeamId { get; set; }

    public (string TeamName, string ClubName) GetSelectedTeam()
    {
        var rankRow = RankingList.FirstOrDefault(rl => rl.TeamId == SelectedTeamId);
        if (rankRow == null) return (string.Empty, string.Empty);

        return (rankRow.TeamName, rankRow.ClubName);
    }

    public List<RankingListRow> RankingList { get; set; }

    public DateTimePeriod GetTournamentPeriod(long tournamentId)
    {
        return new DateTimePeriod(_roundLegPeriods.Where(rl => rl.TournamentId == tournamentId).Min(rl => rl.StartDateTime), _roundLegPeriods.Max(rl => rl.EndDateTime));
    }
}
