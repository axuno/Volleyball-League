using NUnit.Framework;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.Data;
using TournamentManager.ModelValidators;
using Moq;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;
using TournamentManager.Tests.TestComponents;

namespace TournamentManager.Tests.ModelValidators;

[TestFixture]
public class TeamInRoundValidatorTests
{
    private readonly ITenantContext _tenantContext;
    private readonly AppDb _appDb;

    public TeamInRoundValidatorTests()
    {
        #region *** Mocks ***

        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        var roundsRepoMock = TestMocks.GetRepo<RoundRepository>();
        roundsRepoMock
            .Setup(rep => rep.GetRoundsWithTypeAsync(It.IsAny<PredicateExpression>(), It.IsAny<CancellationToken>()))
            .Callback(() => { }).Returns((PredicateExpression filter, CancellationToken cancellationToken) =>
            {
                var tournamentId = (long)((FieldCompareValuePredicate)filter[0].Contents).Value;

                return Task.FromResult(new List<RoundEntity>([new() {Id = 1, Name = "Round 1", TournamentId = tournamentId }, new() { Id = 2, Name = "Round 2", TournamentId = tournamentId }
                ]));
            });
        appDbMock.Setup(a => a.RoundRepository).Returns(roundsRepoMock.Object);

        var tournamentRepoMock = TestMocks.GetRepo<TournamentRepository>();
        tournamentRepoMock
            .Setup(rep => rep.GetTournamentAsync(It.IsAny<PredicateExpression>(), It.IsAny<CancellationToken>()))
            .Callback(() => { }).Returns((PredicateExpression filter, CancellationToken cancellationToken) =>
            {
                var tournamentId = (long)((FieldCompareValuePredicate)filter[0].Contents).Value;
                return Task.FromResult(new TournamentEntity{Id = tournamentId, Name = $"Tournament{tournamentId}", Description = $"DescriptionTournament{tournamentId}" })!;
            });
        appDbMock.Setup(a => a.TournamentRepository).Returns(tournamentRepoMock.Object);

        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
        tenantContextMock.SetupDbContext(dbContextMock);
            
        _tenantContext = tenantContextMock.Object;
        _appDb = appDbMock.Object;

        #endregion
    }

    [TestCase(1, 1, true)]
    [TestCase(2, 2, true)]
    [TestCase(1, 3, false)] // RoundId 3 does not belong to any tournament
    [TestCase(2, 4, false)] // RoundId 4 does not belong to any tournament
    public async Task TeamInRound_RoundId_Should_Belong_To_Tournament(long teamTournamentId, long teamInRoundRoundId, bool expected)
    {
        var tournament = await _appDb.TournamentRepository.GetTournamentAsync(new(TournamentFields.Id == teamTournamentId), CancellationToken.None);
        _ = await _appDb.RoundRepository.GetRoundsWithTypeAsync(
            new(RoundFields.TournamentId == teamTournamentId), CancellationToken.None);

        var team = new TeamInRoundEntity {TeamId = 1, RoundId = teamInRoundRoundId };

        _tenantContext.TournamentContext.TeamTournamentId = teamTournamentId;
        var tv = new TeamInRoundValidator(team, (_tenantContext, teamTournamentId));

        var factResult = await tv.CheckAsync(TeamInRoundValidator.FactId.RoundBelongsToTournament, CancellationToken.None);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(factResult.Enabled, Is.True);
            Assert.That(factResult.Success, Is.EqualTo(expected));
            if (!factResult.Success) Assert.That( factResult.Message, Does.Contain(tournament!.Description));
            Assert.That(factResult.Exception, Is.Null);
        }
    }
}
