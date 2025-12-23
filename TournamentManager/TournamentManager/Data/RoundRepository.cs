using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using SD.LLBLGen.Pro.QuerySpec;
using SD.LLBLGen.Pro.QuerySpec.Adapter;
using TournamentManager.DAL;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data;

public class RoundRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<RoundRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public RoundRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger.LogDebug("RoundRepository created.");
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

    /// <summary>
    /// Gets the minimum StartDateTime of the RoundLeg entity for a Round.
    /// </summary>
    public virtual async Task<DateTime> GetRoundStartAsync(long roundId, CancellationToken cancellationToken)
    {
        var filter = new PredicateExpression(RoundFields.Id == roundId);
        return await GetMinRoundLegAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Gets the minimum StartDateTime of the RoundLeg entity across all Rounds of a Tournament.
    /// </summary>
    public virtual async Task<DateTime> GetTournamentStartAsync(long tournamentId, CancellationToken cancellationToken)
    {
        var filter = new PredicateExpression(RoundFields.TournamentId == tournamentId);
        return await GetMinRoundLegAsync(filter, cancellationToken);
    }

    /// <summary>
    /// Gets the minimum StartDateTime of the RoundLeg entity based on the provided filter.
    /// </summary>
    private async Task<DateTime> GetMinRoundLegAsync(IPredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();

        var qf = new QueryFactory();
        var q = qf.Create()
            .From(qf.RoundLeg.InnerJoin(qf.Round).On(RoundLegFields.RoundId == RoundFields.Id)
                .InnerJoin(qf.Tournament).On(RoundFields.TournamentId == TournamentFields.Id))
            .Where(filter)
            .Select(() => RoundLegFields.StartDateTime.Min().ToValue<DateTime?>());
        
        // Execute the query to fetch the results.
        return await da.FetchScalarAsync<DateTime>(q, cancellationToken);
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
        var metaData = new LinqMetaData(da);
        var round = await metaData.Round.Where(r => r.Id == roundId)
            .WithPath(new PathEdge<RoundEntity>(RoundEntity.PrefetchPathRoundLegs))
            .FirstOrDefaultAsync(cancellationToken);
        return round;
    }

    public virtual async Task<EntityCollection<RoundEntity>> GetRoundsOfTeamAsync(long? teamId, CancellationToken cancellationToken)
    {
        if (!teamId.HasValue) return [];

        using var da = _dbContext.GetNewAdapter();
        var rounds = await (from tir in new LinqMetaData(da).TeamInRound
            where tir.TeamId == teamId
            orderby tir.RoundId
            select tir.Round).ToListAsync(cancellationToken);

        return new(rounds);
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
