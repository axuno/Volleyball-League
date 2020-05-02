using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;

namespace TournamentManager.Tests.ModelValidators
{
    [TestFixture]
    public class SingleSetValidatorTests
    {
        [Test]
        public void All_Ids_Have_A_Check_Function()
        {
            var set = new SetEntity();
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1)));

            var enums = Enum.GetNames(typeof(SingleSetValidator.FactId)).ToList();
            foreach (var e in enums)
            {
                var fact = sv.Facts.First(f => f.Id.Equals(Enum.Parse<SingleSetValidator.FactId>(e)));
                Console.WriteLine(fact.Id);
                Assert.IsTrue(fact.CheckAsync != null);
            }
        }

        [Test]
        [TestCase(0, 0, true)]
        [TestCase(1, 1, true)]
        [TestCase(-1, -2, false)]
        public async Task Ball_Points_Are_Not_Negative(int homePoints, int guestPoints, bool expected)
        {
            var set = new SetEntity {HomeBallPoints = homePoints, GuestBallPoints = guestPoints};
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1)));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.BallPointsNotNegative, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(0, 0, false)]
        [TestCase(-1, -1, false)]
        [TestCase(5, 5, true)]
        [TestCase(15, 15, true)]
        [TestCase(25, 25, true)]
        [TestCase(25, 25, true)]
        [TestCase(25, 0, true)] // non-tie should succeed as well
        [TestCase(35, 0, true)] // non-tie should succeed as well
        public async Task Allow_Tie_In_Regular_Sets_If_Rule_Allows(int homePoints, int guestPoints, bool expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = false};
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { PointsDiffToWinRegular = 0, PointsDiffToWinTiebreak = 0 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieIsAllowed, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(0, 0, false)]
        [TestCase(-1, -1, false)]
        [TestCase(5, 5, false)]
        [TestCase(25, 25, false)]
        [TestCase(25, 23, true)]
        [TestCase(23, 25, true)]
        [TestCase(25, 0, true)]
        [TestCase(35, 0, true)]
        public async Task Disallow_Tie_In_Regular_Sets(int homePoints, int guestPoints, bool expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = false };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { PointsDiffToWinRegular = 2, PointsDiffToWinTiebreak = 2 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieIsAllowed, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(0, 0, false)]
        [TestCase(-1, -1, false)]
        [TestCase(5, 5, true)]
        [TestCase(15, 15, true)]
        [TestCase(25, 25, true)]
        [TestCase(25, 25, true)]
        [TestCase(25, 0, true)] // non-tie should succeed as well
        [TestCase(35, 0, true)] // non-tie should succeed as well
        public async Task Allow_Tie_In_TieBreak_Sets_If_Rule_Allows(int homePoints, int guestPoints, bool expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = true };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { PointsDiffToWinRegular = 0, PointsDiffToWinTiebreak = 0 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieIsAllowed, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(0, 0, false)]
        [TestCase(-1, -1, false)]
        [TestCase(5, 5, false)]
        [TestCase(25, 25, false)]
        [TestCase(25, 23, true)]
        [TestCase(23, 25, true)]
        [TestCase(25, 0, true)]
        [TestCase(35, 0, true)]
        public async Task Disallow_Tie_In_TieBreak_Sets(int homePoints, int guestPoints, bool expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = true };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { PointsDiffToWinRegular = 2, PointsDiffToWinTiebreak = 2 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieIsAllowed, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(25, 0, false, true)]
        [TestCase(0, 25, false, true)]
        [TestCase(0, 24, false, false)]
        [TestCase(24, 0, false, false)]
        [TestCase(25, 24, false, true)] // only points to win are checked here
        [TestCase(15, 0, true, true)]
        [TestCase(0, 15, true, true)]
        [TestCase(14, 0, true, false)]
        [TestCase(0, 14, true, false)]
        [TestCase(15, 14, true, true)] // only points to win are checked here
        public async Task Num_Of_BallPoints_To_Win_Is_Reached(int homePoints, int guestPoints, bool isTieBreak, bool expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, NumOfPointsToWinTiebreak = 15 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.NumOfPointsToWinReached, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(25, 0, false, true)]
        [TestCase(0, 25, false, true)]
        [TestCase(24, 25, false, true)]
        [TestCase(25, 26, false, false)]
        [TestCase(25, 0, true, true)]
        [TestCase(0, 25, true, true)]
        [TestCase(14, 15, true, true)]
        public async Task Regular_Win_Reached_With_One_Point_Ahead(int homePoints, int guestPoints, bool isTieBreak, bool expected)
        {
            // tie-break is ignored
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 1, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 1}));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.RegularWinReachedWithOnePointAhead, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(25, 0, false, true)]
        [TestCase(0, 25, false, true)]
        [TestCase(24, 25, false, true)]
        [TestCase(25, 26, false, true)]
        [TestCase(15, 0, true, true)]
        [TestCase(0, 15, true, true)]
        [TestCase(14, 15, true, true)]
        [TestCase(15, 16, true, false)]
        public async Task TieBreak_Win_Reached_With_One_Point_Ahead(int homePoints, int guestPoints, bool isTieBreak, bool expected)
        {
            // regular set is ignored
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 1, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 1 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieBreakWinReachedWithOnePointAhead, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(25, 0, false, true)]
        [TestCase(0, 25, false, true)]
        [TestCase(23, 25, false, true)]
        [TestCase(25, 26, false, false)]
        [TestCase(25, 0, true, true)]
        [TestCase(0, 25, true, true)]
        [TestCase(14, 15, true, true)]
        [TestCase(15, 16, true, true)]
        public async Task Regular_Win_Reached_With_TwoOrMore_Points_Ahead(int homePoints, int guestPoints, bool isTieBreak, bool expected)
        {
            // tie-break is ignored
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.RegularWinReachedWithTwoPlusPointsAhead, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(25, 0, false, true)]
        [TestCase(0, 25, false, true)]
        [TestCase(23, 25, false, true)]
        [TestCase(25, 26, false, true)]
        [TestCase(15, 0, true, true)]
        [TestCase(0, 15, true, true)]
        [TestCase(14, 15, true, false)]
        [TestCase(14, 16, true, true)]
        public async Task TieBreak_Win_Reached_With_TwoOrMore_Points_Ahead(int homePoints, int guestPoints, bool isTieBreak, bool expected)
        {
            // regular sets are ignored
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2 }));
            var factResult = await sv.CheckAsync(SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead, CancellationToken.None);
            Assert.Multiple(() =>
            {
                Assert.AreEqual(expected, factResult.Success);
                Assert.IsNotNull(factResult.Message);
                Assert.IsNull(factResult.Exception);
            });
        }

        [TestCase(-1, 0, true, SingleSetValidator.FactId.BallPointsNotNegative)]
        [TestCase(-1, 0, false, SingleSetValidator.FactId.BallPointsNotNegative)]
        [TestCase(12, 1, true, SingleSetValidator.FactId.NumOfPointsToWinReached)]
        [TestCase(24, 1, false, SingleSetValidator.FactId.NumOfPointsToWinReached)]
        [TestCase(15, 15, true, SingleSetValidator.FactId.TieIsAllowed)]
        [TestCase(25, 25, false, SingleSetValidator.FactId.TieIsAllowed)]
        [TestCase(15, 14, true, SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead)]
        [TestCase(25, 24, false, SingleSetValidator.FactId.RegularWinReachedWithTwoPlusPointsAhead)]
        [TestCase(15, 16, true, SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead)]
        [TestCase(25, 26, false, SingleSetValidator.FactId.RegularWinReachedWithTwoPlusPointsAhead)]
        public async Task Test_for_all_Facts(int homePoints, int guestPoints, bool isTieBreak, SingleSetValidator.FactId expected)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2 }));
            var result = await sv.CheckAsync(CancellationToken.None);
            Assert.AreEqual(expected, sv.GetFailedFacts().First(r => !r.Success).Id);
        }

        [TestCase(25, 0, false)]
        [TestCase(25, 27, false)]
        [TestCase(15, 0, true)]
        [TestCase(15, 17, true)]
        public async Task Test_for_all_Facts_Should_Succeed(int homePoints, int guestPoints, bool isTieBreak)
        {
            var set = new SetEntity { HomeBallPoints = homePoints, GuestBallPoints = guestPoints, IsTieBreak = isTieBreak };
            var sv = new SingleSetValidator(set, (new OrganizationContext(), new SetRuleEntity(1) { NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2 }));
            var result = await sv.CheckAsync(CancellationToken.None);
            Assert.AreEqual(0, sv.GetFailedFacts().Count);
        }

        [Test]
        public void Check_FieldName_Of_Facts()
        {
            var fv = new SingleSetValidator(new SetEntity(),  (new OrganizationContext(), new SetRuleEntity()));

            foreach (var fact in fv.Facts)
            {
                switch (fact.Id)
                {
                    case SingleSetValidator.FactId.BallPointsNotNegative:
                    case SingleSetValidator.FactId.TieIsAllowed:
                    case SingleSetValidator.FactId.NumOfPointsToWinReached:
                    case SingleSetValidator.FactId.RegularWinReachedWithOnePointAhead:
                    case SingleSetValidator.FactId.TieBreakWinReachedWithOnePointAhead:
                    case SingleSetValidator.FactId.RegularWinReachedWithTwoPlusPointsAhead:
                    case SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead:
                        Assert.IsTrue(fact.FieldNames.Count() == 2);
                        break;
                }
            }
        }
    }
}
