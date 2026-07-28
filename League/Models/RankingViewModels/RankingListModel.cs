using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace League.Models.RankingViewModels;

public class RankingListModel
{
    public TournamentEntity? Tournament { get; set; }

    public List<RankingListRow> RankingList { get; set; } = [];

    public Dictionary<long, FileInfo> ChartFileInfos { get; set; } = [];

    public long? ActiveRoundId { get; set; }
}
