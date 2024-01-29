using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.Plan;

namespace TournamentManager.Data;

/// <summary>
/// Class for database operations for available match dates.
/// </summary>
public class AvailableMatchDateRepository
{
    private readonly ILogger<AvailableMatchDateRepository> _logger = AppLogging.CreateLogger<AvailableMatchDateRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public AvailableMatchDateRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Gets the <see cref="EntityCollection{TEntity}"/> of type <see cref="AvailableMatchDateEntity"/> for a tournament.
    /// </summary>
    /// <param name="tournamentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the <see cref="EntityCollection{TEntity}"/> of type <see cref="AvailableMatchDateEntity"/> for a tournament.</returns>
    public virtual async Task<EntityCollection<AvailableMatchDateEntity>> GetAvailableMatchDatesAsync (long tournamentId, CancellationToken cancellationToken)
    {
        var available = new EntityCollection<AvailableMatchDateEntity>();
        using var da = _dbContext.GetNewAdapter();
        var qp = new QueryParameters
        {
            CollectionToFetch = available,
            FilterToUse = AvailableMatchDateFields.TournamentId == tournamentId
        };
        await da.FetchEntityCollectionAsync(qp, cancellationToken);

        _logger.LogDebug("Fetched {count} available match dates for tournament {tournamentId}.", available.Count, tournamentId);

        return available;
    }

    /// <summary>
    /// Removes entries in AvailableMatchDates database table.
    /// </summary>
    /// <param name="tournamentId">The tournament ID.</param>
    /// <param name="clear">Which entries to delete for the tournament.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the number of deleted records.</returns>
    public virtual async Task<int> ClearAsync(long tournamentId, MatchDateClearOption clear, CancellationToken cancellationToken)
    {
        var deleted = 0;

        // tournament is always in the filter
        var filterAvailable = new RelationPredicateBucket();
        filterAvailable.PredicateExpression.Add(AvailableMatchDateFields.TournamentId == tournamentId);

        if ((clear & MatchDateClearOption.All) == MatchDateClearOption.All)
        {
            deleted = await _dbContext.AppDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                null, cancellationToken);
        }
        else if ((clear & MatchDateClearOption.OnlyAutoGenerated) == MatchDateClearOption.OnlyAutoGenerated)
        {
            filterAvailable.PredicateExpression.AddWithAnd(AvailableMatchDateFields.IsGenerated == true);
            deleted = await _dbContext.AppDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                filterAvailable, cancellationToken);
        }
        else if ((clear & MatchDateClearOption.OnlyManual) == MatchDateClearOption.OnlyManual)
        {
            filterAvailable.PredicateExpression.AddWithAnd(AvailableMatchDateFields.IsGenerated == false);
            deleted = await _dbContext.AppDb.GenericRepository.DeleteEntitiesDirectlyAsync(typeof(AvailableMatchDateEntity),
                filterAvailable, cancellationToken);
        }

        _logger.LogDebug("Deleted {deleted} available match dates for tournament {tournamentId}.", deleted, tournamentId);

        return deleted;
    }
}
