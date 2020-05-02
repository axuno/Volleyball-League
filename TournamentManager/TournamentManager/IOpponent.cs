namespace TournamentManager
{
	public interface IOpponent<T>
	{
		T Home { get; }

		T Guest { get; }

		string ToString();
	}
}