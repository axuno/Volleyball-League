using System.Collections.ObjectModel;
using TournamentManager.Data;

namespace TournamentManager.Plan;

public class TeamCombinationGroup<T> : Collection<TeamCombination<T>>
{
    internal DateTimePeriod DateTimePeriod = new();
}