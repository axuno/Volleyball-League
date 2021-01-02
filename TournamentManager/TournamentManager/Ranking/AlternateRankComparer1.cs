using System;

namespace TournamentManager.Ranking
{
	/// <summary>
	/// The <see cref="IRankComparer"/> for the rules of Bayerischer Volleyballverband as of November 2005
	/// </summary>
	internal class AlternateRankComparer1 : IRankComparer
	{
        private static bool _directCompareRankingInProgress = false;

        /// <summary>
        /// Required CTOR for Activator.CreateInstance(...)
        /// </summary>
        internal AlternateRankComparer1()
        { }

        internal AlternateRankComparer1(Ranking ranking, DateTime upperDateLimit)
        {
            Ranking = ranking;
            UpperDateLimit = upperDateLimit;
        }

        public DateTime UpperDateLimit { get; set; }

        public Ranking? Ranking { get; set; }

        public int Compare(Rank x, Rank y)
		{
			/*
			BVV-Spielordnung 
			Stand: 25.11.05
			http://bvv.volley.de/n/fileadmin/user_all/Download/Ordnungen/spielordnung25112005.pdf

			11. Spielwertung, Nichtantreten von Mannschaften
			11.1 a) Alle Pflichtspiele sind �ber 3 Gewinns�tze auszutragen.
			b) Meisterschaftsspiele der Altersklassen und Pokalspiele bis Bezirksebene k�nnen
			�ber 2 Gewinns�tze ausgetragen werden. Eine bindende Festlegung trifft
			der jeweils zust�ndige Spielausschuss.
			11.2 a) Zur Ermittlung der Rangfolge in Spielrunden und bei Turnieren erhalten gewinnende
			Mannschaften zwei Pluspunkte (2:0), verlierende Mannschaften zwei
			Minuspunkte (0:2). Die Mannschaft, die mehr Pluspunkte aufweist, erh�lt den
			besseren Platz. Bei gleicher Punktzahl ist besser platziert, wer weniger Minuspunkte
			aufweist.
			b) Bei Punktgleichheit von zwei oder mehr Mannschaften entscheidet �ber die
			Platzierung zun�chst die Satzdifferenz (Subtraktionsverfahren). Bei gleicher
			Satzdifferenz z�hlt die Anzahl der gewonnenen S�tze.
			c) Bei Punktgleichheit, gleicher Satzdifferenz und gleicher Anzahl der gewonnenen
			S�tze von zwei oder mehr Mannschaften entscheidet �ber die Platzierung
			die Balldifferenz (Subtraktionsverfahren). Bei gleicher Balldifferenz z�hlt die
			Anzahl der gewonnenen B�lle.
			d) Er gibt sich nach Anwendung der Abs�tze a) bis c) ein Gleichstand f�r zwei
			oder mehr Mannschaften, m�ssen diese Mannschaften nochmals gegeneinander
			spielen; die Entscheidungsspiele sind dann ma�gebend f�r die Platzierung.
			Solche Entscheidungsspiele sollen auf neutralen Pl�tzen stattfinden.
			*/
            if (Ranking is null) throw new NullReferenceException("Ranking property must not be null");
            
            if (x == null || y == null) throw new NullReferenceException($"{nameof(Rank)} arguments must not be null");

            // sort down teams with no matches played
            if (x.MatchesPlayed == 0) return 1;
            if (y.MatchesPlayed == 0) return -1;

            // sort down overruled results with no points at all
            if (x.MatchPoints.Home + x.MatchPoints.Guest + x.SetPoints.Home + x.SetPoints.Guest + x.BallPoints.Home + x.BallPoints.Guest == 0)
                return 1;
            if (y.MatchPoints.Home + y.MatchPoints.Guest + y.SetPoints.Home + y.SetPoints.Guest + y.BallPoints.Home + y.BallPoints.Guest == 0)
                return -1;

			// a)
			if (x.MatchPoints.Home < y.MatchPoints.Home)
				return 1;
			else if (x.MatchPoints.Home > y.MatchPoints.Home)
				return -1;

			if (x.MatchPoints.Guest > y.MatchPoints.Guest)
				return 1;
			else if (x.MatchPoints.Guest < y.MatchPoints.Guest)
				return -1;

			// b)
			if ((x.SetPoints.Home - x.SetPoints.Guest) < (y.SetPoints.Home - y.SetPoints.Guest))
				return 1;
			else if ((x.SetPoints.Home - x.SetPoints.Guest) > (y.SetPoints.Home - y.SetPoints.Guest))
				return -1;

			if (x.SetPoints.Home < y.SetPoints.Home)
				return 1;
			else if (x.SetPoints.Home > y.SetPoints.Home)
				return -1;

			// c)
			if ((x.BallPoints.Home - x.BallPoints.Guest) < (y.BallPoints.Home - y.BallPoints.Guest))
				return 1;
			else if ((x.BallPoints.Home - x.BallPoints.Guest) > (y.BallPoints.Home - y.BallPoints.Guest))
				return -1;

			if (x.BallPoints.Home < y.BallPoints.Home)
				return 1;
			else if (x.BallPoints.Home > y.BallPoints.Home)
				return -1;

			// d) taking already played matches instead of new match to play
			// avoid an infinite loop
			if (!_directCompareRankingInProgress && x.MatchesPlayed > 0 && y.MatchesPlayed > 0)
			{
				_directCompareRankingInProgress = true;
				var directCompareRanking = Ranking.GetList(new[] { x.TeamId, y.TeamId }, UpperDateLimit);
				_directCompareRankingInProgress = false;

				if (directCompareRanking[0].TeamId == x.TeamId)
					return -1;
				else
					return 1;
			}
			else
			{
				// if directCompareRanking is reached twice, both teams must have the same score,
				// so we return a random winner by comparing Team Ids
				return (x.TeamId < y.TeamId ? 1 : -1);
			}
		}
	}
}