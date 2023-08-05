using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using Moq;
using TournamentManager.Data;
using TournamentManager.MultiTenancy;
using TournamentManager.ExtensionMethods;
using TournamentManager.Tests.TestComponents;
using FixtureRuleSet = TournamentManager.MultiTenancy.FixtureRuleSet;
using HomeMatchTime = TournamentManager.MultiTenancy.HomeMatchTime;
using RegularMatchStartTime = TournamentManager.MultiTenancy.RegularMatchStartTime;
using TeamRules = TournamentManager.MultiTenancy.TeamRules;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class TeamValidatorTests
{
    private readonly ITenantContext _tenantContext;
#pragma warning disable IDE0052 // Remove unread private members
    private readonly AppDb _appDb; // mocked in CTOR
#pragma warning restore IDE0052 // Remove unread private members

    public TeamValidatorTests()
    {
        #region *** Mocks ***

        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        var teamRepoMock = TestMocks.GetRepo<TeamRepository>();
        teamRepoMock
            .Setup(rep => rep.TeamNameExistsAsync(It.IsAny<TeamEntity>(), It.IsAny<CancellationToken>()))
            .Callback(() => { }).Returns((TeamEntity teamEntity, CancellationToken cancellationToken) =>
            {
                return Task.FromResult(teamEntity.Id < 10 ? teamEntity.Name : null);
            });
        appDbMock.Setup(a => a.TeamRepository).Returns(teamRepoMock.Object);

        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
            
        tenantContextMock.SetupDbContext(dbContextMock);
            
        _tenantContext = tenantContextMock.Object;

        _appDb = appDbMock.Object;

        #endregion
    }

    [TestCase("Test Team Name", true)]
    [TestCase("", false)]
    [TestCase(null, false)]
    public async Task TeamName_Must_Not_Be_Empty(string teamName, bool expected)
    {
        var team = new TeamEntity {Name = teamName};
            
        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules{HomeMatchTime = new HomeMatchTime{}};
        var tv = new TeamValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamValidator.FactId.TeamNameIsSet, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.IsTrue(factResult.Enabled);
            Assert.AreEqual(expected, factResult.Success);
            Assert.IsNull(factResult.Exception);
        });
    }

    [TestCase("Test Team Name", 100, true)] // team IDs < 10 should fail
    [TestCase("Test Team Name", 1, false)]
    [TestCase(null, 1, false)]
    public async Task Unique(string teamName, long teamId, bool expected)
    {
        var team = new TeamEntity {Name = teamName, Id = teamId};

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules { HomeMatchTime = new HomeMatchTime { } };
        var tv = new TeamValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamValidator.FactId.TeamNameIsUnique, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(factResult.Enabled, teamName != null);
            Assert.AreEqual(expected, factResult.Success);
            if (factResult.Enabled && !factResult.Success) Assert.IsTrue(factResult.Message.Contains(teamName ?? string.Empty));
            Assert.IsNull(factResult.Exception);
        });
    }

    [TestCase(null, false, false)]
    [TestCase("20:00:00", false, false)]
    [TestCase(null, true, false)]
    [TestCase(null, true, true)]
    [TestCase("20:00:00", true, false)]
    [TestCase("20:00:00", true, true)]

    public async Task MatchTime_Must_Be_Set(TimeSpan? matchTime, bool isEditable, bool mustBeSet)
    {
        var team = new TeamEntity { MatchTime = matchTime, MatchDayOfWeek = 1};

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules { HomeMatchTime = new HomeMatchTime { IsEditable = isEditable, MustBeSet = mustBeSet} };
        var tv = new TeamValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamValidator.FactId.MatchDayOfWeekAndTimeIsSet, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(factResult.Enabled, isEditable && mustBeSet);
            if (factResult.Enabled)
            {
                Assert.AreEqual(matchTime.HasValue, factResult.Success);
            }
            Assert.IsNull(factResult.Exception);
        });
    }

    [Test]
    [TestCase( null, true, false)]
    [TestCase(null, false, false)]
    [TestCase("18:00:00", true , false)]
    [TestCase("19:00:00", true, true)]
    [TestCase( "20:00:00", true, true)]
    [TestCase("20:00:00", false, false)]
    [TestCase( "23:00:00", true, false)] 
    public async Task MatchTime_Within_Fixture_TimeLimit(TimeSpan startTime, bool startTimeMustBeSet, bool expected)
    {
        // Note: all times are set and compared to local time

        var team = new TeamEntity();
        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules { HomeMatchTime = new HomeMatchTime {IsEditable = true, MustBeSet = startTimeMustBeSet} };
        _tenantContext.TournamentContext.FixtureRuleSet = new FixtureRuleSet
        {
            RegularMatchStartTime = new RegularMatchStartTime
            {MinDayTime = new TimeSpan(19, 0, 0), 
                MaxDayTime = new TimeSpan(21, 0, 0)}
        };
        var tv = new TeamValidator(team, _tenantContext);

        team.MatchTime = startTime;
        var factResult = await tv.CheckAsync(TeamValidator.FactId.MatchTimeWithinRange, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected, factResult.IsChecked == startTimeMustBeSet &&
                                      factResult.Success && factResult.Message.Contains(_tenantContext.TournamentContext.FixtureRuleSet
                                          .RegularMatchStartTime.MinDayTime.ToShortTimeString()));
            Assert.IsNull(factResult.Exception);
        });
    }

    [TestCase(null, false, false, false)]
    [TestCase(null, true, false, true)]
    [TestCase(null, true, true, false)]
    [TestCase(DayOfWeek.Monday, false, false, false)]
    [TestCase(DayOfWeek.Monday, true, false, true)]
    [TestCase(DayOfWeek.Monday, true, true, true)]
    public async Task DayOfWeek_Must_Be_Set(DayOfWeek? dayOfWeek, bool isEditable, bool mustBeSet, bool expected)
    {
        var team = new TeamEntity { MatchDayOfWeek = dayOfWeek.HasValue ? (int) dayOfWeek : default(int?), MatchTime = new TimeSpan(18,0,0)};

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules
        {
            HomeMatchTime = new HomeMatchTime
            {
                IsEditable = isEditable, 
                MustBeSet = mustBeSet,
            }
        };
        var tv = new TeamValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamValidator.FactId.DayOfWeekWithinRange, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(factResult.Enabled, isEditable && mustBeSet);
            if (factResult.Enabled)
            {
                Assert.AreEqual(expected, factResult.Success);
            }
            Assert.IsNull(factResult.Exception);
        });
    }

    [TestCase(null, false, false)]
    [TestCase(null, true, false)]
    [TestCase(DayOfWeek.Monday, false, true)]
    [TestCase(DayOfWeek.Monday, true, true)]
    [TestCase(DayOfWeek.Friday, false, false)]
    [TestCase(DayOfWeek.Friday, true, false)]
    public async Task DayOfWeek_Must_Be_In_Range(DayOfWeek? dayOfWeek, bool errorIfNotInRange, bool expected)
    {
        var team = new TeamEntity { MatchDayOfWeek = dayOfWeek.HasValue ? (int)dayOfWeek : default(int?) };

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules
        {
            HomeMatchTime = new HomeMatchTime
            {
                IsEditable = true,
                MustBeSet = true,
                ErrorIfNotInDaysOfWeekRange = errorIfNotInRange,
                DaysOfWeekRange = new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday }
            }
        };
        var tv = new TeamValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamValidator.FactId.DayOfWeekWithinRange, CancellationToken.None);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(factResult.Enabled, _tenantContext.TournamentContext.TeamRuleSet.HomeMatchTime.IsEditable && _tenantContext.TournamentContext.TeamRuleSet.HomeMatchTime.MustBeSet);
            if (factResult.Enabled)
            {
                Assert.IsTrue(errorIfNotInRange ? factResult.Type == FactType.Error : factResult.Type == FactType.Warning);
                Assert.AreEqual(expected, factResult.Success);
            }
            Assert.IsNull(factResult.Exception);
        });
    }
}