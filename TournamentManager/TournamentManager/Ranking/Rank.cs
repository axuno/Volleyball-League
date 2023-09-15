namespace TournamentManager.Ranking;

/// <summary>
/// The <see cref="Rank"/> calculated for completed matches.
/// </summary>
public class Rank
{
    /// <summary>
    /// The number of the rank.
    /// </summary>
    public int Number { get; internal set; } = -1;

    /// <summary>
    /// The team ID.
    /// </summary>
    public long TeamId { get; internal set; } = -1;

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
    public PointResult MatchesWon { get; internal set; } = new(0,0);

    /// <summary>
    /// The number of match points won and lost.
    /// </summary>
    public PointResult MatchPoints { get; internal set; } = new(0, 0);

    /// <summary>
    /// The number of sets won and lost.
    /// </summary>
    public PointResult SetsWon { get; internal set; } = new(0, 0);

    /// <summary>
    /// The number of set points won and lost.
    /// </summary>
    public PointResult SetPoints { get; internal set; } = new(0, 0);

    /// <summary>
    /// The number of ball points won and lost.
    /// </summary>
    public PointResult BallPoints { get; internal set; } = new(0, 0);

    /// <summary>
    /// Gets the rank as a string.
    /// </summary>
    /// <returns>Returns the rank as a string.</returns>
    public override string ToString()
    {
        return Number.ToString();
    }
}
