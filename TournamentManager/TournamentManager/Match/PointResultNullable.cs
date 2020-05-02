using System;
using System.Collections.Generic;
using System.Text;

namespace TournamentManager.Match
{
    public class PointResultNullable : IOpponent<int?>
    {
        public PointResultNullable()
        { }

        public PointResultNullable(int? home, int? guest)
        {
            Home = home;
            Guest = guest;
        }

        public int? Home { get; set; }
        public int? Guest { get; set; }
    }
}
