using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentManager;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.RankingViewModels;

public class AllTimeTournamentModel
{
    private readonly List<RoundLegPeriodRow> _roundLegPeriods;

    public AllTimeTournamentModel(List<RankingListRow> rankingList, List<RoundLegPeriodRow> roundLegPeriods)
    {
        RankingList = rankingList;
        _roundLegPeriods = roundLegPeriods;
    }

    public long? SelectedTournamentId { get; set; }

    public List<RankingListRow> RankingList { get; set; }

    public DateTimePeriod GetTournamentPeriod(long tournamentId)
    {
        return new DateTimePeriod(_roundLegPeriods.Where(rl => rl.TournamentId == tournamentId).Min(rl => rl.StartDateTime), _roundLegPeriods.Where(rl => rl.TournamentId == tournamentId).Max(rl => rl.EndDateTime));
    }

    public List<(long Id, string Name, DateTimePeriod Period)> GetAllTournaments()
    {
        var tournaments = new List<(long Id, string Name, DateTimePeriod Period)>();
        RankingList.GroupBy(g => g.TournamentId, (key, grp) => grp.First()).ToList().ForEach((row) => tournaments.Add((row.TournamentId, row.TournamentName, GetTournamentPeriod(row.TournamentId))));
        return tournaments.OrderBy(t => t.Period.Start).ToList();
    }
}