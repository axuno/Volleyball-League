using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Ranking;

namespace TournamentManager.Tests.Ranking;

[TestFixture]
public class HroThreeWinningSetsRankComparisonTests
{
    [Test]
    public void Two_Teams_Are_Fully_Equal_With_Match_Results()
    {
        var matchId = 1;
        var matches = new List<MatchEntity> {
            GetMatch(matchId++, 1, 2, "25:0 25:0 25:0"),
            GetMatch(matchId, 2, 1, "25:0 25:0 25:0")
        };

        var ranking = new TournamentManager.Ranking.Ranking(CreateMatchCompleteRows(matches),
            new List<MatchToPlayRawRow>(), RankComparison.HroThreeWinningSetsRankComparison);

        var rl= ranking.GetList(out var updatedOn);

        Assert.That(rl.Count, Is.EqualTo(2));
        Assert.That(rl[0].Number, Is.EqualTo(1));
        Assert.That(rl[1].Number, Is.EqualTo(2));
        // Note: Even the direct comparison between teams is the same, so the team with the higher team ID is returned.
        Assert.That(rl[0].TeamId, Is.EqualTo(2));
    }

    //[TestCase("25:0 25:0 25:0", "0:25 0:25 0:25", 1, 2)]
    [TestCase("0:25 0:25 0:25", "25:0 25:0 25:0", 2, 1)]
    public void Ranking_Based_On_MatchPoints_Won(string xResult, string yResult, long expected1,long expected2)
    {
        var matchId = 1;
        var matches = new List<MatchEntity> {
            GetMatch(matchId++, 1, 2, xResult),
            GetMatch(matchId, 2, 1, yResult)
        };

        var ranking = new TournamentManager.Ranking.Ranking(CreateMatchCompleteRows(matches),
            new List<MatchToPlayRawRow>(), RankComparison.HroThreeWinningSetsRankComparison);

        var rl= ranking.GetList(out var updatedOn);

        Assert.That(rl.Count, Is.EqualTo(2));
        Assert.That(rl[0].TeamId, Is.EqualTo(expected1));
        Assert.That(rl[1].TeamId, Is.EqualTo(expected2));
    }


