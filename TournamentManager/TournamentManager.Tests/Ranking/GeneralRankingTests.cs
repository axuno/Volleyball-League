using FluentAssertions;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.Ranking;

namespace TournamentManager.Tests.Ranking;

[TestFixture]
internal class GeneralRankingTests
{
    [TestCase(RankComparison.LegacyRankComparison)]
    [TestCase(RankComparison.TwoWinningSetsRankComparison)]
    [TestCase(RankComparison.ThreeWinningSetsRankComparison)]
    [TestCase(RankComparison.HroThreeWinningSetsRankComparison)]
    public void CreateRankingWithRankComparisonTests(RankComparison comparison)
    {
        var ranking = new TournamentManager.Ranking.Ranking(new List<MatchCompleteRawRow>(),
            new List<MatchToPlayRawRow>(), comparison);

        Assert.That(ranking.RankComparer.RankComparison, Is.EqualTo(comparison));
        Assert.That(ranking.RankComparer.Description.Length, Is.AtLeast(1));
    }

    [Test]
    public void CreateRankingWithUnknownRankComparisonShouldThrow()
    {
        Assert.That(() =>
        {
            var ranking = new TournamentManager.Ranking.Ranking(new List<MatchCompleteRawRow>(),
                new List<MatchToPlayRawRow>(), (RankComparison) int.MaxValue);

        }, Throws.Exception.TypeOf<ArgumentOutOfRangeException>());


    }

    [Test]
    public void RankPointsCalculationTests()
    {
        var matchId = 1;
        var matches = new List<MatchEntity> {
            RankingTestUtilities.GetMatch(matchId++, 2, 1, "25:1 25:2 25:3"),
            RankingTestUtilities.GetMatch(matchId, 1, 2, "1:25 2:25 25:3 25:4 15:14")
        };

        var ranking = new TournamentManager.Ranking.Ranking(RankingTestUtilities.CreateMatchCompleteRows(matches),
            new List<MatchToPlayRawRow> { new() { HomeTeamId = 3, GuestTeamId = 4 } },
            RankComparison.ThreeWinningSetsRankComparison);

        var ranks = ranking.GetList(out var lastUpdateOn);
        var r1 = ranks[0];

        Assert.Multiple(() =>
        {
            Assert.That(lastUpdateOn, Is.EqualTo(DateTime.MinValue));
            Assert.That(ranks.LastUpdatedOn, Is.EqualTo(lastUpdateOn));
            Assert.That(r1.MatchesPlayed, Is.EqualTo(2));
            Assert.That(r1.MatchesToPlay, Is.EqualTo(0));
            Assert.That(ranks[3].MatchesToPlay, Is.EqualTo(1));

            Assert.That(ranks.Count, Is.EqualTo(4)); // 2 played for Team Ids 1 and 2, 2 to play for Team Ids 3 and 4
            Assert.That(r1.TeamId, Is.EqualTo(2));
            Assert.That(r1.ToString(), Is.EqualTo("1"));
            
            r1.MatchesWon.Should().BeEquivalentTo(new PointResult(1, 1));
            r1.MatchPoints.Should().BeEquivalentTo(new PointResult(4, 2));
            r1.SetsWon.Should().BeEquivalentTo(new PointResult(5, 3));
            r1.SetPoints.Should().BeEquivalentTo(new PointResult(5, 3));
            r1.BallPoints.Should().BeEquivalentTo(new PointResult(146, 74));
        });
    }

    [Test]
    public void GetMatchDaysTest()
    {
        var matchId = 1;
        var completedMatches = RankingTestUtilities.CreateMatchCompleteRows(new List<MatchEntity> {
            RankingTestUtilities.GetMatch(matchId++, 2, 1, "25:1 25:2 25:3"),
            RankingTestUtilities.GetMatch(matchId, 1, 2, "25:1 25:2 25:3")
        });
        completedMatches[0].MatchDate = new DateTime(2024, 7, 1);
        completedMatches[1].MatchDate = new DateTime(2024, 7, 2);

        var ranking = new TournamentManager.Ranking.Ranking(completedMatches,
            new List<MatchToPlayRawRow>(), RankComparison.ThreeWinningSetsRankComparison);

        var matchDays = ranking.GetMatchDays();

        Assert.That(matchDays.Count, Is.EqualTo(2));
    }

    [Test]
    public void GetRankingHistoryTest()
    {
        var matchId = 1;
        var completedMatches = RankingTestUtilities.CreateMatchCompleteRows(new List<MatchEntity> {
            RankingTestUtilities.GetMatch(matchId++, 2, 1, "25:1 25:2 25:3"),
            RankingTestUtilities.GetMatch(matchId, 1, 2, "25:3 25:4 25:5")
        });
        completedMatches[0].MatchDate = new DateTime(2024, 7, 1);
        completedMatches[1].MatchDate = new DateTime(2024, 7, 2);

        var ranking = new TournamentManager.Ranking.Ranking(completedMatches,
            new List<MatchToPlayRawRow>(), RankComparison.ThreeWinningSetsRankComparison);

        var history = ranking.GetRankingHistory();
        history.ReCalculate();
        history = ranking.GetRankingHistory();
        var chart = new RankingChart(ranking, new List<(long TeamId, string TeamName)> { (1, "1"), (2, "2") },
            new RankingChart.ChartSettings()) { UseMatchDayMarker = true, ShowUpperDateLimit = true };

        Assert.That(history.GetMatchDays().Count, Is.EqualTo(2));
        Assert.That(history.GetByTeam(1).Count, Is.EqualTo(2));
        Assert.That(history.GetByMatchDay().Count, Is.EqualTo(2));
        Assert.That(() => { chart.GetSvg(); chart.GetPng(); }, Throws.Nothing);
    }
}
