using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using FluentAssertions;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class MatchEntityExtensionTests
{
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

    private static MatchRuleEntity GetMatchRule_NoTieBreakRule()
    {
        return new MatchRuleEntity {
            BestOf = true,
            NumOfSets = 3,
            PointsMatchWon = 3,
            PointsMatchLost = 0,
            PointsMatchTie = 1
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
            PointsMatchTie = 1
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
