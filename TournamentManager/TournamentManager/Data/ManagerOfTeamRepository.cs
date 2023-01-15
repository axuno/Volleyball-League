using System;
using System.Collections;
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
using TournamentManager.DAL.FactoryClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

/// <summary>
/// Class for Venue related data selections
/// </summary>
public class ManagerOfTeamRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<ManagerOfTeamRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public ManagerOfTeamRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<IList<UserEntity>> GetTeamManagersAsync(long teamId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from mot in metaData.ManagerOfTeam
            where mot.TeamId == teamId
            select mot.User).ToListAsync(cancellationToken);

        _logger.LogDebug("{count} team managers found", result.Count);
				
        return result;
    }

    public virtual async Task<List<UserEntity>> GetTeamManagersAsync(CancellationToken cancellation)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from mot in metaData.ManagerOfTeam
            select mot.User).ToListAsync(cancellation);
        return result;
    }

    public virtual async Task<List<long>> GetTeamIdsOfManagerAsync(long userId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from mot in metaData.ManagerOfTeam
            where mot.UserId == userId
            select mot.TeamId).ToListAsync(cancellationToken);

        return result;
    }

    public virtual async Task<IList<long>> GetManagerIdsOfTeamAsync(long teamId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from mot in metaData.ManagerOfTeam
            where mot.TeamId == teamId
            select mot.UserId).ToListAsync(cancellationToken);

        return result;
    }

    public virtual async Task<List<ManagerOfTeamEntity>> GetManagerOfTeamEntitiesAsync(PredicateExpression filter, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        return (await da.FetchQueryAsync<ManagerOfTeamEntity>(
            new QueryFactory().ManagerOfTeam.Where(filter), cancellationToken)).Cast<ManagerOfTeamEntity>().ToList();
    }
}