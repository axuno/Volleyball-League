using System.Data;
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

namespace TournamentManager.Data;

/// <summary>
/// Class for Tournament related data selections
/// </summary>
public class TournamentRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<TournamentRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;
    public TournamentRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
        _logger.LogDebug("Repository created. {Repository} {Identifier}", nameof(TournamentRepository), dbContext.Tenant?.Identifier);
    }

    [Obsolete("Use GetTournamentAsync instead", false)]
    public virtual async Task<TournamentEntity?> GetTournamentByIdAsync(long tournamentId, CancellationToken cancellationToken)
    {
        return await GetTournamentAsync(new PredicateExpression(TournamentFields.Id == tournamentId),
            cancellationToken);
    }

    public virtual async Task<TournamentEntity?> GetTournamentAsync(PredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync<TournamentEntity>(
            new QueryFactory().Tournament.Where(filter), cancellationToken)).Cast<TournamentEntity>().FirstOrDefault();
    }

    public virtual async Task<TournamentEntity?> GetTournamentWithRoundsAsync(long tournamentId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
        var q = (metaData.Tournament.Where(t => t.Id == tournamentId))
            .WithPath(new PathEdge<TournamentEntity>(TournamentEntity.PrefetchPathRounds,
                new IPathEdge[] {new PathEdge<RoundEntity>(RoundEntity.PrefetchPathRoundType)}));

        return (await q.ExecuteAsync<IList<TournamentEntity>>(cancellationToken)).FirstOrDefault();
    }

    public virtual async Task<EntityCollection<RoundEntity>> GetTournamentRoundsAsync(long tournamentId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var q = await (from r in metaData.Round
            where r.TournamentId == tournamentId
            select r).ToListAsync(cancellationToken);

        var result = new EntityCollection<RoundEntity>(q);
        return result;
    }

    public virtual async Task<List<long>> GetTournamentRoundIdsAsync(long tournamentId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from r in metaData.Round
            where r.TournamentId == tournamentId
            select r.Id).ToListAsync(cancellationToken);

        return result;
    }

    public virtual async Task<TournamentEntity?> GetTournamentEntityForMatchSchedulerAsync(long tournamentId, CancellationToken cancellationToken)
    {
        var bucket = new RelationPredicateBucket(TournamentFields.Id == tournamentId);
        bucket.Relations.Add(TournamentEntity.Relations.RoundEntityUsingTournamentId);
        bucket.Relations.Add(RoundEntity.Relations.TeamInRoundEntityUsingRoundId);
        bucket.Relations.Add(TeamInRoundEntity.Relations.TeamEntityUsingTeamId);
        bucket.Relations.Add(TeamEntity.Relations.VenueEntityUsingVenueId);

        var prefetchPathTournament = new PrefetchPath2(EntityType.TournamentEntity)
            { TournamentEntity.PrefetchPathRounds };
        prefetchPathTournament[0].SubPath.Add(RoundEntity.PrefetchPathRoundLegs);
        prefetchPathTournament[0].SubPath.Add(RoundEntity.PrefetchPathTeamCollectionViaTeamInRound).SubPath
            .Add(TeamEntity.PrefetchPathVenue);

        var t = new EntityCollection<TournamentEntity>();
        using var da = _dbContext.GetNewAdapter();
        var qp = new QueryParameters(0, 0, 0, bucket)
        {
            CollectionToFetch = t,
            PrefetchPathToUse = prefetchPathTournament
        };
        await da.FetchEntityCollectionAsync(qp, cancellationToken);
        return t.FirstOrDefault();
    }

    internal virtual async Task<bool> SaveTournamentsAsync(TournamentEntity sourceTournament, TournamentEntity targetTournament, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();

        try
        {
            await da.StartTransactionAsync(IsolationLevel.ReadCommitted,
                string.Concat(nameof(SaveTournamentsAsync), Guid.NewGuid().ToString("N")), cancellationToken);

            await da.SaveEntityAsync(targetTournament, true, true, cancellationToken);

            if (sourceTournament.NextTournamentId is null)
            {
                sourceTournament.NextTournamentId = targetTournament.Id;
                sourceTournament.ModifiedOn = targetTournament.ModifiedOn;
                await da.SaveEntityAsync(sourceTournament, true, false, cancellationToken);
                _logger.LogDebug("{Property} set to {NextTournamentId}", nameof(TournamentEntity.NextTournamentId), sourceTournament.NextTournamentId);
            }
            else
            {
                _logger.LogDebug("{Property} was already set to {NextTournamentId}", nameof(TournamentEntity.NextTournamentId), sourceTournament.NextTournamentId);
            }

            await da.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving transaction for tournament IDs {TargetId} and {SourceId}",
                targetTournament.Id, sourceTournament.Id);

            if (da.IsTransactionInProgress)
                da.Rollback();

            return false;
        }
    }
}
