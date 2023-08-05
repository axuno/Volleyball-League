using System.Collections.ObjectModel;

namespace TournamentManager.Plan;

public class TeamCombinationGroup<T> : Collection<TeamCombination<T>>
{
    internal DateTimePeriod DateTimePeriod = new();
}