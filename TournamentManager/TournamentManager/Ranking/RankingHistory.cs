using System;
using System.Collections.Generic;
using TournamentManager.DAL.EntityClasses;


namespace TournamentManager.Ranking
{
	public class RankingHistory
	{
		private readonly Ranking _ranking;
		private List<RankingList> _history = new List<RankingList>();

		internal RankingHistory(Ranking ranking)
		{
			_ranking = ranking;
			ReCalculate();
		}

		public void ReCalculate()
		{
			List<DateTime> days = GetMatchDays();
			_history.Clear();
			_history = new List<RankingList>(days.Count);

			foreach (var day in days)
			{
				_history.Add(_ranking.GetList(day, out var lastUpdatedOn));
			}
		}

		public List<RankingList> GetByMatchDay()
		{
			return _history;
		}

		public RankingTeamHistory GetByTeam(long teamId)
		{
			var teamHistory = new RankingTeamHistory(_history.Count);
			foreach (var list in _history)
			{
				foreach (var rank in list)
				{
					if (rank.TeamId == teamId)
					{
						teamHistory.Add(list.UpperDateLimit.Date, rank);
					}
				}
			}
			return teamHistory;
		}

		public List<DateTime> GetMatchDays()
		{
			return _ranking.GetMatchDays();
		}

	}
}