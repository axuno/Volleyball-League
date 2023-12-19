using System.Collections.ObjectModel;

namespace TournamentManager.Plan;

/// <summary>
/// The class for a collection of <see cref="ParticipantCombination{TP, TR}"/> objects.
/// </summary>
/// <typeparam name="TP">The type of the participant objects.</typeparam>
/// <typeparam name="TR">The type of the referee objects</typeparam>
internal class ParticipantCombinations<TP, TR> : Collection<ParticipantCombination<TP, TR>> where TP : struct  where TR : struct
{
    /// <summary>
    /// The <see cref="DateTimePeriod"/>s per turn.
    /// This is used by <see cref="MatchScheduler"/> to set the desired <see cref="DateTimePeriod"/> per turn.
    /// </summary>
    public Dictionary<int, DateTimePeriod?> TurnDateTimePeriods { get; } = new();

    /// <summary>
    /// Gets the list of unique turns of the <see cref="ParticipantCombination{TP,TR}"/> collection.
    /// </summary>
    /// <returns>The list of unique turns of the <see cref="ParticipantCombination{TP,TR}"/> collection.</returns>
    public IEnumerable<int> GetTurns()
    {
        return this.Select(pc => pc.Turn).Distinct().OrderBy(pc => pc);
    }

    /// <summary>
    /// Gets the list of <see cref="ParticipantCombination{TP,TR}"/> objects for a given turn.
    /// </summary>
    /// <param name="turn"></param>
    /// <returns>The list of <see cref="ParticipantCombination{TP,TR}"/> objects for a given turn.</returns>
    public IEnumerable<ParticipantCombination<TP, TR>> GetCombinations(int turn)
    {
        return this.Where(pc => pc.Turn == turn);
    }
}
