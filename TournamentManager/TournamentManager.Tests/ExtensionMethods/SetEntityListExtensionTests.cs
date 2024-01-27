using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class SetEntityListExtensionTests
{
    [TestCase("25:1 25:1 25:1", 9, 3)]
    [TestCase("1:25 1:25 1:25", 3, 9)]
    [TestCase("25:25 25:25 25:25",6, 6)]
    public void Calculate_Set_Points_NoTieBreak(string setResults, int expectedHomeSetPts, int expectedGuestSetPts)
    {
        var sets = new List<SetEntity> { { -1, setResults } };
        
        var setRule = new SetRuleEntity { PointsSetWon = 3, PointsSetLost = 1, PointsSetTie = 2 };
        var matchRule = new MatchRuleEntity { BestOf = false, NumOfSets = 3 };
        sets.CalculateSetPoints(setRule, matchRule);
        Assert.Multiple(() =>
        {
            Assert.That(expectedHomeSetPts, Is.EqualTo(sets.GetSetPoints().Home));
            Assert.That(expectedGuestSetPts, Is.EqualTo(sets.GetSetPoints().Guest));
        });
    }

    [TestCase("25:1 1:25 25:1", 7, 5, true)]
    [TestCase("25:1 25:1 25:1", 9, 3, false)]
    [TestCase("25:25 25:25 25:25", 0, 0, false)]
    public void Calculate_Set_Points_TieBreak(string setResults, int expectedHomeSetPts, int expectedGuestSetPts, bool expectedTieBreak)
    {
        var sets = new List<SetEntity> { { -1, setResults } };

        var setRule = new SetRuleEntity { PointsSetWon = 3, PointsSetLost = 1, PointsSetTie = 0 };
        var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };
        sets.CalculateSetPoints(setRule, matchRule);
        Assert.Multiple(() =>
        {
            Assert.That(expectedHomeSetPts, Is.EqualTo(sets.GetSetPoints().Home));
            Assert.That(expectedGuestSetPts, Is.EqualTo(sets.GetSetPoints().Guest));
            Assert.That(expectedTieBreak, Is.EqualTo(sets.Last().IsTieBreak));
        });
    }

    [Test]
    public void Sequence_Of_Sets_Is_As_Played()
    {
        var sets = new List<SetEntity> { { -1, "25:1 25:2 25:3" } };

        Assert.That(sets, Has.Count.EqualTo(3));
        Assert.That(sets.All(s => s.SequenceNo == s.GuestBallPoints));
    }

    [Test]
    public void All_Sets_Contain_MatchId()
    {
        var sets = new List<SetEntity> { { 12345, "25:1 25:2 25:3" } };

        Assert.That(sets, Has.Count.EqualTo(3));
        Assert.That(sets.All(s => s.MatchId == 12345));
    }

    [TestCase(25, 1, 5, 4)]
    [TestCase(25, 1, 2, 0)]
    [TestCase(1, 25, 1, 2)]
    public void Overrule_Set_Points(int homeBallPts, int guestBallPts, int homeSetPts, int guestSetPts)
    {
        var set = new SetEntity {IsTieBreak = true} ;
        set.Overrule(homeBallPts, guestBallPts, homeSetPts, guestSetPts);
        Assert.Multiple(() =>
        {
            Assert.That(homeSetPts, Is.EqualTo(set.HomeSetPoints));
            Assert.That(guestSetPts, Is.EqualTo(set.GuestSetPoints));
            Assert.That(set.IsTieBreak, Is.False); // reset by overrule
            Assert.That(set.IsOverruled, Is.True); // set by overrule
        });
    }

    [TestCase("25:1 25:1 25:1", 3, 0)]
    [TestCase("1:25 1:25 1:25", 0, 3)]
    [TestCase("25:25 25:25 25:25", 0, 0)]
    public void Get_Sets_Won(string setResults, int expectedWonHome, int expectedWonGuest)
    {
        var sets = new List<SetEntity> { { -1, setResults } };
        Assert.Multiple(() =>
        {
            Assert.That(expectedWonHome, Is.EqualTo(sets.GetSetsWon().Home));
            Assert.That(expectedWonGuest, Is.EqualTo(sets.GetSetsWon().Guest));
            Assert.That(expectedWonHome > expectedWonGuest ? expectedWonHome : expectedWonGuest, Is.EqualTo(sets.MaxBestOf()));
        });
    }

    [TestCase("0:0 0:0 0:0", 0)]
    [TestCase("25:1 25:1 25:1", 78)]
    public void Get_Sets_Total_Ball_Points(string setResults, int expected)
    {
        var sets = new List<SetEntity> { { -1, setResults } };

        Assert.That(expected, Is.EqualTo(sets.GetTotalBallPoints()));
    }
}
