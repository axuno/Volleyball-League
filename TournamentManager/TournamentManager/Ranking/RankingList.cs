using System;
using System.Collections.Generic;

namespace TournamentManager.Ranking;

public class RankingList : List<Rank>
{
    public DateTime UpperDateLimit { get; internal set; }

    public DateTime LastUpdatedOn { get; internal set; }
}