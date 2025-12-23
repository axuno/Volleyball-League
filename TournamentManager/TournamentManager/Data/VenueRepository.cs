using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.DatabaseSpecific;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data;

/// <summary>
/// Class for Venue related data selections
/// </summary>
public class VenueRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<VenueRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public VenueRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }


    /// <summary>
    /// Gets the matches (<see cref="PlannedMatchRow"/>s) for a venue, which are occupied within the given <see cref="DateTimePeriod"/> of a tournament.
    /// </summary>
    /// <param name="venueId"></param>
    /// <param name="searchPeriod"></param>
    /// <param name="tournamentId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the matches (<see cref="PlannedMatchRow"/>s) for a venue, which are occupied within the given <see cref="DateTimePeriod"/> of a tournament.</returns>
    public virtual async Task<List<PlannedMatchRow>> GetOccupyingMatchesAsync(long venueId, DateTimePeriod searchPeriod, long tournamentId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var matchIds = await (from m in metaData.Match
            where m.Round.TournamentId == tournamentId && m.VenueId == venueId && !m.IsComplete && m.PlannedStart.HasValue && m.PlannedEnd.HasValue
                  && (m.PlannedStart <= searchPeriod.End && searchPeriod.Start <= m.PlannedEnd) // overlapping periods
            select m.Id).ExecuteAsync<List<long>>(cancellationToken);

        var filter = new PredicateExpression(PlannedMatchFields.TournamentId == tournamentId);
        filter.AddWithAnd(PlannedMatchFields.Id.In(matchIds));
        return matchIds.Count > 0
            ? await new MatchRepository(_dbContext).GetPlannedMatchesAsync(filter, cancellationToken)
            : [];
    }

    public virtual async Task<bool> IsValidVenueIdAsync(long? venueId, CancellationToken cancellationToken)
    {
        var result = (await GetVenuesAsync(new PredicateExpression(VenueFields.Id.Equal(venueId)), cancellationToken)).Count == 1;
        _logger.LogDebug("Valid venue: {ValidVenue}", result);
        return result;
    }

    public virtual async Task<List<VenueEntity>> GetVenuesAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync<VenueEntity>(
            new QueryFactory().Venue.Where(filter), cancellationToken)).Cast<VenueEntity>().ToList();
    }

    /// <summary>
    /// Gets a <see cref="List{T}"/> of <see cref="VenueDistanceResultRow"/>s, which have within a maximum distance from a given location.
    /// </summary>
    /// <param name="maxDistance">The maximum distance from the location.</param>
    /// <param name="longitude">The longitude of the location to use for distance calculation.</param>
    /// <param name="latitude">The latitude of the location to use for distance calculation.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns a <see cref="List{T}"/> of <see cref="VenueDistanceResultRow"/>s, which have within a maximum distance from a given location.</returns>
    public virtual async Task<List<VenueDistanceResultRow>> GetVenuesForDistanceAsync(double maxDistance, double longitude, double latitude, CancellationToken cancellationToken)
    {
        /* DANGER ZONE: This query returns the same result, BUT DOES NOT respect schema/catalog overwrites.
           The adapter has to be passed, otherwise the default adapter will be created
           see https://www.llblgen.com/TinyForum/Messages.aspx?ThreadID=26767
        await da.FetchProjectionAsync<VenueDistanceResultRow>(
                RetrievalProcedures.GetQueryForVenueDistanceResultTypedView(maxDistance, latitude, longitude), cancellationToken);
        */
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchProjectionAsync<VenueDistanceResultRow>(
            RetrievalProcedures.GetVenueDistanceCallAsQuery(maxDistance, latitude, longitude, da), cancellationToken);
    }

    public virtual async Task<List<VenueTeamRow>> GetVenueTeamRowsAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return await da.FetchQueryAsync<VenueTeamRow>(
            new QueryFactory().VenueTeam.Where(filter), cancellationToken);
    }
}
