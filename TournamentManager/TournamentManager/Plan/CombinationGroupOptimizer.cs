using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TournamentManager.Plan;

/// <summary>
/// Specifies the way matches are grouped.
/// </summary>
public enum CombinationGroupOptimization
{
    NoGrouping,
    GroupWithAlternatingHomeGuest,
    LeastGroupsPossible
}

/// <summary>
/// The class will optimize match combinations in way, that groups of matches
/// can be played without overlapping teams (playing teams, referee teams).
/// </summary>
/// <typeparam name="T">The type of the team objects. Objects must have IComparable implemented.</typeparam>
internal class CombinationGroupOptimizer<T>
{
    private readonly TeamCombinationGroup<T> _group;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="group">A collection of team  combinations with type <typeparamref name="T"/> objects.</param>
    internal CombinationGroupOptimizer(TeamCombinationGroup<T> group)
    {
        _group = group;
    }

    /// <summary>
    /// Groups the calculated team combinations for matches.  in a way, that most matches
    /// can be played in parallel.
    /// </summary>
    /// <param name="optiType">Optimization type for groups. Differences can be seen with an uneven number of teams.</param>
    /// <returns>Return a collection containing collections of team combinations.</returns>
    internal Collection<TeamCombinationGroup<T>> GetBundledGroups(CombinationGroupOptimization optiType)
    {
        var combinationsQueue = new Queue<TeamCombination<T>>(_group.Count);
        TeamCombinationGroup<T> group;
        var bundledGroups = new Collection<TeamCombinationGroup<T>>();

        // create the FIFO queue
        foreach (var combination in _group)
        {
            combinationsQueue.Enqueue(combination);
        }

        switch (optiType)
        {
            case CombinationGroupOptimization.NoGrouping:
                // every group contains a collection with only 1 match
                while (combinationsQueue.Count > 0)
                {
                    group = new TeamCombinationGroup<T> { combinationsQueue.Dequeue() };
                    bundledGroups.Add(group);
                }
                break;

            case CombinationGroupOptimization.GroupWithAlternatingHomeGuest:
                group = new TeamCombinationGroup<T>();
                while (combinationsQueue.Count > 0)
                {
                    var combination = combinationsQueue.Dequeue();
                    if (AnyTeamExistsInGroup(combination, group))
                    {
                        bundledGroups.Add(group);
                        group = new TeamCombinationGroup<T>();
                    }
                    group.Add(combination);
                }
                if (group.Count > 0)
                {
                    bundledGroups.Add(group);
                }
                break;

            case CombinationGroupOptimization.LeastGroupsPossible:
                while (combinationsQueue.Count > 0)
                {
                    var tmpGroup = new List<TeamCombination<T>>();
                    tmpGroup.AddRange(combinationsQueue);

                    group = new TeamCombinationGroup<T>();
                    foreach (var combination in tmpGroup)
                    {
                        if (!AnyTeamExistsInGroup(combination, group))
                        {
                            group.Add(combinationsQueue.Dequeue());
                        }
                    }
                    bundledGroups.Add(group);
                }
                break;
        }
        return bundledGroups;
    }

    /// <summary>
    /// Checks, whether one of the existing matches contains any of the two or three teams of a certain match.
    /// </summary>
    /// <param name="combination"></param>
    /// <param name="group"></param>
    /// <returns>Returns true, one of the existing matches contains any of the two or three teams of a certain match, false otherwise.</returns>
    private static bool AnyTeamExistsInGroup(TeamCombination<T> combination, TeamCombinationGroup<T> group)
    {
        var teams = new Stack<T>(30);
        foreach (var t in group)
        {
            teams.Push(t.HomeTeam);
            teams.Push(t.GuestTeam);
            teams.Push(t.Referee);
        }
        while (teams.Count > 0)
        {
            var team = teams.Pop();
            if (Comparer<T>.Default.Compare(team, combination.HomeTeam) == 0 ||
                Comparer<T>.Default.Compare(team, combination.GuestTeam) == 0 ||
                Comparer<T>.Default.Compare(team, combination.Referee) == 0)
                return true;
        }
        return false;
    }
}
