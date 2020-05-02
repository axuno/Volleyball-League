namespace TournamentManager.Plan
{
	/// <summary>
	/// Generic class to store the team pairs for a match.
	/// </summary>
	/// <typeparam name="T">The type of the team objects.</typeparam>
	public class TeamCombination<T>
	{
		private T _homeTeam;
		private T _guestTeam;
		private T _referee;

		/// <summary>
		/// Creates a new team combination of home/guest team and a referee.
		/// </summary>
		/// <param name="homeTeam">The home team object.</param>
		/// <param name="guestTeam">The guest team object.</param>
		/// <param name="referee"></param>
		public TeamCombination(T homeTeam, T guestTeam, T referee)
		{
			_homeTeam = homeTeam;
			_guestTeam = guestTeam;
			_referee = referee;
		}

		/// <summary>
		/// Gets or sets the home team of this combination.
		/// </summary>
		public T HomeTeam
		{
			get { return _homeTeam; }
			set { _homeTeam = value; }
		}

		/// <summary>
		/// Gets or sets the guest team of this combination.
		/// </summary>
		public T GuestTeam
		{
			get { return _guestTeam; }
			set { _guestTeam = value; }
		}

		/// <summary>
		/// Gets or sets the referee of this combination.
		/// </summary>
		public T Referee
		{
			get { return _referee; }
			set { _referee = value; }
		}

		/// <summary>
		/// Returns the string representation of the team combination.
		/// </summary>
		/// <returns>Returns the string representation of the team combination.</returns>
		public override string ToString()
		{
			return string.Concat(_homeTeam.ToString(), " : ", _guestTeam.ToString(), " / ", _referee.ToString());
		}
	}
}