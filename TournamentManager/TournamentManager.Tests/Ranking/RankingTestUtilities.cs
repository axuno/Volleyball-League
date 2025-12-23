using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.Ranking;

namespace TournamentManager.Tests.Ranking;
internal class RankingTestUtilities
{
    public static MatchEntity GetMatch(long matchId, long home, long guest, string setResults)
    {
        var matchRule = GetMatchRule_TieBreakRule();
        var setRule = GetSetRule();
        var dateTime = DateTime.UtcNow.Date.AddHours(-4);
        var match = new MatchEntity(matchId) {
            HomeTeamId = home,
            GuestTeamId = guest,
            RealStart = dateTime,
            RealEnd = dateTime.AddHours(2),
            CreatedOn = dateTime,
            ModifiedOn = dateTime
        };
        match.Sets.Add(match.Id, setResults);
        _ = match.Sets.CalculateSetPoints(setRule, matchRule);
        _ = match.CalculateMatchPoints(matchRule);

        return match;
    }

    public static IList<MatchCompleteRawRow> CreateMatchCompleteRows(List<MatchEntity> matches)
    {
        var completeMatches = new List<MatchCompleteRawRow>();
        foreach (var match in matches)
        {
            completeMatches.Add(new()
            {
                Id = match.Id,
                HomeTeamId = match.HomeTeamId,
                GuestTeamId = match.GuestTeamId,
                HomeMatchPoints = match.HomePoints,
                GuestMatchPoints = match.GuestPoints,
                HomeSetPoints = match.Sets.Sum(s => s.HomeSetPoints),
                GuestSetPoints = match.Sets.Sum(s => s.GuestSetPoints),
                HomeBallPoints = match.Sets.Sum(s => s.HomeBallPoints),
                GuestBallPoints= match.Sets.Sum(s => s.GuestBallPoints),
                MatchDate = match.RealStart
            });
        }

        return completeMatches;
    }

    public static MatchRuleEntity GetMatchRule_NoTieBreakRule()
    {
        return new()
        {
            BestOf = true,
            NumOfSets = 3,
            PointsMatchWon = 3,
            PointsMatchLost = 0,
            PointsMatchTie = 1,
            RankComparer = (int) RankComparison.HroThreeWinningSetsRankComparison
        };
    }

    public static MatchRuleEntity GetMatchRule_TieBreakRule()
    {
        return new()
        {
            BestOf = true,
            NumOfSets = 3,
            PointsMatchWon = 3,
            PointsMatchLost = 0,
            PointsMatchWonAfterTieBreak = 2,
            PointsMatchLostAfterTieBreak = 1,
            PointsMatchTie = 1,
            RankComparer = (int) RankComparison.HroThreeWinningSetsRankComparison
        };
    }

    public static SetRuleEntity GetSetRule()
    {
        return new()
        {
            NumOfPointsToWinRegular = 25,
            NumOfPointsToWinTiebreak = 15,
            PointsDiffToWinRegular = 2,
            PointsDiffToWinTiebreak = 2,
            PointsSetWon = 1,
            PointsSetLost = 0,
            PointsSetTie = 0
        };
    }
}
