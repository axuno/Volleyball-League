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

namespace TournamentManager.Tests.Plan;

[TestFixture]
internal class AvailableMatchDatesTests
{
    [Test]
    public void Generate_Available_Dates_Should_Succeed()
    {
        var availableDates = GetAvailableMatchDatesInstance();
        var tournament = GetTournament();
        var tournamentLeg = tournament.Rounds.First().RoundLegs.First();
        var matches = new EntityCollection<MatchEntity>();

        Assert.Multiple(() =>
        {
            Assert.That(del: async () => await availableDates.GenerateNewAsync(tournament.Rounds[0], matches, CancellationToken.None), Throws.Nothing);
            Assert.That(availableDates.GetGeneratedAndManualAvailableMatchDateDays(tournamentLeg).Count, Is.EqualTo(87));
            Assert.That(availableDates.GetGeneratedAndManualAvailableMatchDates(1, new DateTimePeriod(tournamentLeg.StartDateTime, tournamentLeg.EndDateTime), null).Count, Is.EqualTo(17));
        });
    }

    [Test]
    public async Task Clearing_Available_Dates_Should_Succeed()
    {
        var availableDates = GetAvailableMatchDatesInstance();
        Assert.That(await availableDates.ClearAsync(MatchDateClearOption.All, CancellationToken.None), Is.EqualTo(0));
    }

    private AvailableMatchDates GetAvailableMatchDatesInstance()
    {
        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

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

        // Create AvailableMatchDates instance
        var logger = NullLogger<AvailableMatchDates>.Instance;
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default), "Europe/Berlin",
            CultureInfo.CurrentCulture,
            NodaTime.TimeZones.Resolvers.LenientResolver);
        var availableMatchDates = new AvailableMatchDates(tenantContextMock.Object, tzConverter, logger);
        return availableMatchDates;
    }

    private TournamentEntity GetTournament()
    {
        var teams = new EntityCollection<TeamEntity>() {
            { new (1) { Venue = new VenueEntity(1){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 5, MatchTime = new TimeSpan(18, 0, 0) } },
            { new (2) { Venue = new VenueEntity(2){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 4, MatchTime = new TimeSpan(18, 30, 0) } },
            { new (3) { Venue = new VenueEntity(3){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 3, MatchTime = new TimeSpan(19, 0, 0) } },
            { new (4) { Venue = new VenueEntity(4){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 2, MatchTime = new TimeSpan(19, 30, 0) } },
            { new (5) { Venue = new VenueEntity(5){ IsDirty = false, IsNew = false }, MatchDayOfWeek = 1, MatchTime = new TimeSpan(20, 0, 0) } }
        };
        foreach (var teamEntity in teams)
        {
            teamEntity.Fields.State = EntityState.Fetched;
            teamEntity.IsNew = teamEntity.IsDirty = false;
        }

        var round = new RoundEntity(1) {
            RoundLegs = { new RoundLegEntity {Id = 1, RoundId = 1, StartDateTime = new DateTime(2024, 1, 1), EndDateTime = new DateTime(2024, 4, 30) } }, IsNew = false,IsDirty = false
        };
        
        var teamInRounds = new EntityCollection<TeamInRoundEntity> {
            new() { Round = round, Team = teams[0], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[1], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[2], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[3], IsNew = false, IsDirty = false },
            new() { Round = round, Team = teams[4], IsNew = false, IsDirty = false }
        };
        
        round.TeamInRounds.AddRange(teamInRounds);
        
        foreach (var teamInRound in teamInRounds)
        {
            teamInRound.Fields.State = EntityState.Fetched;
            teamInRound.IsNew = teamInRound.IsDirty = false;
        }
        
        var tournament = new TournamentEntity(1) {IsNew = false, IsDirty = false };
        round.Tournament = tournament;
        // Must be set to false, otherwise teams cannot be added to the collection
        round.TeamCollectionViaTeamInRound.IsReadOnly = false; 
        round.TeamCollectionViaTeamInRound.AddRange(teams);
        round.Fields.State = EntityState.Fetched;

        tournament.Rounds.Add(round);

        return tournament;
    }
}

