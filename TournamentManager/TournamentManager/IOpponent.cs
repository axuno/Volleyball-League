namespace TournamentManager
{
	public interface IOpponent<out T>
	{
		T Home { get; }

		T Guest { get; }

		string? ToString();
	}
}