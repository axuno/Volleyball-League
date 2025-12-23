using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
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
            RankingTestUtilities.GetMatch(matchId++, 1, 2, "25:0 25:0 25:0"),
            RankingTestUtilities.GetMatch(matchId, 2, 1, "25:0 25:0 25:0")
        };

        var ranking = new TournamentManager.Ranking.Ranking(RankingTestUtilities.CreateMatchCompleteRows(matches),
            new List<MatchToPlayRawRow>(), RankComparison.HroThreeWinningSetsRankComparison);
        var rl = ranking.GetList(out _);

        Assert.That(rl, Has.Count.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(rl[0].Number, Is.EqualTo(1));
            Assert.That(rl[1].Number, Is.EqualTo(2));
            // Note: Even the direct comparison between teams is the same, so the team with the higher team ID is returned.
            Assert.That(rl[0].TeamId, Is.EqualTo(2));
        }
    }

    [Test]
    public void MatchPointsWonDecideTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 }, // one match won
                MatchesWon = { Home = 1, Guest = 0 }, 
                SetPoints = { Home = 3, Guest = 1 },
                SetsWon = { Home = 3, Guest = 1 },
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 0, Guest = 3 }, // no match won
                MatchesWon = { Home = 0, Guest = 1 },
                SetPoints = { Home = 0, Guest = 3 },
                SetsWon = { Home = 0, Guest = 3 },
                BallPoints = { Home = 0, Guest = 25 },
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[1], ranks[0]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        }
    }

    [Test]
    public void MatchesWonDecideTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 0, Guest = 0 }, 
                MatchesWon = { Home = 1, Guest = 0 }, // one match won
                SetPoints = { Home = 3, Guest = 1 },
                SetsWon = { Home = 3, Guest = 1 },
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 0, Guest = 0 }, 
                MatchesWon = { Home = 0, Guest = 0 },// no match won
                SetPoints = { Home = 0, Guest = 3 },
                SetsWon = { Home = 0, Guest = 3 },
                BallPoints = { Home = 0, Guest = 25 },
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[0], ranks[1]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        }
    }

    [Test]
    public void SetPointsDifferenceDecidesTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 1 }, // one set lost
                SetsWon = { Home = 3, Guest = 1 },
                BallPoints = { Home = 50, Guest = 25 },
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 }, // no set lost
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[0], ranks[1]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(2));
            Assert.That(teamId2, Is.EqualTo(2));
        }
    }

    [Test]
    public void SetsWonDecideTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 4, Guest = 0 }, // 4 sets won
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 }, // 3 sets won
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[1], ranks[0]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        }
    }

    [Test]
    public void BallPointsDifferenceDecidesTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 75, Guest = 1 }, // one ball point lost
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 75, Guest = 0 }, // no ball point lost
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[0], ranks[1]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(2));
            Assert.That(teamId2, Is.EqualTo(2));
        }
    }

    [Test]
    public void BallPointsWonDecideTest()
    {
        var comparer = new RankComparer(RankComparison.HroThreeWinningSetsRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 76, Guest = 1 }, // same ball point difference, but 1 more won
                MatchesPlayed = 1
            },
            new() {
                TeamId = 2,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 75, Guest = 0 },
                MatchesPlayed = 1
            }
        };

        ranks.Sort(comparer);
        var teamId1 = ranks[0].TeamId;
        // swap x and y of sorted list for the comparer
        ranks = [ranks[1], ranks[0]];
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        }
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

    
}
