namespace TournamentManager.RoundRobin;

internal interface IRoundRobinSystem<TP> where TP : struct, IEquatable<TP>
{
    /// <summary>
    /// Gets the list of participants.
    /// </summary>
    ICollection<TP> Participants { get; }

    /// <summary>
    /// Generates a list of matches using the round-robin system with for the given participants.
    /// </summary>
    /// <returns>A list of matches represented as <see cref="ValueTuple"/>s of turn and home / guest participants.</returns>
    IList<(int Turn, TP Home, TP Guest)> GenerateMatches();
}
