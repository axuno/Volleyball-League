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
    private readonly IAppDb _appDb;
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

        _logger.LogDebug("Generating excluded dates for tournament {TournamentId} with {RoundLegPeriods} round leg periods.", tournamentId, roundLegPeriods.Count);

        var minDate = roundLegPeriods.Min(leg => leg.StartDateTime);
        var maxDate = roundLegPeriods.Max(leg => leg.EndDateTime);

        // remove all existing excluded dates for the tournament
        if (removeExisting)
        {
            _logger.LogDebug("Removing existing excluded dates for tournament {TournamentId}.", tournamentId);
            var filter = new RelationPredicateBucket(ExcludeMatchDateFields.TournamentId == tournamentId);
            await _appDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(ExcludeMatchDateEntity), filter,
                cancellationToken);
        }

        var excludedDates = new EntityCollection<ExcludeMatchDateEntity>();

        _logger.LogDebug("Importing excluded dates from {MinDate} to {MaxDate} for tournament {TournamentId}.", minDate, maxDate, tournamentId);
        foreach (var record in importer.Import(new(minDate, maxDate)))
        {
            var entity = record.ToExcludeMatchDateEntity();
            entity.TournamentId = tournamentId;
            excludedDates.Add(entity);
        }

        _logger.LogDebug("Saving {ExcludedDates} excluded dates for tournament {TournamentId}.", excludedDates.Count, tournamentId);
        await _appDb.GenericRepository.SaveEntitiesAsync(excludedDates, false, false, cancellationToken);
    }
}
