using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.LinqSupportClasses.PrefetchPathAPI;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data
{
	public class RoundRepository
	{
        private static readonly ILogger _logger = AppLogging.CreateLogger<RoundRepository>();
        private readonly MultiTenancy.IDbContext _dbContext;

	    public RoundRepository(MultiTenancy.IDbContext dbContext)
	    {
	        _dbContext = dbContext;
            _logger.LogTrace("RoundRepository created.");
	    }

        public virtual async Task<MatchRuleEntity> GetMatchRuleAsync(long roundId, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            return await new LinqMetaData(da).Round.Select(r => r.MatchRule).ExecuteAsync<MatchRuleEntity>(cancellationToken);
        }

        public virtual async Task<RoundEntity?> GetRoundWithRulesAsync(long roundId, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);
            var round = await metaData.Round.Where(r => r.Id == roundId)
                .WithPath(new PathEdge<RoundEntity>(RoundEntity.PrefetchPathMatchRule), new PathEdge<RoundEntity>(RoundEntity.PrefetchPathSetRule))
                .FirstOrDefaultAsync<RoundEntity>(cancellationToken);
            return round;
        }

		public virtual async Task<List<RoundLegPeriodRow>> GetRoundLegPeriodAsync(IPredicateExpression filter,
            CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var result = (await da.FetchQueryAsync(
                new QueryFactory().RoundLegPeriod.Where(filter)
                ,cancellationToken)).ToList();
            return result;
        }

		public virtual async Task<RoundEntity?> GetRoundWithLegsAsync(long roundId, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var result = (await da.FetchQueryAsync(
                new QueryFactory().Round.Where(RoundFields.Id == roundId)
                    .WithPath(RoundEntity.PrefetchPathRoundLegs), cancellationToken)).Cast<RoundEntity>().ToList();
            return result.FirstOrDefault();
        }

		public virtual RoundEntity GetRoundWithLegs(long roundId)
		{
			var round = new RoundEntity(roundId);
            IPrefetchPath2 prefetchPathRoundLegs = new PrefetchPath2(EntityType.RoundEntity)
            {
                RoundEntity.PrefetchPathRoundLegs
            };

            using var da = _dbContext.GetNewAdapter();
            da.FetchEntity(round, prefetchPathRoundLegs);
            da.CloseConnection();
            return round;
        }

		public virtual EntityCollection<RoundEntity> GetRoundsOfTeam(long? teamId)
		{
			if (!teamId.HasValue) return new EntityCollection<RoundEntity>();

            using var da = _dbContext.GetNewAdapter();
            var rounds = (from tir in new LinqMetaData(da).TeamInRound
                where tir.TeamId == teamId
                orderby tir.RoundId
                select tir.Round);

            da.CloseConnection();
            return new EntityCollection<RoundEntity>(rounds);
        }
 
        public virtual async Task<List<RoundTeamRow>> GetRoundsWithTeamsAsync(PredicateExpression filter, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            return await da.FetchQueryAsync<RoundTeamRow>(
                new QueryFactory().RoundTeam.Where(filter), cancellationToken);
        }

        public virtual async Task<List<RoundEntity>> GetRoundsWithTypeAsync(PredicateExpression filter, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            return ((EntityCollection<RoundEntity>)  await da.FetchQueryAsync<RoundEntity>(
                new QueryFactory().Round.WithPath(RoundEntity.PrefetchPathRoundType).Where(filter), cancellationToken)).ToList();
        }
    }
}