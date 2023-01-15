namespace TournamentManager;

public class Opponent : IOpponent<string>
{
    public Opponent() : this(string.Empty, string.Empty) { }

    public Opponent(string home, string guest, string separator = " : ")
    {
        Home = home;
        Guest = guest;
        Separator = separator;
    }

    public string Home { get; }

    public string Guest { get; }

    public string Separator { get; set; }

    public override string ToString()
    {
        return string.Join(Separator, Home, Guest);
    }
}