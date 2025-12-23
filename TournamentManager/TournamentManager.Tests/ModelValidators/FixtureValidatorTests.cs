using System.Globalization;
using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using TournamentManager.Data;
using Moq;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.ExtensionMethods;
using TournamentManager.Tests.TestComponents;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class FixtureValidatorTests
{
    private (ITenantContext TenantConext, Axuno.Tools.DateAndTime.TimeZoneConverter TimeZoneConverter, PlannedMatchRow PlannedMatch) _data;
    private readonly CultureInfo _culture = CultureInfo.GetCultureInfo("en-US");
    private const string ExcludedDateReason = "Unit-Test";

    public FixtureValidatorTests()
    {
        #region *** TimeZoneConverter ***

        _data.TimeZoneConverter = new("Europe/Berlin", _culture);

        #endregion

        #region *** Mocks ***
        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        var venueRepoMock = TestMocks.GetRepo<VenueRepository>();
        venueRepoMock.Setup(rep => rep.IsValidVenueIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns((long id, CancellationToken cancellationToken) => Task.FromResult(id % 2 == 0));
        venueRepoMock.Setup(rep => rep.GetOccupyingMatchesAsync(It.IsAny<long>(), It.IsAny<DateTimePeriod>(), It.IsAny<long>(), It.IsAny<CancellationToken>()
        )).Returns(
            (long venueId, DateTimePeriod searchPeriod, long tournamentId, CancellationToken cancellationToken) =>
                Task.FromResult(venueId % 2 == 0
                    ? new List<PlannedMatchRow>()
                    : [new() {Id = 1, VenueId = venueId}]));
        appDbMock.Setup(a => a.VenueRepository).Returns(venueRepoMock.Object);

        var matchRepoMock = TestMocks.GetRepo<MatchRepository>();
        matchRepoMock.Setup(rep =>
                rep.AreTeamsBusyAsync(It.IsAny<MatchEntity>(), It.IsAny<bool>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((MatchEntity match, bool onlyDatePart, long tournamentId, CancellationToken cancellationToken) =>
                Task.FromResult(match.Id % 2 == 0 || !match.PlannedStart.HasValue
                    ? [] 
                    : new[] { match.HomeTeamId }));
        appDbMock.Setup(a => a.MatchRepository).Returns(matchRepoMock.Object);
            
        var excludedMatchDateRepoMock = TestMocks.GetRepo<ExcludedMatchDateRepository>();
        excludedMatchDateRepoMock.Setup(rep =>
                rep.GetExcludedMatchDateAsync(It.IsAny<MatchEntity>(), It.IsAny<long>(),
                    It.IsAny<CancellationToken>()))
            .Returns(
                (MatchEntity match, long tournamentId, CancellationToken cancellationToken) =>
                    Task.FromResult(match.Id % 2 == 0 || !match.PlannedStart.HasValue
                        ? null
                        : new ExcludeMatchDateEntity
                        {
                            Id = 1, TournamentId = tournamentId, DateFrom = match.PlannedStart.Value.AddDays(-1),
                            DateTo = match.PlannedStart.Value.AddDays(1), Reason = ExcludedDateReason
                        }));
        appDbMock.Setup(a => a.ExcludedMatchDateRepository).Returns(excludedMatchDateRepoMock.Object);

        var teamRepoMock = TestMocks.GetRepo<TeamRepository>();
        teamRepoMock
            .Setup(rep => rep.GetTeamEntitiesAsync(It.IsAny<PredicateExpression>(), It.IsAny<CancellationToken>()))
            .Callback(() => { }).Returns((PredicateExpression filter, CancellationToken cancellationToken) =>
            {
                var team1Id = (long)((FieldCompareValuePredicate)((PredicateExpression)filter[0].Contents)[0].Contents).Value;
                var team2Id = (long)((FieldCompareValuePredicate)((PredicateExpression)filter[0].Contents)[2].Contents).Value;
                var teams = new List<TeamEntity>
                {
                    new() {
                        Id = team1Id,
                        VenueId = 100 + team1Id,
                        MatchDayOfWeek = team1Id < 10000 ? (int)(team1Id % 6) : default(int?),
                        MatchTime = new TimeSpan(18, 0, 0)
                    },
                    new() {
                        Id = team2Id,
                        // make both venue ids equal
                        VenueId = team2Id != 123 ? 200 + team2Id : 100 + team1Id,
                        MatchDayOfWeek = team2Id < 10000 ? (int)(team2Id % 6) : default(int?),
                        MatchTime = new TimeSpan(19, 0, 0)
                    }
                };
                return Task.FromResult(teams);
            });
        appDbMock.Setup(a => a.TeamRepository).Returns(teamRepoMock.Object);

        var roundRepoMock = TestMocks.GetRepo<RoundRepository>();
        roundRepoMock.Setup(rep => rep.GetRoundWithLegsAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long roundId, CancellationToken cancellationToken) =>
                {
                    var roundEntity = new RoundEntity {Id = roundId, Name="RoundName", Description = "RoundDescription", NumOfLegs = 2};
                    for (var i = 0; i < roundEntity.NumOfLegs; i++)
                    {
                        roundEntity.RoundLegs.Add(new()
                        {
                            Id = i, SequenceNo = i + 1, StartDateTime = new(2020, 9 + i, 1),
                            EndDateTime = new(2020, 9 + i, 20), RoundId = roundEntity.Id
                        });
                    }

                    return Task.FromResult(roundEntity)!;
                }
            );
        appDbMock.Setup(a => a.RoundRepository).Returns(roundRepoMock.Object);

        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
            
        tenantContextMock.SetupDbContext(dbContextMock);
        _data.TenantConext = tenantContextMock.Object;
            
        #endregion
    }

    [Test]
    public void All_Ids_Have_A_Check_Function()
    {
        var match = new MatchEntity();
        _data.TenantConext.TournamentContext.FixtureRuleSet = new();
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);

        var enums = Enum.GetNames<FixtureValidator.FactId>().ToList();
        foreach (var e in enums)
        {
            var fact = fv.Facts.First(f => f.Id.Equals(Enum.Parse<FixtureValidator.FactId>(e)));
            Console.WriteLine(fact.Id);
            Assert.That(fact.CheckAsync, Is.Not.Null);
        }
    }

    [Test]
    [TestCase(null, true, false)]
    [TestCase(null, false, true)]
    [TestCase("2000-01-01 20:00:00", true, true)]
    [TestCase("2200-01-01 20:00:00", true, true)]
    public async Task PlannedStart_MustBeSet(DateTime? plannedStart, bool plannedMatchTimeMustBeSet, bool expected)
    {
        var match = new MatchEntity { PlannedStart = plannedStart};
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { PlannedMatchDateTimeMustBeSet = plannedMatchTimeMustBeSet};
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);

        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartIsSet, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.EqualTo(_data.TenantConext.TournamentContext.FixtureRuleSet.PlannedMatchDateTimeMustBeSet));
            Assert.That(!factResult.Enabled || factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase("2019-06-15 19:00:00", false)]
    [TestCase("2099-06-15 19:00:00", true)]
    [TestCase(null, true)]
    public async Task PlannedStart_Is_Future_Date(DateTime? plannedStart, bool expected)
    {
        var match = new MatchEntity {PlannedStart = plannedStart};
        _data.TenantConext.TournamentContext.FixtureRuleSet = new();
        var fv = new FixtureValidator(match, _data, new(2019, 06, 30, 19, 00, 00));
        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartIsFutureDate, CancellationToken.None);
        Assert.That(factResult.Success, Is.EqualTo(expected));
    }
        
    [Test]
    [TestCase(null, "19:00:00", "21:00:00", false, true)]
    [TestCase("2020-01-01 20:00:00", "19:00:00", "21:00:00", false, true)]
    [TestCase("2020-01-01 22:00:00", "19:00:00", "21:00:00", false, false)]
    [TestCase("2020-01-01 17:00:00", "19:00:00", "21:00:00", false, false)]
    [TestCase("2020-07-01 17:00:00", "19:00:00", "21:00:00", false, true)] // DST in time zone Europe/Berlin
    public async Task PlannedStart_MayBeNull_OrWithin_TimeLimit(DateTime? plannedStart, TimeSpan minStart, TimeSpan maxStart, bool plannedMatchTimeMustBeSet, bool expected)
    {
        // Note: While PlannedStart is treated as UTC, MinStart and MaxStart are in local time

        var match = new MatchEntity();
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { PlannedMatchDateTimeMustBeSet = plannedMatchTimeMustBeSet, RegularMatchStartTime = new() { MinDayTime = minStart, MaxDayTime = maxStart } };
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);

        match.PlannedStart = plannedStart;
        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartWithinDesiredTimeRange, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success && factResult.Message.Contains(_data.TenantConext.TournamentContext.FixtureRuleSet
                    .RegularMatchStartTime.MinDayTime.ToShortTimeString()), Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase(null, false, true)]
    [TestCase("2020-09-01 20:00:00", false, true)]
    [TestCase("2020-09-30 20:00:00", false, false)]
    [TestCase("2020-10-02 17:00:00", false, true)]
    [TestCase("2020-10-30 17:00:00", false, false)]
    [TestCase(null, true, true)]
    [TestCase("2020-09-01 20:00:00", true, true)]
    [TestCase("2020-09-30 20:00:00", true, false)]
    [TestCase("2020-10-02 17:00:00", true, false)]
    [TestCase("2020-10-30 17:00:00", true, false)]
    public async Task PlannedStart_Within_Leg_Time_Limits(DateTime? plannedStart, bool dateWithLegBoundaries, bool expected)
    {
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { PlannedMatchTimeMustStayInCurrentLegBoundaries = dateWithLegBoundaries };
        var match = new MatchEntity
        {
            Id = 1,
            PlannedStart = plannedStart,
            PlannedEnd = plannedStart?.AddHours(2),
            RoundId = 2,
            LegSequenceNo = 1,
            HomeTeamId = 7,
            GuestTeamId = 10
        };
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);
        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartWithinRoundLegs, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase("2020-09-01 20:00:00", 101, true, false)]
    [TestCase("2020-09-01 20:00:00", 102, true, true)] // all even match IDs are valid
    [TestCase(null, 102, true, true)]
    [TestCase(null, 101, false, true)]
    public async Task PlannedStart_Teams_Are_Not_Busy(DateTime? plannedStart, long matchId, bool onlyDatePart, bool expected)
    {
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { UseOnlyDatePartForTeamFreeBusyTimes = onlyDatePart };
        var match = new MatchEntity
        {
            Id = matchId, PlannedStart = plannedStart,
            PlannedEnd = plannedStart?.AddHours(2),
            HomeTeamId = 123,
            GuestTeamId = 987
        };
        _data.PlannedMatch = new()
        {
            // needed, because ...NameForRound is not part of the MatchEntity
            HomeTeamNameForRound = match.HomeTeamId.ToString(),
            GuestTeamNameForRound = match.GuestTeamId.ToString()
        };

        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);

        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartTeamsAreNotBusy, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success, Is.EqualTo(expected));
            if (!expected)
                Assert.That(factResult.Message.Contains(match.HomeTeamId.ToString()) ||
                              factResult.Message.Contains(match.GuestTeamId.ToString()), Is.True);
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase("2020-09-01 20:00:00", 101, true, false)]
    [TestCase("2020-09-01 20:00:00", 102, true, true)] // all even match IDs are valid
    [TestCase("2020-09-01 20:00:00", 101, false, false)]
    [TestCase("2020-09-01 20:00:00", 102, false, true)]
    [TestCase(null, 102, false, true)]
    [TestCase(null, 102, true, true)]
    public async Task PlannedStart_Is_Excluded_MatchDate(DateTime? plannedStart, long matchId, bool onlyDatePart, bool expected)
    {
        Thread.CurrentThread.CurrentCulture = _culture;
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { UseOnlyDatePartForTeamFreeBusyTimes = onlyDatePart };
        var match = new MatchEntity
        {
            Id = matchId,
            PlannedStart = plannedStart,
            PlannedEnd = plannedStart?.AddHours(2),
            HomeTeamId = 123,
            GuestTeamId = 987
        };
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);

        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartNotExcluded, CancellationToken.None);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Success || factResult.Message.Contains(ExcludedDateReason), Is.True);
            Assert.That(factResult.Success || StringContainsDateSeparatorBetweenApostrophes(factResult.Message, _culture), Is.True);
            Assert.That(factResult.Success || onlyDatePart ||
                          StringContainsTimeSeparatorBetweenApostrophes(factResult.Message, _culture), Is.True);
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase(1, 2, "2020-06-15 18:00:00", 101, true)] // venueId = 100 + homeTeamId,  weekday = homeTeamId % 6
    [TestCase(1, 2, "2020-06-16 19:30:00", 202, true)] // venueId = 200 + guestTeamId, weekday = guestTeamId % 6
    [TestCase(1, 2, "2020-06-16 18:00:00", 101, false)] // home venue but other weekday home team
    [TestCase(1, 2, "2020-06-17 19:30:00", 202, false)] // home venue but other weekday guest team
    [TestCase(1, 123, "2020-06-15 18:00:00", 101, true)] // guest team has same venue as home team, but different home weekdays - here home weekday fits
    [TestCase(1, 123, "2020-06-17 19:30:00", 101, true)] // guest team has same venue as home team, but different home weekdays - here guest weekday fits
    [TestCase(1, 2, "2020-06-16 18:00:00", 999, true)] // not home weekday but also not home venue
    [TestCase(1, 2, "2020-06-17 19:30:00", 999, true)] // not home weekday but also not home venue
    [TestCase(10001, 2, "2020-06-15 18:00:00", 101, true)] // homeTeamId > 10000 makes home weekday null
    [TestCase(1, 10002, "2020-06-16 19:30:00", 101, true)] // guestTeamId > 10000 makes guest weekday null
    [TestCase(1, 2, null, 101, true)]
    [TestCase(1, 2, "2020-06-15 18:00:00", null, true)]
    public async Task PlannedStart_Is_Team_Weekday(long homeTeam, long guestTeam, DateTime? plannedStart, long? venueId, bool expected)
    {
        _data.TenantConext.TournamentContext.FixtureRuleSet = new();
        var match = new MatchEntity { Id = 9999, HomeTeamId = homeTeam, GuestTeamId = guestTeam, PlannedStart = plannedStart, VenueId = venueId};
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);
        var factResult = await fv.CheckAsync(FixtureValidator.FactId.PlannedStartWeekdayIsTeamWeekday, CancellationToken.None);
        Assert.That(factResult.Success, Is.EqualTo(expected));
    }

    private static bool StringContainsDateSeparatorBetweenApostrophes(string toTest, CultureInfo culture)
    {
        // string should look like '07/07/2020' - in this case tested for '/'
        return toTest.Split('\'', StringSplitOptions.RemoveEmptyEntries)[0]
            .Contains(culture.DateTimeFormat.DateSeparator);
    }

    private static bool StringContainsTimeSeparatorBetweenApostrophes(string toTest, CultureInfo culture)
    {
        // string should look like '07/07/2020 3:15 PM' - in this case tested for ':'
        return toTest.Split('\'', StringSplitOptions.RemoveEmptyEntries)[0]
            .Contains(culture.DateTimeFormat.TimeSeparator);
    }

    [Test]
    [TestCase(null, false)]
    [TestCase(2, true)] // all even venueId are valid
    [TestCase(3, false)]
    public void Planned_Venue_Is_Set(long? venueId, bool expected)
    {
        var match = new MatchEntity { VenueId = venueId };
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { PlannedVenueMustBeSet = true};
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);
        var factResult = fv.CheckAsync(FixtureValidator.FactId.PlannedVenueIsSet, CancellationToken.None).Result;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase(null, true)]
    [TestCase(2, true)] // all even venueId are valid
    [TestCase(3, false)]
    public void Planned_Venue_Not_Occupied(long? venueId, bool expected)
    {
        var match = new MatchEntity {Id = 2, VenueId = venueId};
        _data.TenantConext.TournamentContext.FixtureRuleSet = new() { PlannedVenueMustBeSet = false };
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);
        var factResult = fv.CheckAsync(FixtureValidator.FactId.PlannedVenueNotOccupiedWithOtherMatch, CancellationToken.None).Result;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Success, Is.EqualTo(expected));
            Assert.That(factResult.Exception, Is.Null);
        }
    }

    [Test]
    [TestCase(1, 2, 101, true)] // venueId = 100 + homeTeamId
    [TestCase(1, 2, 202, true)] // venueId = 200 + guestTeamId
    [TestCase(1, 2, 333, false)]
    public void Planned_Venue_Is_Registered_For_A_Team(long homeTeam, long guestTeam, long venueId, bool expected)
    {
        _data.TenantConext.TournamentContext.FixtureRuleSet = new();
        var match = new MatchEntity { Id = 9999, HomeTeamId = homeTeam, GuestTeamId = guestTeam, VenueId = venueId};
        var fv = new FixtureValidator(match, _data, DateTime.UtcNow);
        var factResult = fv.CheckAsync(FixtureValidator.FactId.PlannedVenueIsRegisteredVenueOfTeam, CancellationToken.None).Result;
        Assert.That(factResult.Success, Is.EqualTo(expected));
    }

    [Test]
    public void FieldName_Of_Facts()
    {
        _data.TenantConext.TournamentContext.FixtureRuleSet = new();
        var fv = new FixtureValidator(new(), _data, DateTime.UtcNow);

        foreach (var fact in fv.Facts)
        {
            switch (fact.Id)
            {
                case FixtureValidator.FactId.PlannedStartIsFutureDate:
                case FixtureValidator.FactId.PlannedStartIsSet:
                case FixtureValidator.FactId.PlannedStartNotExcluded:
                case FixtureValidator.FactId.PlannedStartTeamsAreNotBusy:
                case FixtureValidator.FactId.PlannedStartWeekdayIsTeamWeekday:
                case FixtureValidator.FactId.PlannedStartWithinDesiredTimeRange:
                case FixtureValidator.FactId.PlannedStartWithinRoundLegs:
                    Assert.That(fact.FieldNames.Count() == 1 && fact.FieldNames.Contains(nameof(fv.Model.PlannedStart)), Is.True);
                    break;
                case FixtureValidator.FactId.PlannedVenueIsRegisteredVenueOfTeam:
                case FixtureValidator.FactId.PlannedVenueIsSet:
                case FixtureValidator.FactId.PlannedVenueNotOccupiedWithOtherMatch:
                    Assert.That(fact.FieldNames.Count() == 1 && fact.FieldNames.Contains(nameof(fv.Model.VenueId)), Is.True);
                    break;
            }
        }
    }
}
