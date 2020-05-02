using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

namespace TournamentManager.Data
{
	/// <summary>
	/// Class for Venue related data selections
	/// </summary>
	public class VenueRepository
	{
        private static readonly ILogger _logger = AppLogging.CreateLogger<VenueRepository>();
        private readonly IDbContext _dbContext;
	    public VenueRepository(IDbContext dbContext)
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
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);

                var matchIds = await (from m in metaData.Match
                    where m.Round.TournamentId == tournamentId && m.VenueId == venueId && !m.IsComplete && m.PlannedStart.HasValue && m.PlannedEnd.HasValue
                          && (m.PlannedStart <= searchPeriod.End && searchPeriod.Start <= m.PlannedEnd) // overlapping periods
                    select m.Id).ExecuteAsync<List<long>>(cancellationToken);

                var filter = new PredicateExpression(PlannedMatchFields.TournamentId == tournamentId);
                filter.AddWithAnd(PlannedMatchFields.Id.In(matchIds));
                return matchIds.Count > 0
                    ? await new MatchRepository(_dbContext).GetPlannedMatchesAsync(filter, cancellationToken)
                    : new List<PlannedMatchRow>();
            }
        }

        /// <summary>
        /// Gets all tournament matches played at the given venue.
        /// </summary>
        /// <param name="venueId">Venue id to search for (needle). May be new.</param>
        /// <param name="searchTime">DateSegment to find out overlapping times (match date/time (or auxiliary date/time if present). Can be null.</param>
        /// <param name="tournamentId">Tournament id for which matches shall be searched (haystack).</param>
        /// <returns></returns>
        public virtual EntityCollection<MatchEntity> GetOccupyingMatches(long venueId, DateTimePeriod searchTime, long tournamentId)
		{
			using (var da = _dbContext.GetNewAdapter())
			{
				var metaData = new LinqMetaData(da);

				IQueryable<MatchEntity> matches = from m in metaData.Match
												  where m.Round.TournamentId == tournamentId && m.VenueId == venueId
				                                  select m;

				// select matches which have planned date/times overlapping with the given date/time
				if (searchTime != null)
				{
					matches = from m in matches
						where (
							  (m.PlannedStart <= searchTime.Start && m.PlannedEnd >= searchTime.Start)
							   || (m.PlannedStart <= searchTime.End && m.PlannedEnd >= searchTime.End))
						select m;
				}
				da.CloseConnection();
				return new EntityCollection<MatchEntity>(matches);
			}
		}

        public virtual async Task<bool> IsValidVenueIdAsync(long? venueId, CancellationToken cancellationToken)
        {
            return (await GetVenuesAsync(new PredicateExpression(VenueFields.Id.Equal(venueId)), cancellationToken))
                   .Count == 1;
        }

        public virtual async Task<List<VenueEntity>> GetVenuesAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return (await da.FetchQueryAsync<VenueEntity>(
                    new QueryFactory().Venue.Where(filter), cancellationToken)).Cast<VenueEntity>().ToList();
            }
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
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchProjectionAsync<VenueDistanceResultRow>(
                    RetrievalProcedures.GetVenueDistanceCallAsQuery(maxDistance, latitude, longitude, da), cancellationToken);
            }
        }

        public virtual async Task<List<VenueTeamRow>> GetVenueTeamRowsAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<VenueTeamRow>(
                    new QueryFactory().VenueTeam.Where(filter), cancellationToken);
            }
        }
	}
}