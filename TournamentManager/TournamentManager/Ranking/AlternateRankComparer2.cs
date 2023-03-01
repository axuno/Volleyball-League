using System;

namespace TournamentManager.Ranking;

/// <summary>
/// The <see cref="IRankComparer"/> for the rules of Bayerischer Volleyballverband as of June 2019
/// </summary>
internal class AlternateRankComparer2 : IRankComparer
{
    private static bool _directCompareRankingInProgress = false;
        
    /// <summary>
    /// Required CTOR for Activator.CreateInstance(...)
    /// </summary>
    internal AlternateRankComparer2()
    {
        throw new NotImplementedException("This comparer is not implemented yet according to the given rules.");
    }

    internal AlternateRankComparer2(Ranking ranking, DateTime upperDateLimit) : this()
    {
        Ranking = ranking;
        UpperDateLimit = upperDateLimit;
    }

    public DateTime UpperDateLimit { get; set; } = DateTime.MinValue;

    public Ranking? Ranking { get; set; }

    public int Compare(Rank? x, Rank? y)
    {
        if (Ranking == null) throw new NullReferenceException($"{nameof(Ranking)} cannot be null.");

        /*
        BVV-Spielordnung 
        Stand: 28.06.2019
        http://www.volleyball-bayern.de/fileadmin/user_upload/Bezirke/Niederbayern/Spielwesen/VSPO_2019.pdf

        11.2	
        
        a)	Zur Ermittlung der Rangfolge in Spielrunden und bei Turnieren erhalten
              - bei Spielen über 3 Gewinnsätze
              der Gewinner bei einem 3:0 oder 3:1 drei Punkte,
              der Gewinner bei einem 3:2 zwei Punkte und der Verlierer einen Punkt.
              Der Verlierer erhält bei einem 1:3 oder 0:3 keinen Punkt.
              Es werden nur Pluspunkte vergeben.
              Über die Rangfolge von zwei oder mehr Mannschaften entscheidet in absteigender Priorität
              a1) die Anzahl der Punkte,
              a2) die Anzahl gewonnener Spiele,
              a3) der Satzquotient, indem die Anzahl der gewonnenen Sätze durch die Anzahl der verlorenen Sätze dividiert wird,
              a4) der Ballpunktquotient, indem die Anzahl der gewonnenen Ballpunkte durch die Anzahl der verlorenen Ballpunkte dividiert wird,
              a5) der direkte Vergleich zwischen den Mannschaften, wobei die Kriterien nach a1) bis a4) zur Berechnung der Rangfolge herangezogen werden.
              Ergibt sich nach Anwendung der Ziffern a1) bis a5) ein Gleichstand für zwei oder mehr Mannschaften, müssen diese Mannschaften nochmals gegeneinander spielen; die Entscheidungsspiele sind dann maßgebend für die Platzierung. Solche Entscheidungsspiele sollen auf neutralen Plätzen stattfinden. Bei Turnieren kann in der Ausschreibung eine hiervon abweichende Regelung getroffen werden.
         
        b)	Zur Ermittlung der Rangfolge in Spielrunden und bei Turnieren erhalten
              - bei Spielen über 2 Gewinnsätze
              der Gewinner bei einem 2:0 oder 2:1 zwei Punkte, der Verlierer keinen Punkt.
              Es werden nur Pluspunkte vergeben.
              Über die Rangfolge von zwei oder mehr Mannschaften entscheidet in absteigender Priorität
              b1) die Anzahl der Punkte,
              b2) die Satzdifferenz, indem die Anzahl der verlorenen Sätze von der Anzahl der gewonnenen Sätze subtrahiert wird,
              b3) bei gleicher Satzdifferenz die Anzahl der gewonnenen Sätze,
              b4) die Ballpunktdifferenz, indem die Anzahl der verlorenen Bälle von der Anzahl der gewonnenen Bälle subtrahiert wird,
              b5) bei gleicher Balldifferenz die Anzahl der gewonnenen Bälle.
              Ergibt sich nach Anwendung der Ziffern b1) bis b5) ein Gleichstand für zwei oder mehr Mannschaften, müssen diese Mannschaften nochmals gegeneinander spielen; die Entscheidungsspiele sind dann maßgebend für die Platzierung. Solche Entscheidungsspiele sollen auf neutralen Plätzen stattfinden. Bei Turnieren kann in der Ausschreibung eine hiervon abweichende Regelung getroffen werden.
        */

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