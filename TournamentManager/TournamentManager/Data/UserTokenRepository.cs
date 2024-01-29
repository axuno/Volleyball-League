using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

public class UserTokenRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<UserTokenRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public UserTokenRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<IdentityUserTokenEntity?> GetTokenAsync(long userId, string loginProvider, string name, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
        var result = await
            (from token in metaData.IdentityUserToken
                where token.LoginProvider == loginProvider && token.Name == name
                select token).FirstOrDefaultAsync(cancellationToken);

        if (result != null)
            _logger.LogDebug("User Id {userId}: Token {tokenName} found for {loginProvider}", userId, name,
                loginProvider);

        return result;
    }
}