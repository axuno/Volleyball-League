using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;
using TournamentManager.Plan;
using TournamentManager.Tests.TestComponents;
using Moq;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.Plan;

[TestFixture]
internal class MatchSchedulerTests
{
    private ITenantContext _tenantContext = new TenantContext();
    private readonly TournamentEntity? _tournamentEntityForMatchScheduler = ScheduleHelper.GetTournament();

    [Test]
    public void Generate_Schedule_Should_Succeed()
    {
        EntityCollection<MatchEntity> matches = new(); 
        var scheduler = GetMatchSchedulerInstance();
        scheduler.OnBeforeSave += (sender, fixtures) => matches = fixtures;
        var participants = _tournamentEntityForMatchScheduler!.Rounds.First().TeamCollectionViaTeamInRound;
        var expectedNumOfMatches = participants.Count * (participants.Count - 1) / 2;

        Assert.Multiple(() =>
        {
            Assert.That(del: async () => await scheduler.ScheduleFixturesForTournament(false, CancellationToken.None), Throws.Nothing);
            Assert.That(matches.Count, Is.EqualTo(expectedNumOfMatches));
            Assert.That(matches.All(m => (m.PlannedEnd!.Value - m.PlannedStart!.Value) == _tenantContext.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch), Is.True);
            Assert.That(matches.All(m => m.HomeTeamId == m.RefereeId), Is.True);
            Assert.That(matches.All(m => m.LegSequenceNo == 1), Is.True);
            Assert.That(matches.All(m => m.RoundId == 1), Is.True);
            Assert.That(matches.All(m => m.VenueId == participants.First(p => p.Id == m.HomeTeamId).VenueId), Is.True);
        });
    }

    private MatchScheduler GetMatchSchedulerInstance()
    {
        var tournamentMatches = new EntityCollection<MatchEntity>();

        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        #region ** GenericRepository mocks setup **

        var genericRepoMock = TestMocks.GetRepo<GenericRepository>();
        genericRepoMock.Setup(rep =>
                rep.SaveEntitiesAsync(It.IsAny<EntityCollection<MatchEntity>>(), true, false,It.IsAny<CancellationToken>()))
            .Returns((EntityCollection<MatchEntity> matches, bool refetchAfterSave, bool recursion, CancellationToken cancellationToken) =>
            {
                // DO NOT add matches to tournamentMatches, because
                // the same instance is returned from method GetMatches() of MatchRepository.
                // Meaning the matches already exist there.
                foreach (var matchEntity in tournamentMatches)
                {
                    matchEntity.IsDirty = false;
                    matchEntity.IsNew = false;
                }

                return Task.CompletedTask;
            });
        genericRepoMock.Setup(rep =>
                rep.DeleteEntitiesDirectlyAsync(It.IsAny<Type>(), It.IsAny<IRelationPredicateBucket>(), It.IsAny<CancellationToken>()))
            .Returns((Type type, IRelationPredicateBucket bucket, CancellationToken cancellationToken) =>
            {
                if (type == typeof(MatchEntity))
                {
                    var count = tournamentMatches.Count;
                    tournamentMatches.Clear();
                    return Task.FromResult(count);
                }
                throw new ArgumentException("Type not supported");
            });
        appDbMock.Setup(a => a.GenericRepository).Returns(genericRepoMock.Object);

        #endregion

        #region ** TournamentRepository mocks setup **

        var tournamentRepoMock = TestMocks.GetRepo<TournamentRepository>();
        tournamentRepoMock.Setup(rep =>
                rep.GetTournamentEntityForMatchSchedulerAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, CancellationToken cancellationToken) => Task.FromResult(_tournamentEntityForMatchScheduler));
        appDbMock.Setup(a => a.TournamentRepository).Returns(tournamentRepoMock.Object);

        #endregion

        #region ** MatchRepository mocks setup **

        var matchRepoMock = TestMocks.GetRepo<MatchRepository>();
        matchRepoMock.Setup(rep =>
                rep.AnyCompleteMatchesExistAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, CancellationToken cancellationToken) => Task.FromResult(false));
        matchRepoMock.Setup(rep =>
                rep.GetMatches(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, CancellationToken cancellationToken) =>
            {
               return Task.FromResult(tournamentMatches);
            });
        appDbMock.Setup(a => a.MatchRepository).Returns(matchRepoMock.Object);
        
        #endregion

        #region ** AvailableMatchDateRepository mocks setup **

        var availableMatchDatesRepoMock = TestMocks.GetRepo<AvailableMatchDateRepository>();
        availableMatchDatesRepoMock.Setup(rep =>
                rep.GetAvailableMatchDatesAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, CancellationToken cancellationToken) =>
            {
                var availableMatchDates = new EntityCollection<AvailableMatchDateEntity>();
                return Task.FromResult(availableMatchDates);
            });
        availableMatchDatesRepoMock.Setup(rep =>
            rep.ClearAsync(It.IsAny<long>(), It.IsAny<MatchDateClearOption>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, MatchDateClearOption clear, CancellationToken cancellationToken) => Task.FromResult(0));
        appDbMock.Setup(a => a.AvailableMatchDateRepository).Returns(availableMatchDatesRepoMock.Object);

        #endregion

        #region ** ExcludedMatchDateRepository mocks setup **

        var excludedMatchDatesMock = TestMocks.GetRepo<ExcludedMatchDateRepository>();
        excludedMatchDatesMock.Setup(rep =>
                           rep.GetExcludedMatchDatesAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
            .Returns((long tournamentId, CancellationToken cancellationToken) =>
            {
                var excludedMatchDates = new EntityCollection<ExcludeMatchDateEntity>();
                return Task.FromResult(excludedMatchDates);
            });
        appDbMock.Setup(a => a.ExcludedMatchDateRepository).Returns(excludedMatchDatesMock.Object);

        #endregion

        // Build complete TenantContext mock
        var dbContextMock = TestMocks.GetDbContextMock();
        dbContextMock.SetupAppDb(appDbMock);
        tenantContextMock.SetupDbContext(dbContextMock);

        // Create MatchScheduler instance
        var logger = NullLogger<MatchScheduler>.Instance;
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default), "Europe/Berlin",
            CultureInfo.CurrentCulture,
            NodaTime.TimeZones.Resolvers.LenientResolver);
        
        _tenantContext = tenantContextMock.Object;
        _tenantContext.TournamentContext.MatchPlanTournamentId = 1;
        _tenantContext.TournamentContext.FixtureRuleSet.PlannedDurationOfMatch = TimeSpan.FromHours(2);
        _tenantContext.TournamentContext.RefereeRuleSet.RefereeType = RefereeType.Home;
        var scheduler = new MatchScheduler(_tenantContext, tzConverter, NullLoggerFactory.Instance);
        return scheduler;
    }
}

