namespace TournamentManager
{
	public class Opponent : IOpponent<string>
	{
        public Opponent() : this(null, null) { }

        public Opponent(string home, string guest, string separator = " : ")
        {
            Home = home;
            Guest = guest;
            Separator = separator;
        }

		public string Home { get; }

		public string Guest { get; }

        public string Separator { get; set; }

		public override string ToString() => string.Join(Separator, Home, Guest);
	}
}