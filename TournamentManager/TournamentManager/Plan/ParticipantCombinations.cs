using System.Collections.ObjectModel;

namespace TournamentManager.Plan;

/// <summary>
/// The class for a collection of <see cref="ParticipantCombination{T}"/> objects.
/// </summary>
/// <typeparam name="T"></typeparam>
internal class ParticipantCombinations<T> : Collection<ParticipantCombination<T>>
{
    /// <summary>
    /// The <see cref="DateTimePeriod"/>s per turn.
    /// This is used by <see cref="MatchScheduler"/> to set the desired <see cref="DateTimePeriod"/> per turn.
    /// </summary>
    public Dictionary<int, DateTimePeriod?> TurnDateTimePeriods { get; } = new();

    /// <summary>
    /// Gets the list of unique turns of the <see cref="ParticipantCombination{T}"/> collection.
    /// </summary>
    /// <returns></returns>
    public IEnumerable<int> GetTurns()
    {
        return this.Select(pc => pc.Turn).Distinct().OrderBy(pc => pc);
    }
}
