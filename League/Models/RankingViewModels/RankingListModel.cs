using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Data;
using TournamentManager.Ranking;

namespace League.Models.RankingViewModels;

public class RankingListModel
{
    public TournamentEntity Tournament { get; set; }

    public List<RankingListRow> RankingList { get; set; }

    public Dictionary<long, System.IO.FileInfo> ChartFileInfos { get; set; }

    public long? ActiveRoundId { get; set; }
}