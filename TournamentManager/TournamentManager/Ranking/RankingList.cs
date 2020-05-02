using System;
using System.Collections.Generic;

namespace TournamentManager.Ranking
{
	public class RankingList : List<Rank>
	{
		private DateTime _upperDateLimit;
        private DateTime _lastUpdatedOn;


		public DateTime UpperDateLimit
		{
			get => _upperDateLimit;
            internal set => _upperDateLimit = value;
        }

        public DateTime LastUpdatedOn
        {
            get => _lastUpdatedOn;
            internal set => _lastUpdatedOn = value;
        }
	}
}