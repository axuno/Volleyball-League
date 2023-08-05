using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

public class UserClaimRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<UserClaimRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public UserClaimRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<IList<IdentityUserClaimEntity>> GetUserClaimsAsync(long userId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from uc in metaData.IdentityUserClaim
            where uc.UserId == userId
            select uc).ToListAsync(cancellationToken);
        da.CloseConnection();
        _logger.LogDebug("{claimsCount} found", result.Count);

        return result;
    }

    public virtual async Task<IdentityUserClaimEntity> GetUserClaimAsync(long userId, string claimType, string claimValue, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from uc in metaData.IdentityUserClaim
            where uc.UserId == userId && uc.ClaimType == claimType && uc.ClaimValue == claimValue
            select uc).FirstOrDefaultAsync(cancellationToken);
        da.CloseConnection();
        return result;
    }

    public virtual async Task<List<UserEntity>> GetUsersForClaimAsync(string claimType, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from uc in metaData.IdentityUserClaim
            where uc.ClaimType == claimType
            select uc.User).ToListAsync(cancellationToken);

        return result;
    }
        
}