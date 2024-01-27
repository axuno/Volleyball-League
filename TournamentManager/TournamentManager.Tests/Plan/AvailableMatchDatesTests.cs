using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
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
        var tournament = ScheduleHelper.GetTournament();
        var tournamentLeg = tournament.Rounds.First().RoundLegs.First();
        var matches = new EntityCollection<MatchEntity>();

        Assert.Multiple(() =>
        {
            Assert.That(del: async () => await availableDates.GenerateNewAsync(tournament.Rounds[0], matches, CancellationToken.None), Throws.Nothing);
            Assert.That(availableDates.GetGeneratedAndManualAvailableMatchDateDays(tournamentLeg), Has.Count.EqualTo(87));
            Assert.That(availableDates.GetGeneratedAndManualAvailableMatchDates(1, new DateTimePeriod(tournamentLeg.StartDateTime, tournamentLeg.EndDateTime), null), Has.Count.EqualTo(17));
        });
    }

    [Test]
    public async Task Clearing_Available_Dates_Should_Succeed()
    {
        var availableDates = GetAvailableMatchDatesInstance();
        Assert.That(await availableDates.ClearAsync(MatchDateClearOption.All, CancellationToken.None), Is.EqualTo(0));
    }

    private static AvailableMatchDates GetAvailableMatchDatesInstance()
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
}

