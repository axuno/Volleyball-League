using System;
using System.Collections.Generic;
using System.Linq;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Match;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="SetEntity"/> <see cref="List{T}"/> extensions to DAL entities, which can't be located in generic CustomExtensions
/// because of dependencies to classes of <see cref="TournamentManager"/>.
/// </summary>
public static class SetEntityListExtensions
{
    /// <summary>
    /// Gets the number of sets won by the home and guest team.
    /// </summary>
    /// <param name="setList">The <see cref="IList{SetEntity}"/> for which the calculation takes place.</param>
    /// <returns>Returns a <see cref="IOpponent{T}"/> with the number of won sets for home and guest team.</returns>
    public static IOpponent<int> GetSetsWon(this IList<SetEntity> setList)
    {
        var setsWon = new PointResult(setList.Count(set => set.HomeBallPoints > set.GuestBallPoints), setList.Count(set => set.HomeBallPoints < set.GuestBallPoints));
        return setsWon;
    }

    /// <summary>
    /// Gets the maximum of sets won by home or guest team.
    /// </summary>
    /// <param name="setList">The <see cref="IList{SetEntity}"/> for which the calculation takes place.</param>
    /// <returns>Returns the maximum of won sets by home or guest team</returns>
    public static int MaxBestOf(this IList<SetEntity> setList)
    {
        var setsWon = setList.GetSetsWon();
        return setsWon.Home < setsWon.Guest ? setsWon.Guest : setsWon.Home;
    }

    /// <summary>
    /// Assigns &quot;set points&quot; to each of the <see cref="SetEntity"/>s in the list.
    /// For &quot;Best-of&quot; matches, <see cref="SetEntity.IsTieBreak"/> is flagged, too.
    /// </summary>
    /// <param name="setList">An <see cref="IList{SetEntity}"/>.</param>
    /// <param name="setRule">The <see cref="SetRuleEntity"/> with the set rules to apply.</param>
    /// <param name="matchRule">The <see cref="MatchRuleEntity"/> with the set rules to apply.</param>
    /// <returns>Returns the <see cref="IList{SetEntity}"/> after &quot;set points&quot; were set.</returns>
    public static IList<SetEntity> CalculateSetPoints(this IList<SetEntity> setList, SetRuleEntity setRule, MatchRuleEntity matchRule)
    {
        var wonSets = new PointResult(0,0);
        foreach (var set in setList)
        {
            set.CalculateSetPoints(setRule);
            set.IsTieBreak = false || matchRule.BestOf && wonSets.Home == wonSets.Guest &&
                wonSets.Home + wonSets.Guest == matchRule.MaxNumOfSets() - 1;
            wonSets.Home += set.HomeBallPoints > set.GuestBallPoints ? 1 : 0;
            wonSets.Guest += set.HomeBallPoints < set.GuestBallPoints ? 0 : 1;
        }

        return setList;
    }

    /// <summary>
    /// Gets the sum of set points for home and guest team.
    /// </summary>
    /// <param name="setList"></param>
    /// <returns>Returns an <see cref="IOpponent{T}"/> with the sum of set points for home and guest team.</returns>
    public static IOpponent<int> GetSetPoints(this IList<SetEntity> setList)
    {
        return new PointResult(setList.Sum(s => s.HomeSetPoints), setList.Sum(s => s.GuestSetPoints));
    }

    /// <summary>
    /// Gets the total ball points of all sets.
    /// </summary>
    /// <param name="setList"></param>
    /// <returns>Returns the total ball points of all sets.</returns>
    public static int GetTotalBallPoints(this IList<SetEntity> setList)
    {
        return setList.Sum(s => s.HomeBallPoints) + setList.Sum(s => s.GuestBallPoints);
    }
}