using System;
using System.Collections.Generic;
using System.Linq;
using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="MatchEntity"/> extensions, which can't be located in generic CustomExtensions
/// because of dependencies to classes of <see cref="TournamentManager"/>.
/// </summary>
public static class MatchEntityExtensions
{
    public static MatchEntity CalculateMatchPoints(this MatchEntity match, MatchRuleEntity matchRule)
    {
        if (!match.Sets.Any()) return match;

        match.IsOverruled = match.Sets.Any(s => s.IsOverruled);

        var setPoints = match.Sets.GetSetPoints();

        if (setPoints.Home < setPoints.Guest)
        {
            match.HomePoints = matchRule.PointsMatchLost;
            match.GuestPoints = matchRule.PointsMatchWon;
        }
        else if (setPoints.Home > setPoints.Guest)
        {
            match.HomePoints = matchRule.PointsMatchWon;
            match.GuestPoints = matchRule.PointsMatchLost;
        }
        else
        {
            match.HomePoints = match.GuestPoints = matchRule.PointsMatchTie;
        }

        match.IsComplete = true;

        return match;
    }
}