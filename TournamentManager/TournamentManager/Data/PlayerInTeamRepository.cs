using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

/// <summary>
/// Class for player in team related data operations
/// </summary>
public class PlayerInTeamRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<PlayerInTeamRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public PlayerInTeamRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<IList<UserEntity>> GetPlayersInTeamAsync(long teamId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from pit in metaData.PlayerInTeam
            where pit.TeamId == teamId
            select pit.User).ToListAsync(cancellationToken);

        _logger.LogDebug("{count} player(s) found for team id {teamId}", result.Count, teamId);

        return result;
    }

    public virtual async Task<List<long>> GetTeamIdsForPlayerAsync(long userId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from pit in metaData.PlayerInTeam
            where pit.UserId == userId
            select pit.TeamId).ToListAsync(cancellationToken);

        _logger.LogDebug("{userId} team(s) found for user id {count}", userId, result.Count);

        return result;
    }

    public virtual async Task<List<UserEntity>> GetPlayersAsync(CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from pit in metaData.PlayerInTeam
            select pit.User).ToListAsync(cancellationToken);

        _logger.LogDebug("{count} player(s) found", result.Count);

        return result;
    }
}