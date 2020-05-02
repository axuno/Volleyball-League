using System;
using System.Collections.Generic;

namespace TournamentManager.Ranking
{
	public class RankingTeamHistory : Dictionary<DateTime, Rank>
	{
		public RankingTeamHistory()
			: base()
		{
		}

		public RankingTeamHistory(int capacity)
			: base(capacity)
		{
		}
	}
}