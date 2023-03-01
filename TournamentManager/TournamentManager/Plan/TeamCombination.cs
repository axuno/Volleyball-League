namespace TournamentManager.Plan;

/// <summary>
/// Generic class to store the team pairs for a match.
/// </summary>
/// <typeparam name="T">The type of the team objects.</typeparam>
public class TeamCombination<T>
{
    /// <summary>
    /// Creates a new team combination of home/guest team and a referee.
    /// </summary>
    /// <param name="homeTeam">The home team object.</param>
    /// <param name="guestTeam">The guest team object.</param>
    /// <param name="referee"></param>
    public TeamCombination(T homeTeam, T guestTeam, T referee)
    {
        HomeTeam = homeTeam;
        GuestTeam = guestTeam;
        Referee = referee;
    }

    /// <summary>
    /// Gets or sets the home team of this combination.
    /// </summary>
    public T HomeTeam { get; set; }

    /// <summary>
    /// Gets or sets the guest team of this combination.
    /// </summary>
    public T GuestTeam { get; set; }

    /// <summary>
    /// Gets or sets the referee of this combination.
    /// </summary>
    public T Referee { get; set; }

    /// <summary>
    /// Returns the string representation of the team combination.
    /// </summary>
    /// <returns>Returns the string representation of the team combination.</returns>
    public override string ToString()
    {
        return string.Concat(HomeTeam?.ToString() ?? nameof(HomeTeam), " : ", GuestTeam?.ToString() ?? nameof(GuestTeam), " / ", Referee?.ToString() ?? nameof(Referee));
    }
}