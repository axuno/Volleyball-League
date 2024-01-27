﻿using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Ranking;

namespace TournamentManager.Tests.Ranking;

[TestFixture]
public class LegacyRankComparisonTests
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
            new List<MatchToPlayRawRow>(), RankComparison.LegacyRankComparison);

        var rl= ranking.GetList(out var updatedOn);

        Assert.That(rl, Has.Count.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(rl[0].Number, Is.EqualTo(1));
            Assert.That(rl[1].Number, Is.EqualTo(2));
            // Note: Even the direct comparison between teams is the same, so the team with the higher team ID is returned.
            Assert.That(rl[0].TeamId, Is.EqualTo(2));
        });
    }

    [Test]
    public void MatchPointsDifferenceDecidesTest()
    {
        var comparer = new RankComparer(RankComparison.LegacyRankComparison);
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
        ranks = new List<Rank> { ranks[1], ranks[0] };
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        Assert.Multiple(() =>
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        });
    }

    [Test]
    public void SetPointsDifferenceDecidesTest()
    {
        var comparer = new RankComparer(RankComparison.LegacyRankComparison);
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
        ranks = new List<Rank> { ranks[0], ranks[1] };
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        Assert.Multiple(() =>
        {
            Assert.That(teamId1, Is.EqualTo(2));
            Assert.That(teamId2, Is.EqualTo(2));
        });
    }

    [Test]
    public void BallPointsDifferenceDecidesTest()
    {
        var comparer = new RankComparer(RankComparison.LegacyRankComparison);
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
        ranks = new List<Rank> { ranks[0], ranks[1] };
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        Assert.Multiple(() =>
        {
            Assert.That(teamId1, Is.EqualTo(2));
            Assert.That(teamId2, Is.EqualTo(2));
        });
    }

    [Test]
    public void BallPointsWonDecideTest()
    {
        var comparer = new RankComparer(RankComparison.LegacyRankComparison);
        var ranks = new List<Rank> {
            new() {
                TeamId = 1,
                MatchPoints = { Home = 3, Guest = 0 },
                MatchesWon = { Home = 1, Guest = 0 },
                SetPoints = { Home = 3, Guest = 0 },
                SetsWon = { Home = 3, Guest = 0 },
                BallPoints = { Home = 76, Guest = 0 }, // one more ball point
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
        ranks = new List<Rank> { ranks[1], ranks[0] };
        ranks.Sort(comparer);
        var teamId2 = ranks[0].TeamId;

        Assert.Multiple(() =>
        {
            Assert.That(teamId1, Is.EqualTo(1));
            Assert.That(teamId2, Is.EqualTo(1));
        });
    }
}
