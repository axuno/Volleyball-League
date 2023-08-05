using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

public class UserLoginRepository
{
    private static readonly ILogger _logger = AppLogging.CreateLogger<UserLoginRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public UserLoginRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<List<IdentityUserLoginEntity>> GetUserLoginsAsync(long userId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
        var result = await (from login in metaData.IdentityUserLogin where login.UserId == userId select login).ExecuteAsync<EntityCollection<IdentityUserLoginEntity>>(cancellationToken);
        _logger.LogDebug("{userLoginCount} found for {userId}", result.Count, userId);
        return result.ToList();
    }

    public virtual async Task<UserEntity?> GetUserByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
        var result = await
            (from login in metaData.IdentityUserLogin
                where login.LoginProvider == loginProvider && login.ProviderKey == providerKey
                select login.User).FirstOrDefaultAsync(cancellationToken);
        return result;
    }
}
