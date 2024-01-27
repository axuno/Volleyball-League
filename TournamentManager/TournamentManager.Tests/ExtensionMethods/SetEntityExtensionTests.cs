using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class SetEntityExtensionTests
{
    [TestCase(25, 1, 3, 1)]
    [TestCase(1, 25, 1, 3)]
    [TestCase(25, 25, 2, 2)]
    public void Calculate_Set_Points(int homeBallPts, int guestBallPts, int expectedHomeSetPts, int expectedGuestSetPts)
    {
        var set = new SetEntity { HomeBallPoints = homeBallPts, GuestBallPoints = guestBallPts };
        var setRule = new SetRuleEntity { PointsSetWon = 3, PointsSetLost = 1, PointsSetTie = 2 };
        set.CalculateSetPoints(setRule);
        Assert.Multiple(() =>
        {
            Assert.That(set.HomeSetPoints, Is.EqualTo(expectedHomeSetPts));
            Assert.That(set.GuestSetPoints, Is.EqualTo(expectedGuestSetPts));
        });
    }

    [TestCase(25, 1, 5, 4)]
    [TestCase(25, 1, 2, 0)]
    [TestCase(1, 25, 1, 2)]
    public void Overrule_Set_Points(int homeBallPts, int guestBallPts, int homeSetPts, int guestSetPts)
    {
        var set = new SetEntity { HomeBallPoints = homeBallPts, GuestBallPoints = guestBallPts, IsTieBreak = true, IsOverruled = false };
        set.Overrule(homeBallPts, guestBallPts, homeSetPts, guestSetPts);
        Assert.Multiple(() =>
        {
            Assert.That(set.HomeSetPoints, Is.EqualTo(homeSetPts));
            Assert.That(set.GuestSetPoints, Is.EqualTo(guestSetPts));
            Assert.That(set.IsTieBreak, Is.False);
            Assert.That(set.IsOverruled, Is.True);
        });
    }
}