    private IList<MatchCompleteRawRow> CreateMatchCompleteRows(List<MatchEntity> matches)
    {
        var completeMatches = new List<MatchCompleteRawRow>();
        foreach (var match in matches)
        {
            completeMatches.Add(new MatchCompleteRawRow {
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

    private MatchEntity GetMatch(long matchId, long home, long guest, string setResults)
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

    /*
    [Test]
    public void Calc_With_No_Sets_Should_Not_Throw()
    {
        var matchRule = GetMatchRule_NoTieBreakRule();
        var setRule = GetSetRule();
        var match = new MatchEntity(1234);

        Assert.That(del: () => _ = match.Sets.CalculateSetPoints(setRule, matchRule), Throws.Nothing);
    }

    [TestCase("25:1 25:2 25:3", 3, 0)]
    [TestCase("25:1 25:2 1:25 1:25 15:1", 3, 0)]
    [TestCase("1:25 2:25 2:25", 0, 3)]
    [TestCase("1:25 2:25 25:13 25:1 1:15", 0, 3)]
    [TestCase("1:25 25:1", 1, 1)]
    public void Calc_MatchPoints_No_TieBreakRule(string setResults, int expectedHome, int expectedGuest)
    {
        // Note: Test cases are not validated against the rules here, but they are valid.
        //       Validation is tested in ModelValidator tests

        var matchRule = GetMatchRule_NoTieBreakRule();
        var setRule = GetSetRule();
        var match = new MatchEntity(1234);
        match.Sets.Add(match.Id, setResults);
        _ = match.Sets.CalculateSetPoints(setRule, matchRule);
        _ = match.CalculateMatchPoints(matchRule);

        var pointResult = new PointResult(match.HomePoints, match.GuestPoints);
        var expectedResult = new PointResult(expectedHome, expectedGuest);

        pointResult.Should().BeEquivalentTo(expectedResult);
    }

    [TestCase("25:1 25:2 25:3", 3, 0)]
    [TestCase("25:1 25:2 1:25 1:25 15:1", 2, 1)]
    [TestCase("1:25 2:25 2:25", 0, 3)]
    [TestCase("1:25 2:25 25:13 25:1 1:15", 1, 2)]
    [TestCase("1:25 25:1", 1, 1)]
    public void Calc_MatchPoints_With_TieBreakRule(string setResults, int expectedHome, int expectedGuest)
    {
        // Note: Test cases are not validated against the rules here, but they are valid.
        //       Validation is tested in ModelValidator tests

        var matchRule = GetMatchRule_TieBreakRule();
        var setRule = GetSetRule();
        var match = new MatchEntity(1234);
        match.Sets.Add(match.Id, setResults);
        _ = match.Sets.CalculateSetPoints(setRule, matchRule);
        _ = match.CalculateMatchPoints(matchRule);

        var pointResult = new PointResult(match.HomePoints, match.GuestPoints);
        var expectedResult = new PointResult(expectedHome, expectedGuest);

        pointResult.Should().BeEquivalentTo(expectedResult);
    }

    [Test]
    public void Calc_MatchPoints_With_TieBreakRule_Throws()
    {
        var matchRule = GetMatchRule_TieBreakRule();
        var setRule = GetSetRule();
        var match = new MatchEntity(1234);
        match.Sets.Add(match.Id, "25:1 25:2 1:25 1:25 15:1");
        _ = match.Sets.CalculateSetPoints(setRule, matchRule);

        // This will trigger an exception, because the set points are tie
        var lastSet = match.Sets.Last();
        lastSet.HomeSetPoints = lastSet.GuestSetPoints = 0;

        Assert.That(del: () => _ = match.CalculateMatchPoints(matchRule), Throws.InvalidOperationException);
    }

    [Test]
    public void CreateMatches()
    {
        // SELECT * FROM [Match] WHERE RoundId=1 and LegSequenceNo=1 FOR JSON AUTO
        // Round-robin:
        // Each team must match 5 opponents, thus there are 5 rounds.
        // There are 3 matches per round (6 teams divided by 2).
        // This makes 15 matches (5 rounds x 3 matches per round).
        var matchesJson =
            """
            [
                {
                    "Id": 1,
                    "HomeTeamId": 1,
                    "GuestTeamId": 12,
                    "RefereeId": 1,
                    "RoundId": 1,
                    "VenueId": 1,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-09-27T18:00:00",
                    "PlannedEnd": "2023-09-27T20:00:00",
                    "OrigPlannedStart": "2023-09-20T18:00:00",
                    "OrigPlannedEnd": "2023-09-20T20:00:00",
                    "Remarks": "",
                    "ChangeSerial": 1,
                    "CreatedOn": "2023-09-01T22:03:26.830",
                    "ModifiedOn": "2023-09-02T05:22:21.063"
                },
                {
                    "Id": 2,
                    "HomeTeamId": 14,
                    "GuestTeamId": 17,
                    "RefereeId": 14,
                    "RoundId": 1,
                    "VenueId": 5,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-09-19T18:00:00",
                    "PlannedEnd": "2023-09-19T20:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:26.913",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 3,
                    "HomeTeamId": 22,
                    "GuestTeamId": 25,
                    "RefereeId": 22,
                    "RoundId": 1,
                    "VenueId": 18,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-09-19T18:00:00",
                    "PlannedEnd": "2023-09-19T20:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:26.990",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 4,
                    "HomeTeamId": 1,
                    "GuestTeamId": 14,
                    "RefereeId": 1,
                    "RoundId": 1,
                    "VenueId": 1,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-10-18T18:00:00",
                    "PlannedEnd": "2023-10-18T20:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.053",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 5,
                    "HomeTeamId": 12,
                    "GuestTeamId": 17,
                    "RefereeId": 12,
                    "RoundId": 1,
                    "VenueId": 11,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-10-17T18:00:00",
                    "PlannedEnd": "2023-10-17T20:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.147",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 6,
                    "HomeTeamId": 22,
                    "GuestTeamId": 1,
                    "RefereeId": 22,
                    "RoundId": 1,
                    "VenueId": 18,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-11-07T19:00:00",
                    "PlannedEnd": "2023-11-07T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.240",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 7,
                    "HomeTeamId": 25,
                    "GuestTeamId": 12,
                    "RefereeId": 25,
                    "RoundId": 1,
                    "VenueId": 2,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-11-10T19:00:00",
                    "PlannedEnd": "2023-11-10T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.333",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 8,
                    "HomeTeamId": 14,
                    "GuestTeamId": 22,
                    "RefereeId": 14,
                    "RoundId": 1,
                    "VenueId": 5,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-11-14T19:00:00",
                    "PlannedEnd": "2023-11-14T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.397",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 9,
                    "HomeTeamId": 17,
                    "GuestTeamId": 25,
                    "RefereeId": 17,
                    "RoundId": 1,
                    "VenueId": 14,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-11-15T18:30:00",
                    "PlannedEnd": "2023-11-15T20:30:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.477",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 10,
                    "HomeTeamId": 1,
                    "GuestTeamId": 17,
                    "RefereeId": 1,
                    "RoundId": 1,
                    "VenueId": 1,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-12-06T19:00:00",
                    "PlannedEnd": "2023-12-06T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:28.577",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 11,
                    "HomeTeamId": 12,
                    "GuestTeamId": 14,
                    "RefereeId": 12,
                    "RoundId": 1,
                    "VenueId": 11,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-12-05T19:00:00",
                    "PlannedEnd": "2023-12-05T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.617",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 12,
                    "HomeTeamId": 22,
                    "GuestTeamId": 12,
                    "RefereeId": 22,
                    "RoundId": 1,
                    "VenueId": 18,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-12-19T19:00:00",
                    "PlannedEnd": "2023-12-19T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.680",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 13,
                    "HomeTeamId": 25,
                    "GuestTeamId": 1,
                    "RefereeId": 25,
                    "RoundId": 1,
                    "VenueId": 2,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2023-12-15T19:00:00",
                    "PlannedEnd": "2023-12-15T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.743",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 14,
                    "HomeTeamId": 14,
                    "GuestTeamId": 25,
                    "RefereeId": 14,
                    "RoundId": 1,
                    "VenueId": 5,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2024-01-16T19:00:00",
                    "PlannedEnd": "2024-01-16T21:00:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.820",
                    "ModifiedOn": "2023-09-01T14:30:00"
                },
                {
                    "Id": 15,
                    "HomeTeamId": 17,
                    "GuestTeamId": 22,
                    "RefereeId": 17,
                    "RoundId": 1,
                    "VenueId": 14,
                    "LegSequenceNo": 1,
                    "IsComplete": false,
                    "IsOverruled": false,
                    "PlannedStart": "2024-01-17T18:30:00",
                    "PlannedEnd": "2024-01-17T20:30:00",
                    "Remarks": "",
                    "ChangeSerial": 0,
                    "CreatedOn": "2023-09-01T22:03:27.887",
                    "ModifiedOn": "2023-09-01T14:30:00"
                }
            ]
            
            """;

        var matches = JsonSerializer.Deserialize<List<MatchEntity>>(matchesJson)!;
        matches[0].Sets.Add(matches[0].Id, "25:1 25:2 1:25 1:25 15:1");
        //return new List<MatchCompleteRawRow>();
    }
    */
    private static MatchRuleEntity GetMatchRule_NoTieBreakRule()
    {
        return new MatchRuleEntity {
            BestOf = true,
            NumOfSets = 3,
            PointsMatchWon = 3,
            PointsMatchLost = 0,
            PointsMatchTie = 1,
            RankComparer = (int) RankComparison.HroThreeWinningSetsRankComparison
        };
    }

    private static MatchRuleEntity GetMatchRule_TieBreakRule()
    {
        return new MatchRuleEntity {
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

    private static SetRuleEntity GetSetRule()
    {
        return new SetRuleEntity {
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
