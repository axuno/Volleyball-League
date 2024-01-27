using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.ModelValidators;
using Moq;
using TournamentManager.Data;
using TournamentManager.MultiTenancy;
using TournamentManager.Tests.TestComponents;
using TeamRules = TournamentManager.MultiTenancy.TeamRules;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class TeamVenueValidatorTests
{
    private readonly ITenantContext _tenantContext;

    public TeamVenueValidatorTests()
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

        var venueRepoMock = TestMocks.GetRepo<VenueRepository>();
        venueRepoMock
            .Setup(rep => rep.IsValidVenueIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Callback(() => { }).Returns((long? theValue, CancellationToken cancellationToken) =>
            {
                return Task.FromResult(theValue is < 10);
            });

        appDbMock.Setup(a => a.TeamRepository).Returns(teamRepoMock.Object);
        appDbMock.Setup(a => a.VenueRepository).Returns(venueRepoMock.Object);

        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
            
        tenantContextMock.SetupDbContext(dbContextMock);
            
        _tenantContext = tenantContextMock.Object;

        #endregion
    }

    [TestCase(null, true, false)]
    [TestCase(1, true, true)]
    [TestCase(null, false, true)]
    [TestCase(1, false, true)]
    public async Task Venue_Is_Set_If_Required(long? venueId, bool mustBeSet, bool expected)
    {
        var team = new TeamEntity { VenueId = venueId };

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules {
            HomeVenue = new HomeVenue { MustBeSet = mustBeSet }
        };
        var tv = new TeamVenueValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamVenueValidator.FactId.VenueIsSetIfRequired, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(factResult.Exception, Is.Null);
            Assert.That(factResult.Success, Is.EqualTo(expected));
        });
    }

    [TestCase(1, true)]
    [TestCase(100, false)] // above 10 is invalid
    [TestCase(null, false)] // null is invalid
    public async Task Team_VenueId_Is_Valid(long? venueId, bool expected)
    {
        var team = new TeamEntity { VenueId = venueId };

        _tenantContext.TournamentContext.TeamRuleSet = new TeamRules {
            HomeVenue = new HomeVenue { MustBeSet = false }
        };
        var tv = new TeamVenueValidator(team, _tenantContext);

        var factResult = await tv.CheckAsync(TeamVenueValidator.FactId.VenueIsValid, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(factResult.Exception, Is.Null);
            Assert.That(factResult.Success, Is.EqualTo(expected));
        });
    }
}
