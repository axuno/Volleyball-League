using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.RankingViewModels;

public class RankingListModel
{
    public TournamentEntity? Tournament { get; set; }

    public List<RankingListRow> RankingList { get; set; } = new();

    public Dictionary<long, System.IO.FileInfo> ChartFileInfos { get; set; } = new();

    public long? ActiveRoundId { get; set; }
}
