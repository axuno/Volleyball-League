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
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL;
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;
using TournamentManager.DAL.RelationClasses;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Data;

/// <summary>
/// Class for Tournament related data selections
/// </summary>
public class TournamentRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<TournamentRepository>();
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

    public virtual EntityCollection<RoundEntity> GetTournamentRounds(long tournamentId)
    {
        using var da = _dbContext.GetNewAdapter();
        //var selectedRounds = new EntityCollection<RoundEntity>();
        var metaData = new LinqMetaData(da);

        var q = (from r in metaData.Round
            where r.TournamentId == tournamentId
            select r);

        var result = new EntityCollection<RoundEntity>(q);
        da.CloseConnection();
        return result;
    }

    public virtual async Task<TournamentEntity?> GetTournamentEntityForMatchPlannerAsync(long tournamentId, CancellationToken cancellationToken)
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
}
