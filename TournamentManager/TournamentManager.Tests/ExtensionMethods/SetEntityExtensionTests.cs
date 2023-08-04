using System;
using System.Collections.Generic;
using System.Text;
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
            Assert.AreEqual(expectedHomeSetPts, set.HomeSetPoints);
            Assert.AreEqual(expectedGuestSetPts, set.GuestSetPoints);
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
            Assert.AreEqual(homeSetPts, set.HomeSetPoints);
            Assert.AreEqual(guestSetPts, set.GuestSetPoints);
            Assert.IsFalse(set.IsTieBreak);
            Assert.IsTrue(set.IsOverruled);
        });
    }
}
