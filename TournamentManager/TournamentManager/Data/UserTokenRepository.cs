using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data
{
    public class UserTokenRepository
    {
        private static readonly ILogger _logger = AppLogging.CreateLogger<UserTokenRepository>();
        private readonly MultiTenancy.IDbContext _dbContext;

        public UserTokenRepository(MultiTenancy.IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<IdentityUserTokenEntity> GetTokenAsync(long userId, string loginProvider, string name, CancellationToken cancellationToken)
        {
            using (var da = _dbContext.GetNewAdapter())
            {
                var metaData = new LinqMetaData(da);
                var result = await
                    (from token in metaData.IdentityUserToken
                     where token.LoginProvider == loginProvider && token.Name == name
                        select token).FirstOrDefaultAsync(cancellationToken);
                return result;
            }
        }
    }
}
