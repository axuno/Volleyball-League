using Moq;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;
using TournamentManager.MultiTenancy;
using TournamentManager.Tests.TestComponents;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class MatchResultValidatorTests
{
    private readonly (ITenantContext TenantContext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, (MatchRuleEntity MatchRule, SetRuleEntity SetRule) Rules) _data;
        
    public MatchResultValidatorTests()
    {
        #region *** TimeZoneConverter ***

        var tzProvider = new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default);
        var tzId = "Europe/Berlin";
        _data.TimeZoneConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(tzProvider, tzId, System.Globalization.CultureInfo.GetCultureInfo("en-US"), NodaTime.TimeZones.Resolvers.LenientResolver);

        #endregion

        #region *** Mocks ***
            
        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        var roundRepoMock = TestMocks.GetRepo<RoundRepository>();
        roundRepoMock.Setup(rep => rep.GetRoundWithLegsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long roundId, CancellationToken cancellationToken) =>
                {
                    var roundEntity = new RoundEntity { Id = roundId, Name = "RoundName", Description = "RoundDescription", NumOfLegs = 2 };
                    for (var i = 0; i < roundEntity.NumOfLegs; i++)
                    {
                        roundEntity.RoundLegs.Add(new RoundLegEntity
                        {
                            Id = i,
                            SequenceNo = i + 1,
                            StartDateTime = new DateTime(2020, 9 + i, 1),
                            EndDateTime = new DateTime(2020, 9 + i, 20),
                            RoundId = roundEntity.Id
                        });
                    }

                    return Task.FromResult(roundEntity)!;
                }
            );
        appDbMock.Setup(a => a.RoundRepository).Returns(roundRepoMock.Object);

        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
            
        tenantContextMock.SetupDbContext(dbContextMock);
            
        _data.TenantContext = tenantContextMock.Object;

        #endregion
    }

    [Test]
    public void All_Ids_Have_A_Check_Function()
    {
        var set = new MatchEntity();
        var sv = new MatchResultValidator(set, _data, MatchValidationMode.Default);

        var enums = Enum.GetNames(typeof(MatchResultValidator.FactId)).ToList();
        foreach (var e in enums)
        {
            var fact = sv.Facts.First(f => f.Id.Equals(Enum.Parse<MatchResultValidator.FactId>(e)));
            Console.WriteLine(fact.Id);
            Assert.That(fact.CheckAsync, Is.Not.EqualTo(null));
        }
    }

    [Test]
    public async Task Sets_Validation_Causes_Errors()
    {
        var match = new MatchEntity();
        var sets = new List<SetEntity>
        {
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false} // fails
        };
        match.Sets.AddRange(sets);

        var setRule = new SetRuleEntity
        {
            NumOfPointsToWinRegular = 25,
            PointsDiffToWinRegular = 2,
            NumOfPointsToWinTiebreak = 15,
            PointsDiffToWinTiebreak = 2
        };

        var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };

        var mv = new MatchResultValidator(match, (_data.TenantContext, _data.TimeZoneConverter, (matchRule, setRule)), MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.SetsValidatorSuccessful, CancellationToken.None);
        var factResult = mv.GetFailedFacts().First(f => f.Id == MatchResultValidator.FactId.SetsValidatorSuccessful);
        Assert.Multiple(() =>
        {
            Assert.That(mv.SetsValidator.SingleSetErrors, Is.Empty);
            Assert.That(mv.SetsValidator.GetFailedFacts().First().Id, Is.EqualTo(SetsValidator.FactId.BestOfRequiredTieBreakPlayed));
            Assert.That(factResult.Success, Is.False);
            Assert.That(factResult.Exception, Is.Null);
        });
    }

    [Test]
    public async Task Sets_Validation_Causes_No_Errors()
    {
        var match = new MatchEntity();
        var sets = new List<SetEntity>
        {
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = true},
        };
        match.Sets.AddRange(sets);

        var setRule = new SetRuleEntity
        {
            NumOfPointsToWinRegular = 25,
            PointsDiffToWinRegular = 2,
            NumOfPointsToWinTiebreak = 15,
            PointsDiffToWinTiebreak = 2
        };

        var matchRule = new MatchRuleEntity { BestOf = true, NumOfSets = 2 };

        var mv = new MatchResultValidator(match, (_data.TenantContext, _data.TimeZoneConverter, (matchRule, setRule)), MatchValidationMode.Default);
        var factResult = await mv.CheckAsync(MatchResultValidator.FactId.SetsValidatorSuccessful, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.That(mv.GetFailedFacts(), Is.Empty);
            Assert.That(mv.SetsValidator.SingleSetErrors, Is.Empty);
            Assert.That(factResult.Success, Is.True);
            Assert.That(factResult.Exception, Is.Null);
        });
    }

    [TestCase("2020-09-01 20:00:00", false)]
    [TestCase("2020-09-20 23:00:00", true)] // end date will be out of the round leg
    [TestCase("2020-06-01 20:00:00", true)] // start date will be out of the round leg
    [TestCase(null, false)] // does not fail, because this case is covered by MatchResultValidator.FactId.RealMatchDateIsSet
    public async Task RealMatchDate_Within_Round_Legs(DateTime? realStart, bool fails)
    {
        var match = new MatchEntity
        {
            Id = 1,
            RealStart = realStart,
            RealEnd = realStart?.AddHours(2),
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };

        var mv = new MatchResultValidator(match, _data, MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.RealMatchDateWithinRoundLegs, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            if (fails)
            {
                Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.RealMatchDateWithinRoundLegs));
                Assert.That(factResults.First().Message, Is.Not.Null);
            }
            else
            {
                Assert.That(factResults, Is.Empty);
            }
                
        });
    }

    [Test]
    public async Task RealMatchDate_Must_Not_Be_Null()
    {
        var match = new MatchEntity
        {
            Id = 1,
            RealStart = null,
            RealEnd = null,
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };

        var mv = new MatchResultValidator(match, _data, MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.RealMatchDateIsSet, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            Assert.That(factResults, Has.Count.EqualTo(1));
            Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.RealMatchDateIsSet));
            Assert.That(factResults.First().Message, Is.Not.Null);
        });
    }

    [Test]
    public async Task RealMatchDate_Must_Not_Be_Future_Date()
    {
        var today = new DateTime(2023, 7, 1, 23, 59, 59);
        var match = new MatchEntity
        {
            Id = 1,
            RealStart = today.AddDays(1),
            RealEnd = today.AddDays(1),
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };

        var mv = new MatchResultValidator(match, _data, MatchValidationMode.Default) { Today = today };
        await mv.CheckAsync(MatchResultValidator.FactId.RealMatchDateTodayOrBefore, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            Assert.That(factResults, Has.Count.EqualTo(1));
            Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.RealMatchDateTodayOrBefore));
            Assert.That(factResults.First().Message, Is.Not.Null);
        });
    }

    [Test]
    public async Task RealMatchDate_Should_Equal_Fixture()
    {
        var plannedStart = new DateTime(2020, 07, 01, 20, 00, 00);
        var match = new MatchEntity
        {
            Id = 1,
            PlannedStart = plannedStart,
            PlannedEnd = plannedStart.AddHours(2),
            RealStart = plannedStart.AddDays(-1),
            RealEnd = plannedStart.AddDays(-1).AddHours(2),
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };

        var mv = new MatchResultValidator(match, _data, MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.RealMatchDateEqualsFixture, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            Assert.That(factResults, Has.Count.EqualTo(1));
            Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.RealMatchDateEqualsFixture));
            Assert.That(factResults.First().Message, Is.Not.Null);
            Assert.That(factResults.First().Type, Is.EqualTo(FactType.Warning));
        });
    }

    [TestCase(180, true)]
    [TestCase(10, true)]
    [TestCase(80, false)]
    public async Task RealMatch_Duration_Is_Plausible(int duration, bool shouldFail)
    {
        var start = new DateTime(2020, 07, 01, 20, 00, 00);
        var match = new MatchEntity
        {
            Id = 1,
            PlannedStart = start,
            PlannedEnd = start.AddHours(2),
            RealStart = start,
            RealEnd = start.AddMinutes(duration).AddMinutes(5),
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };
        var sets = new List<SetEntity>
        {
            // total ball points: 144, average time = 144 * 33 = 4,752 seconds = 79 minutes
            new() {Id=10, SequenceNo = 1, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
            new() {Id=11, SequenceNo = 2, HomeBallPoints = 23, GuestBallPoints = 25, IsTieBreak = false},
            new() {Id=12, SequenceNo = 3, HomeBallPoints = 25, GuestBallPoints = 23, IsTieBreak = false},
        };
        match.Sets.AddRange(sets);

        var mv = new MatchResultValidator(match, _data, MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.RealMatchDurationIsPlausible, CancellationToken.None);
        var factResults = mv.GetFailedFacts();

        if (shouldFail)
        {
            Assert.Multiple(() =>
            {
                Assert.That(factResults, Has.Count.EqualTo(1));
                Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.RealMatchDurationIsPlausible));
                Assert.That(factResults.First().Message, Is.Not.Null);
                Assert.That(factResults.First().Type, Is.EqualTo(FactType.Warning));
            });
        }
        else
        {
            Assert.That(factResults, Is.Empty);
        }

    }

    [TestCase(0,0)]
    [TestCase(1,4)]
    [TestCase(2, 3)]
    [TestCase(3, 2)]
    [TestCase(4, 1)]
    public async Task Valid_Match_Points_Should_Pass_Validation(int? home, int? guest)
    {
        var data = _data;
        data.Rules.MatchRule = new MatchRuleEntity
        {
            BestOf = true, NumOfSets = 3, PointsMatchLost = 0, PointsMatchTie = 1, PointsMatchWon = 2,
            PointsMatchLostAfterTieBreak = 3, PointsMatchWonAfterTieBreak = 4
        };

        var match = new MatchEntity
        {
            HomePoints = home,
            GuestPoints = guest
        };
        
        var mv = new MatchResultValidator(match, data, MatchValidationMode.Default);
        await mv.CheckAsync(MatchResultValidator.FactId.MatchPointsAreValid, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.That(factResults, Has.Count.EqualTo(0));
    }

    [TestCase(10, 1)]
    [TestCase(-1, -1)]
    [TestCase(null, 1)]
    [TestCase(null, null)]
    public async Task Invalid_Match_Points_Should_Fail_To_Validate(int? home, int? guest)
    {
        var data = _data;
        data.Rules.MatchRule = new MatchRuleEntity
        {
            BestOf = true, NumOfSets = 3, PointsMatchLost = 0, PointsMatchTie = 1, PointsMatchWon = 2,
            PointsMatchLostAfterTieBreak = 3, PointsMatchWonAfterTieBreak = 4
        };

        var match = new MatchEntity
        {
            HomePoints = home,
            GuestPoints = guest
        };

        var mv = new MatchResultValidator(match, data, MatchValidationMode.Overrule);
        await mv.CheckAsync(MatchResultValidator.FactId.MatchPointsAreValid, CancellationToken.None);
        var factResults = mv.GetFailedFacts();
        Assert.Multiple(() =>
        {
            Assert.That(factResults, Has.Count.EqualTo(1));
            Assert.That(factResults.First().Id, Is.EqualTo(MatchResultValidator.FactId.MatchPointsAreValid));
            Assert.That(factResults.First().Message, Is.Not.Null.And.Contains("0,1,2,3,4"));
            Assert.That(factResults.First().Type, Is.EqualTo(FactType.Critical));
        });
    }

    [Test]
    public void Check_FieldName_Of_Facts()
    {
        var mv = new MatchResultValidator(new MatchEntity(), _data, MatchValidationMode.Default);

        foreach (var fact in mv.Facts)
        {
            switch (fact.Id)
            {
                case MatchResultValidator.FactId.RealMatchDateIsSet:
                    Assert.That(fact.FieldNames.Count(), Is.EqualTo(2));
                    break;
                case MatchResultValidator.FactId.RealMatchDateWithinRoundLegs:
                    Assert.That(fact.FieldNames.Count(), Is.EqualTo(2));
                    break;
                case MatchResultValidator.FactId.SetsValidatorSuccessful:
                    Assert.That(fact.FieldNames.Count(), Is.EqualTo(1));
                    break;
            }
        }
    }
}
