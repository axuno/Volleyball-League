using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.ExtensionMethods;

/// <summary>
/// <see cref="MatchEntity"/> extension methods.
/// </summary>
public static class MatchEntityExtensions
{
    /// <summary>
    /// Calculates the match points following the defined <see cref="MatchRuleEntity"/> rules
    /// and stores them in the <see cref="MatchEntity"/>.
    /// </summary>
    /// <param name="match"></param>
    /// <param name="matchRule"></param>
    /// <returns></returns>
    public static MatchEntity CalculateMatchPoints(this MatchEntity match, MatchRuleEntity matchRule)
    {
        if (!match.Sets.Any()) return match;

        var hasTieBreak = match.Sets.Any(s => s.IsTieBreak);
        match.IsOverruled = match.Sets.Any(s => s.IsOverruled);
        var setPoints = match.Sets.GetSetPoints();

        // Only check PointsMatchWonAfterTieBreak for not zero,
        // because for PointsMatchLostAfterTieBreak it might be the desired value
        if (hasTieBreak && matchRule.PointsMatchWonAfterTieBreak != 0)
        {
            // Special match point distribution for tie-breaks
            TieBreakRule(match, matchRule, setPoints);
        }
        else
        {
            // Regular match point distribution
            NoTieBreakRule(match, matchRule, setPoints);
        }

        match.IsComplete = true;
        return match;
    }

    /// <summary>
    /// This method gets called, when the <see cref="MatchRuleEntity.PointsMatchWonAfterTieBreak"/>
    /// and the <see cref="MatchRuleEntity.PointsMatchLostAfterTieBreak"/> are zero,
    /// or the match was won without a tie-break.
    /// </summary>
    /// <exception cref="InvalidOperationException">Throws if set result is tie.</exception>
    private static void NoTieBreakRule(MatchEntity match, MatchRuleEntity matchRule, IOpponent<int?> setPoints)
    {
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
    }

    /// <summary>
    /// Usually, there are 3 match points to distribute, if the tie-break rule applies:
    /// a) If a team wins without a tie-break, the match points are 3:0
    /// b) If a team wins after a tie-break, the match points are 2:1
    /// </summary>
    private static void TieBreakRule(MatchEntity match, MatchRuleEntity matchRule, IOpponent<int?> setPoints)
    {
        if (setPoints.Home < setPoints.Guest)
        {
            match.HomePoints = matchRule.PointsMatchLostAfterTieBreak;
            match.GuestPoints = matchRule.PointsMatchWonAfterTieBreak;
        }
        else if (setPoints.Home > setPoints.Guest)
        {
            match.HomePoints = matchRule.PointsMatchWonAfterTieBreak;
            match.GuestPoints = matchRule.PointsMatchLostAfterTieBreak;
        }
        else
        {
            throw new InvalidOperationException(
                $"Set points '{setPoints.Home}:{setPoints.Guest}' is a tie result, which is not allowed using {nameof(TieBreakRule)}.");
        }
    }
}
