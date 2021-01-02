using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.RelationClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data
{
	public class TeamRepository
	{
        private static readonly ILogger _logger = AppLogging.CreateLogger<TeamRepository>();
        private readonly MultiTenancy.IDbContext _dbContext;
	    public TeamRepository(MultiTenancy.IDbContext dbContext)
	    {
	        _dbContext = dbContext;
	    }

        public virtual async Task<List<TeamVenueRoundRow>> GetTeamVenueRoundInfoAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<TeamVenueRoundRow>(
                    new QueryFactory().TeamVenueRound.Where(filter), cancellationToken);
            }
        }

        public virtual async Task<List<TeamUserRoundRow>> GetTeamUserRoundInfosAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<TeamUserRoundRow>(
                    new QueryFactory().TeamUserRound.Where(filter), cancellationToken);
            }
        }

        public virtual async Task<List<LatestTeamTournamentRow>> GetLatestTeamTournamentAsync(IPredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<LatestTeamTournamentRow>(
                    new QueryFactory().LatestTeamTournament.Where(filter), cancellationToken);
            }
        }

        public virtual async Task<List<TeamEntity>> GetTeamEntitiesAsync(PredicateExpression filter, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                return (await da.FetchQueryAsync<TeamEntity>(
                    new QueryFactory().Team.Where(filter), cancellationToken)).Cast<TeamEntity>().ToList();
            }
        }

        public virtual async Task<TeamEntity> GetTeamEntityAsync(PredicateExpression filter, CancellationToken cancellationToken)
        {
            return (await GetTeamEntitiesAsync(filter, cancellationToken)).FirstOrDefault();
        }

        public virtual IQueryable<TeamEntity> GetTeamAsQuery(TournamentEntity t)
		{
			using (var da = _dbContext.GetNewAdapter())
			{
				var metaData = new LinqMetaData(da);

                var result = (metaData.TeamInRound.Where(tir => tir.Round.TournamentId == t.Id)
                    .Select(tir => tir.Team));
				da.CloseConnection();
				return result;
			}
		}

		/// <summary>
		/// Gets all rounds and their teams for a tournament prefetched.
		/// </summary>
		/// <param name="tournament">Tournament entity to use.</param>
		/// <example><![CDATA[
		/// RoundEntity round = tir.Where(t => t.Round.Name == "A").First().Round;
		/// TeamEntity team = tir.Where(t => t.Team.Name == "Die Unglaublichen").First().Team;
		/// RoundEntity[] rounds = tir.Select(x => x.Round).Distinct().ToArray();
		/// TeamEntity[] teamsOfRound = tir.Where(x => x.Round.Name == "F").Select(y => y.Team).ToArray();
		/// ]]></example>
		/// <returns>Returns the TeamInRoundEntity for the tournament id.</returns>
		public virtual EntityCollection<TeamInRoundEntity> GetTeamsAndRounds(TournamentEntity tournament)
		{
			IPrefetchPath2 prefetchPathTeamInRound = new PrefetchPath2(EntityType.TeamInRoundEntity);
			prefetchPathTeamInRound.Add(TeamInRoundEntity.PrefetchPathRound);
			prefetchPathTeamInRound.Add(TeamInRoundEntity.PrefetchPathTeam);

			var filter = new RelationPredicateBucket();
			filter.Relations.Add(TeamInRoundEntity.Relations.RoundEntityUsingRoundId);
			filter.PredicateExpression.Add(RoundFields.TournamentId == tournament.Id);

			IPrefetchPath2 prefetchPathTeam = new PrefetchPath2(EntityType.TeamEntity);
			prefetchPathTeam.Add(TeamEntity.PrefetchPathTeamInRounds);

			var tir = new EntityCollection<TeamInRoundEntity>();
			using (var da = _dbContext.GetNewAdapter())
			{
				da.FetchEntityCollection(tir, filter, prefetchPathTeamInRound);
				da.CloseConnection();
			}
			return tir;
		}

        public virtual async Task<bool> TeamExistsAsync(long teamId, CancellationToken cancellationToken)
        {
            return (await GetTeamNamesAsync(new PredicateExpression(TeamFields.Id == teamId), cancellationToken)).Any();
        }

		public virtual async Task<IList<string>> GetTeamNamesAsync(PredicateExpression filter, CancellationToken cancellationToken)
		{
            using (var da = _dbContext.GetNewAdapter())
            {
                return await da.FetchQueryAsync<string>(new QueryFactory().Create().Select(() => TeamFields.Name.ToValue<string>()).Where(filter), cancellationToken);
            }
		}

		/// <summary>
		/// Gets the first <see cref="TeamEntity.Name"/> from the database, which equals the sanitized representation
		/// of the team name and which does not have the same <see cref="TeamEntity.Id"/>.
		/// </summary>
		/// <param name="teamEntity">The <see cref="TeamEntity"/> to use for the query.</param>
		/// <param name="cancellationToken"></param>
		/// <returns>Returns the first <see cref="TeamEntity.Name"/> from the database, which equals the sanitized representation
        /// of the team name and which does not have the same <see cref="TeamEntity.Id"/> </returns>
		public virtual async Task<string> TeamNameExistsAsync(TeamEntity teamEntity, CancellationToken cancellationToken)
        {
            string Sanitize(string name)
            {
				var result = new System.Text.StringBuilder();
                foreach (var c in name)
                {
                    if(char.IsLetterOrDigit(c)) result.Append(char.ToUpperInvariant(c));
                }
                return result.ToString();
            }

            using (var da = _dbContext.GetNewAdapter())
            {
                var teamNames = await da.FetchQueryAsync<string>(new QueryFactory().Create().Select(() => TeamFields.Name.ToValue<string>()).Where(TeamFields.Id != teamEntity.Id), cancellationToken);
                return teamNames.FirstOrDefault(tn => Sanitize(tn).Equals(Sanitize(teamEntity.Name)));
            }
		}
	}
}