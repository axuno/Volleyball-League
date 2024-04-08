using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class SetsValidatorTests
{
    [Test]
    public void All_Ids_Have_A_Check_Function()
    {
        var sv = new SetsValidator(new List<SetEntity>(), (new TenantContext(), (new MatchRuleEntity(1), new SetRuleEntity(1))), MatchValidationMode.Default);

        var enums = Enum.GetNames(typeof(SetsValidator.FactId)).ToList();
        foreach (var e in enums)
        {
            var fact = sv.Facts.First(f => f.Id.Equals(Enum.Parse<SetsValidator.FactId>(e)));
            Console.WriteLine(fact.Id);
            Assert.That(fact.CheckAsync, Is.Not.EqualTo(null));
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
        var sv = new SetsValidator(sets, (new TenantContext(), (new MatchRuleEntity {BestOf = false, NumOfSets = 3}, new SetRuleEntity())), MatchValidationMode.Default);
        await sv.CheckAsync(SetsValidator.FactId.MinAndMaxOfSetsPlayed, CancellationToken.None);
        Assert.Multiple(() =>
            {
                if (shouldSucceed)
                {
                    Assert.That(sv.GetFailedFacts(), Is.Empty);
                }
                else
                {
                    Assert.That(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.MinAndMaxOfSetsPlayed).Success, Is.False);
                    Assert.That(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.MinAndMaxOfSetsPlayed).Message, Is.Not.Null);
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

        var sv = new SetsValidator(sets, (new TenantContext(), (new MatchRuleEntity { BestOf = true, NumOfSets = 3 }, new SetRuleEntity())), MatchValidationMode.Default);
        await sv.CheckAsync(SetsValidator.FactId.BestOfMinAndMaxOfSetsPlayed, CancellationToken.None);
        Assert.Multiple(() =>
            {
                if (shouldSucceed)
                {
                    Assert.That(sv.GetFailedFacts(), Is.Empty);
                }
                else
                {
                    Assert.That(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfMinAndMaxOfSetsPlayed).Success, Is.False);
                    Assert.That(sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfMinAndMaxOfSetsPlayed).Message, Is.Not.Null);
                }
            }
        );
    }
        
    [Test]
    public async Task Invalid_Sets()
    {
        var sets = new List<SetEntity>
        {
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 16, GuestBallPoints = 15, IsTieBreak = true} // false
        };

        var setRule = new SetRuleEntity
        {
            NumOfPointsToWinRegular = 25, PointsDiffToWinRegular = 2, 
            NumOfPointsToWinTiebreak = 15, PointsDiffToWinTiebreak = 2
        };

        var matchRule = new MatchRuleEntity() {BestOf = true, NumOfSets = 2};
            
        var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, setRule)), MatchValidationMode.Default);
        await sv.CheckAsync(CancellationToken.None);
        var errorFacts = sv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            Assert.That(errorFacts, Has.Count.EqualTo(1));
            Assert.That(sv.SingleSetErrors, Has.Count.EqualTo(1));
            Assert.That(sv.SingleSetErrors.First().SequenceNo, Is.EqualTo(3));
            Assert.That(sv.SingleSetErrors.First().FactId, Is.EqualTo(SingleSetValidator.FactId.TieBreakWinReachedWithTwoPlusPointsAhead));
        });
    }

    [Test]
    public async Task BestOf_Required_TieBreak_Played()
    {
        var sets = new List<SetEntity>
        {
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = false},
        };

        var matchRule = new MatchRuleEntity() { BestOf = true, NumOfSets = 2 };

        var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, new SetRuleEntity())), MatchValidationMode.Default);
        await sv.CheckAsync(SetsValidator.FactId.BestOfRequiredTieBreakPlayed, CancellationToken.None);
        var factResult = sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfRequiredTieBreakPlayed);
        Assert.Multiple(() =>
        {
            Assert.That(factResult.Success, Is.False);
            Assert.That(factResult.Exception, Is.Null);
        });

        sets[2].IsTieBreak = true;
        await sv.CheckAsync(SetsValidator.FactId.BestOfRequiredTieBreakPlayed, CancellationToken.None);
        Assert.That(sv.GetFailedFacts(), Is.Empty);
    }

    [Test]
    public async Task BestOf_No_Match_After_BestOf_Reached()
    {
        var sets = new List<SetEntity>
        {
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = true},
            new() {Id=13, SequenceNo = 4, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=15, SequenceNo = 5, HomeBallPoints = 15, GuestBallPoints = 10, IsTieBreak = true},
        };

        var setRule = new SetRuleEntity {PointsSetWon = 1, PointsSetLost = 0, PointsSetTie = 0};
        var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };

        var sv = new SetsValidator(sets, (new TenantContext(), (matchRule, new SetRuleEntity())), MatchValidationMode.Default);
        await sv.CheckAsync(SetsValidator.FactId.BestOfNoMatchAfterBestOfReached, CancellationToken.None);
        var factResult = sv.GetFailedFacts().First(f => f.Id == SetsValidator.FactId.BestOfNoMatchAfterBestOfReached);
        Assert.Multiple(() =>
        {
            Assert.That(factResult.Success, Is.False);
            Assert.That(factResult.Exception, Is.Null);
        });

        // Remove sets which exceed "Best-of-2 out of 3"
        sets.RemoveAt(3);
        sets.RemoveAt(3);
        await sv.CheckAsync(SetsValidator.FactId.BestOfNoMatchAfterBestOfReached, CancellationToken.None);
        Assert.That(sv.GetFailedFacts(), Is.Empty);
    }

    [Test]
    public void Check_FieldName_Of_Facts()
    {
        var sv = new SetsValidator(new List<SetEntity>(), (new TenantContext(), (new MatchRuleEntity(), new SetRuleEntity())), MatchValidationMode.Default);

        foreach (var fact in sv.Facts)
        {
            switch (fact.Id)
            {
                case SetsValidator.FactId.AllSetsAreValid:
                    Assert.That(fact.FieldNames.Count(), Is.EqualTo(1));
                    break;
            }
        }
    }
}
