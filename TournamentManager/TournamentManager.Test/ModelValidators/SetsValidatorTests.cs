using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.ModelValidators
{
    [TestFixture]
    public class SetsValidatorTests
    {
        private readonly ILogger _logger = new NullLogger<SetsValidatorTests>();

        [Test]
        public void All_Ids_Have_A_Check_Function()
        {
            var sv = new SetsValidator(new List<SetEntity>(), (new TenantContext(), (new MatchRuleEntity(1), new SetRuleEntity(1))));

            var enums = Enum.GetNames(typeof(SetsValidator.FactId)).ToList();
            foreach (var e in enums)
            {
                var fact = sv.Facts.First(f => f.Id.Equals(Enum.Parse<SetsValidator.FactId>(e)));
                Console.WriteLine(fact.Id);
                Assert.IsTrue(fact.CheckAsync != null);
            }
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, false)]
        public async Task Min_And_Max_Number_Of_Sets(int numOfSets, bool shouldSucceed)
        {
            var sets = new List<SetEntity>();
            for (var i = 0; i < numOfSets; i++)
            {
                sets.Add(new SetEntity());
            }
            var sv = new SetsValidator(sets, (new TenantContext(), (new MatchRuleEntity {BestOf = false, NumOfSets = 3}, new SetRuleEntity())));
            await sv.CheckAsync(SetsValidator.FactId.MixAndMaxOfSetsPlayed, CancellationToken.None);
            Assert.Multiple(() =>
                {
                    if (shouldSucceed)
                    {
                        Assert.AreEqual(0, sv.GetFailedFacts().Count);
                    }
                    else
                    {
                        Assert.IsFalse(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.MixAndMaxOfSetsPlayed).Success);
                        Assert.IsNotNull(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.MixAndMaxOfSetsPlayed).Message);
                    }
                }
            );
        }

        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, true)]
        [TestCase(4, true)]
        [TestCase(5, true)]
        [TestCase(6, false)]
        public async Task Min_And_Max_Number_Of_Sets_BestOf(int numOfSets, bool shouldSucceed)
        {
            var sets = new List<SetEntity>();
            for (var i = 0; i < numOfSets; i++)
            {
                sets.Add(new SetEntity());
            }

            var sv = new SetsValidator(sets, (new TenantContext(), (new MatchRuleEntity { BestOf = true, NumOfSets = 3 }, new SetRuleEntity())));
            await sv.CheckAsync(SetsValidator.FactId.BestOfMixAndMaxOfSetsPlayed, CancellationToken.None);
            Assert.Multiple(() =>
                {
                    if (shouldSucceed)
                    {
                        Assert.AreEqual(0, sv.GetFailedFacts().Count);
                    }
                    else
                    {
                        Assert.IsFalse(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfMixAndMaxOfSetsPlayed).Success);
                        Assert.IsNotNull(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfMixAndMaxOfSetsPlayed).Message);
                    }
                }
            );
        }
        
        [Test]
        public async Task Invalid_Sets()
        {
            var sets = new List<SetEntity>
            {
                new SetEntity {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
                new SetEntity {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
                new SetEntity {Id=12, SequenceNo = 3, HomeBallPoints = 16, GuestBallPoints = 15, IsTieBreak = true} // false
            };

            var setRule = new SetRuleEntity
            {
                NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, 
                NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2
            };

            var matchRule = new MatchRuleEntity() {BestOf = true, NumOfSets = 2};
            
            var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, setRule)));
            await sv.CheckAsync(CancellationToken.None);
            var errorFacts = sv.GetFailedFacts();
            Assert.Multiple(() =>
            {
                Assert.AreEqual(1, errorFacts.Count);
                Assert.AreEqual(1, sv.SingleSetErrors.Count);
                Assert.AreEqual(3, sv.SingleSetErrors.First().SequenceNo);
                Assert.AreEqual(SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead, sv.SingleSetErrors.First().FactId);
            });
        }

        [Test]
        public async Task BestOf_Required_TieBreak_Played()
        {
            var sets = new List<SetEntity>
            {
                new SetEntity {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
                new SetEntity {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
                new SetEntity {Id=12, SequenceNo = 3, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = false},
            };

            var matchRule = new MatchRuleEntity() { BestOf = true, NumOfSets = 2 };

            var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, new SetRuleEntity())));
            await sv.CheckAsync(SetsValidator.FactId.BestOfRequiredTieBreakPlayed, CancellationToken.None);
            var factResult = sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfRequiredTieBreakPlayed);
            Assert.Multiple(() =>
            {
                Assert.IsFalse(factResult.Success);
                Assert.IsNull(factResult.Exception);
            });

            sets[2].IsTieBreak = true;
            await sv.CheckAsync(SetsValidator.FactId.BestOfRequiredTieBreakPlayed, CancellationToken.None);
            Assert.AreEqual(0, sv.GetFailedFacts().Count);
        }

        [Test]
        public async Task BestOf_No_Match_After_BestOf_Reached()
        {
            var sets = new List<SetEntity>
            {
                new SetEntity {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
                new SetEntity {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
                new SetEntity {Id=12, SequenceNo = 3, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = true},
                new SetEntity {Id=13, SequenceNo = 4, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
                new SetEntity {Id=15, SequenceNo = 5, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = true},
            };

            var setRule = new SetRuleEntity {PointsSetWon = 1, PointsSetLost = 0, PointsSetTie = 0};
            var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };

            var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, new SetRuleEntity())));
            await sv.CheckAsync(SetsValidator.FactId.BestOfNoMatchAfterBestOfReached, CancellationToken.None);
            var factResult = sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfNoMatchAfterBestOfReached);
            Assert.Multiple(() =>
            {
                Assert.IsFalse(factResult.Success);
                Assert.IsNull(factResult.Exception);
            });

            // Remove sets which exceed "Best-of-2 out of 3"
            sets.RemoveAt(3);
            sets.RemoveAt(3);
            await sv.CheckAsync(SetsValidator.FactId.BestOfNoMatchAfterBestOfReached, CancellationToken.None);
            Assert.AreEqual(0, sv.GetFailedFacts().Count);
        }

        [Test]
        public void Check_FieldName_Of_Facts()
        {
            var sv = new SetsValidator(new List<SetEntity>(), (new TenantContext(), (new MatchRuleEntity(), new SetRuleEntity())));

            foreach (var fact in sv.Facts)
            {
                switch (fact.Id)
                {
                    case SetsValidator.FactId.AllSetsAreValid:
                        Assert.IsTrue(fact.FieldNames.Count() == 1);
                        break;
                }
            }
        }
    }
}
