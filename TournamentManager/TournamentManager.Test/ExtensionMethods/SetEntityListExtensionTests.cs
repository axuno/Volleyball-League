using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NuGet.Frameworks;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.Tests.ExtensionMethods
{
    [TestFixture]
    public class SetEntityListExtensionTests
    {
        [TestCase(25, 1, 25, 1, 25, 1, 9, 3)]
        [TestCase(1, 25, 1, 25, 1, 25, 3, 9)]
        [TestCase(25, 25, 25, 25, 25, 25, 6, 6)]
        public void Calculate_Set_Points_NoTieBreak(int s1HomeBall, int s1GuestBall, int s2HomeBall, int s2GuestBall, int s3HomeBall, int s3GuestBall, int expectedHomeSetPts, int expectedGuestSetPts)
        {
            var sets = new List<SetEntity>
            {
                new() {HomeBallPoints = s1HomeBall, GuestBallPoints = s1GuestBall},
                new() {HomeBallPoints = s2HomeBall, GuestBallPoints = s2GuestBall},
                new() {HomeBallPoints = s3HomeBall, GuestBallPoints = s3GuestBall}
            };
            var setRule = new SetRuleEntity { PointsSetWon = 3, PointsSetLost = 1, PointsSetTie = 2 };
            var matchRule = new MatchRuleEntity { BestOf = false, NumOfSets = 3 };
            sets.CalculateSetPoints(setRule, matchRule);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedHomeSetPts, sets.GetSetPoints().Home);
                Assert.AreEqual(expectedGuestSetPts, sets.GetSetPoints().Guest);
            });
        }

        [TestCase(25, 1, 1, 25, 25, 1, 7, 5, true)]
        [TestCase(25, 1, 25, 1, 25, 1, 9, 3, false)]
        [TestCase(25, 25, 25, 25, 25, 25, 0, 0, false)]
        public void Calculate_Set_Points_TieBreak(int s1HomeBall, int s1GuestBall, int s2HomeBall, int s2GuestBall, int s3HomeBall, int s3GuestBall, int expectedHomeSetPts, int expectedGuestSetPts, bool expectedTieBreak)
        {
            var sets = new List<SetEntity>
            {
                new() {HomeBallPoints = s1HomeBall, GuestBallPoints = s1GuestBall},
                new() {HomeBallPoints = s2HomeBall, GuestBallPoints = s2GuestBall},
                new() {HomeBallPoints = s3HomeBall, GuestBallPoints = s3GuestBall}
            };
            var setRule = new SetRuleEntity { PointsSetWon = 3, PointsSetLost = 1, PointsSetTie = 0 };
            var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };
            sets.CalculateSetPoints(setRule, matchRule);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedHomeSetPts, sets.GetSetPoints().Home);
                Assert.AreEqual(expectedGuestSetPts, sets.GetSetPoints().Guest);
                Assert.AreEqual(expectedTieBreak, sets.Last().IsTieBreak);
            });
        }

        [TestCase(25, 1, 5, 4)]
        [TestCase(25, 1, 2, 0)]
        [TestCase(1, 25, 1, 2)]
        public void Overrule_Set_Points(int homeBallPts, int guestBallPts, int homeSetPts, int guestSetPts)
        {
            var set = new SetEntity { HomeBallPoints = homeBallPts, GuestBallPoints = guestBallPts, IsTieBreak = true, IsOverruled = false};
            set.Overrule(homeBallPts, guestBallPts, homeSetPts, guestSetPts);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(homeSetPts, set.HomeSetPoints);
                Assert.AreEqual(guestSetPts, set.GuestSetPoints);
                Assert.IsFalse(set.IsTieBreak);
                Assert.IsTrue(set.IsOverruled);
            });
        }

        [TestCase(25, 1, 25, 1, 25, 1, 3, 0)]
        [TestCase(1, 25, 1, 25, 1, 25, 0, 3)]
        [TestCase(25, 25, 25, 25, 25, 25, 0, 0)]
        public void Get_Sets_Won(int s1HomeBall, int s1GuestBall, int s2HomeBall, int s2GuestBall, int s3HomeBall, int s3GuestBall, int expectedWonHome, int expectedWonGuest)
        {
            var sets = new List<SetEntity>
            {
                new() {HomeBallPoints = s1HomeBall, GuestBallPoints = s1GuestBall},
                new() {HomeBallPoints = s2HomeBall, GuestBallPoints = s2GuestBall},
                new() {HomeBallPoints = s3HomeBall, GuestBallPoints = s3GuestBall}
            };
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expectedWonHome, sets.GetSetsWon().Home);
                Assert.AreEqual(expectedWonGuest, sets.GetSetsWon().Guest);
                Assert.AreEqual(expectedWonHome > expectedWonGuest ? expectedWonHome : expectedWonGuest, sets.MaxBestOf());
            });
        }

        [TestCase(0, 0, 0, 0, 0, 0, 0)]
        [TestCase(25, 1, 25, 1, 25, 1, 78)]
        public void Get_Sets_Total_Ball_Points(int s1HomeBall, int s1GuestBall, int s2HomeBall, int s2GuestBall,
            int s3HomeBall, int s3GuestBall, int expected)
        {
            var sets = new List<SetEntity>
            {
                new() {HomeBallPoints = s1HomeBall, GuestBallPoints = s1GuestBall},
                new() {HomeBallPoints = s2HomeBall, GuestBallPoints = s2GuestBall},
                new() {HomeBallPoints = s3HomeBall, GuestBallPoints = s3GuestBall}
            };

            Assert.AreEqual(expected, sets.GetTotalBallPoints());
        }
    }
}
