using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Importers.ExcludeDates;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Plan;

/// <summary>
/// This class manages excluded match dates.
/// </summary>
internal class ExcludeMatchDates
{
    private readonly AppDb _appDb;
    private readonly ILogger<ExcludeMatchDates> _logger;
    
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="logger"></param>
    public ExcludeMatchDates(ITenantContext tenantContext, ILogger<ExcludeMatchDates> logger)
    {
        _appDb = tenantContext.DbContext.AppDb;
        _logger = logger;
    }

    /// <summary>
    /// Generates dates which are excluded for the tournament with <paramref name="tournamentId"/>.
    /// and saves them to persistent storage. Existing entries for the tournament are removed if <paramref name="removeExisting"/> is <see langword="true"/>.
    /// </summary>
    /// <param name="importer"></param>
    /// <param name="tournamentId"></param>
    /// <param name="removeExisting"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task GenerateExcludeDates(IExcludeDateImporter importer, long tournamentId, bool removeExisting, CancellationToken cancellationToken)
    {
        var roundLegPeriods = await _appDb.RoundRepository.GetRoundLegPeriodAsync(new PredicateExpression(
                RoundLegPeriodFields.TournamentId == tournamentId),
            cancellationToken);

        _logger.LogDebug("Generating excluded dates for tournament {tournamentId} with {roundLegPeriods} round leg periods.", tournamentId, roundLegPeriods.Count);

        var minDate = roundLegPeriods.Min(leg => leg.StartDateTime);
        var maxDate = roundLegPeriods.Max(leg => leg.EndDateTime);

        // remove all existing excluded dates for the tournament
        if (removeExisting)
        {
            _logger.LogDebug("Removing existing excluded dates for tournament {tournamentId}.", tournamentId);
            var filter = new RelationPredicateBucket(ExcludeMatchDateFields.TournamentId == tournamentId);
            await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(ExcludeMatchDateEntity), filter,
                cancellationToken);
        }

        var excludedDates = new EntityCollection<ExcludeMatchDateEntity>();

        _logger.LogDebug("Importing excluded dates from {minDate} to {maxDate} for tournament {tournamentId}.", tournamentId, minDate, maxDate);
        foreach (var record in importer.Import(new DateTimePeriod(minDate, maxDate)))
        {
            var entity = record.ToExcludeMatchDateEntity();
            entity.TournamentId = tournamentId;
            excludedDates.Add(entity);
        }

        _logger.LogDebug("Saving {excludedDates} excluded dates for tournament {tournamentId}.", excludedDates.Count, tournamentId);
        await _appDb.GenericRepository.SaveEntitiesAsync(excludedDates, false, false, cancellationToken);
    }
}
