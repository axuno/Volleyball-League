namespace TournamentManager.Plan;

/// <summary>
/// Class for the combination of two participants and an optional referee for a match.
/// </summary>
/// <typeparam name="TP">The type of the participant objects.</typeparam>
/// <typeparam name="TR">The type of the referee objects</typeparam>
internal class ParticipantCombination<TP, TR> where TP : struct where TR : struct
{
    /// <summary>
    /// A participant combination of home/guest participants and an optional referee.
    /// </summary>
    /// <param name="turn">The turn number for this combination.</param>
    /// <param name="home">The home participant object.</param>
    /// <param name="guest">The guest participant object.</param>
    /// <param name="referee">The referee object.</param>
    public ParticipantCombination(int turn, TP home, TP guest, TR? referee)
    {
        Turn = turn;
        Home = home;
        Guest = guest;
        Referee = referee;
    }

    /// <summary>
    /// Gets or sets the turn of this combination.
    /// </summary>
    public int Turn { get; set; }

    /// <summary>
    /// Gets or sets the home participant of this combination.
    /// </summary>
    public TP Home { get; set; }

    /// <summary>
    /// Gets or sets the guest participant of this combination.
    /// </summary>
    public TP Guest { get; set; }

    /// <summary>
    /// Gets or sets the referee of this combination.
    /// </summary>
    public TR? Referee { get; set; }

    /// <summary>
    /// Swaps the home and guest participants.
    /// </summary>
    public void SwapHomeGuest()
    {
        (Home, Guest) = (Guest, Home);
    }

    /// <summary>
    /// Returns the string representation of the participants combination.
    /// </summary>
    /// <returns>Returns the string representation of the participants combination.</returns>
    public override string ToString()
    {
        return string.Concat(Home.ToString(), " : ", Guest.ToString(), " / ", Referee?.ToString() ?? "-");
    }
}
