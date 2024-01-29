using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Data;
using TournamentManager.Plan;
using TournamentManager.Tests.TestComponents;
using Moq;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Plan;

[TestFixture]
internal class ExcludeMatchDatesTests
{
    private ITenantContext _tenantContext = new TenantContext();
    private readonly EntityCollection<ExcludeMatchDateEntity> _excludeMatchDays = new();

    [Test]
    public async Task Generate_Schedule_Should_Succeed()
    {
        var excludeMatchDates = GetExcludeMatchDatesInstance();
        await excludeMatchDates.GenerateExcludeDates(new ExcludeMatchDatesTestImporter(), 1, true, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(_excludeMatchDays, Has.Count.EqualTo(1));
            Assert.That(_excludeMatchDays[0].TournamentId, Is.EqualTo(1));
            Assert.That(_excludeMatchDays[0].DateFrom, Is.EqualTo(new DateTime(2024, 1, 1)));
            Assert.That(_excludeMatchDays[0].DateTo, Is.EqualTo(new DateTime(2024, 1, 1).AddDays(1).AddMinutes(-1)));
            Assert.That(_excludeMatchDays[0].Reason, Is.EqualTo("Any Reason"));
        });
    }

    private ExcludeMatchDates GetExcludeMatchDatesInstance()
    {
        var tenantContextMock = TestMocks.GetTenantContextMock();
        var appDbMock = TestMocks.GetAppDbMock();

        #region ** RoundRepository mocks setup **

        var roundRepoMock = TestMocks.GetRepo<RoundRepository>();
        roundRepoMock.Setup(rep =>
                rep.GetRoundLegPeriodAsync(It.IsAny<PredicateExpression>(), It.IsAny<CancellationToken>()))
            .Returns((IPredicateExpression filter, CancellationToken cancellationToken) =>
            {
                var roundLegRow = new List<RoundLegPeriodRow> {
                    new() {
                        StartDateTime = new DateTime(2024, 1, 1),
                        EndDateTime = new DateTime(2024, 6, 30)
                    }
                };

                return Task.FromResult(roundLegRow);
            });
        appDbMock.Setup(a => a.RoundRepository).Returns(roundRepoMock.Object);

        #endregion

        #region ** GenericRepository mocks setup **

        var genericRepoMock = TestMocks.GetRepo<GenericRepository>();
        genericRepoMock.Setup(rep =>
                rep.SaveEntitiesAsync(It.IsAny<EntityCollection<ExcludeMatchDateEntity>>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .Callback(() => { })
            .Returns((EntityCollection<ExcludeMatchDateEntity> dates, bool refetchAfterSave, bool recursion, CancellationToken cancellationToken) =>
            {
                foreach (var emd in dates)
                {
                    emd.IsDirty = false;
                    emd.IsNew = false;
                }
                _excludeMatchDays.AddRange(dates);
                return Task.CompletedTask;
            });
        genericRepoMock.Setup(rep =>
                rep.DeleteEntitiesDirectlyAsync(It.IsAny<Type>(), It.IsAny<IRelationPredicateBucket>(), It.IsAny<CancellationToken>()))
            .Callback(() => { })
            .Returns((Type type, IRelationPredicateBucket bucket, CancellationToken cancellationToken) =>
            {
                if (type == typeof(ExcludeMatchDateEntity))
                {
                    var count = _excludeMatchDays.Count;
                    _excludeMatchDays.Clear();
                    return Task.FromResult(count);
                }
                throw new ArgumentException("Type not supported");
            });
        appDbMock.Setup(a => a.GenericRepository).Returns(genericRepoMock.Object);

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

        // Create ExcludeMatchDates instance
        var logger = NullLogger<ExcludeMatchDates>.Instance;
        _tenantContext = tenantContextMock.Object;

        var excludeMatchDates = new ExcludeMatchDates(_tenantContext, logger);
        return excludeMatchDates;
    }
}

internal class ExcludeMatchDatesTestImporter : IExcludeDateImporter
{
    public IEnumerable<ExcludeDateRecord> Import(DateTimePeriod dateLimits)
    {
        yield return new ExcludeDateRecord
        {
            Period = new DateTimePeriod(dateLimits.Start, dateLimits.Start?.AddDays(1).AddMinutes(-1)),
            Reason = "Any Reason"
        };
    }
}
