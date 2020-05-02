using TournamentManager.DAL.EntityClasses;
using TournamentManager.Match;

namespace TournamentManager.Ranking
{
	/// <summary>
	/// The <see cref="Rank"/> calculated for completed matches.
	/// </summary>
    public class Rank
	{
		public Rank()
		{
			Number = -1;
			TeamId = -1;
			MatchesWonAndLost = new PointResult(0,0);
			MatchPoints = new PointResult(0, 0);
			SetPoints = new PointResult(0, 0);
			BallPoints = new PointResult(0, 0);
		}

		/// <summary>
		/// The number of the rank.
		/// </summary>
		public int Number { get; internal set; }

		/// <summary>
		/// The team ID.
		/// </summary>
		public long TeamId { get; internal set; }

		/// <summary>
		/// The number of matches played.
		/// </summary>
		public int MatchesPlayed { get; internal set; }

		/// <summary>
		/// The number of matches still to be played.
		/// </summary>
        public int MatchesToPlay { get; internal set; }

		/// <summary>
		/// The number of matches the team won and lost.
		/// </summary>
		public PointResult MatchesWonAndLost { get; internal set; }

		/// <summary>
		/// The number of match points won and lost.
		/// </summary>
        public PointResult MatchPoints { get; internal set; }

		/// <summary>
		/// The number of set points won and lost.
		/// </summary>
		public PointResult SetPoints { get; internal set; }

		/// <summary>
		/// The number of ball points won and lost.
		/// </summary>
		public PointResult BallPoints { get; internal set; }

		/// <summary>
		/// Gets the rank a string.
		/// </summary>
		/// <returns>Returns the rank as a string.</returns>
		public override string ToString()
		{
			return Number.ToString();
		}
	}
}