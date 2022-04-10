using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data
{
    public class RoleRepository
    {
        private static readonly ILogger _logger = AppLogging.CreateLogger<RoleRepository>();
        private readonly MultiTenancy.IDbContext _dbContext;
        
        public RoleRepository(MultiTenancy.IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public virtual async Task<IdentityRoleEntity?> GetRoleByIdAsync(long id, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);
            var result = await (from r in metaData.IdentityRole
                where r.Id == id
                select r).FirstOrDefaultAsync(cancellationToken);
            return result;
        }

        public virtual async Task<IdentityRoleEntity?> GetRoleByNameAsync(string roleName, CancellationToken cancellationToken)
        {
            roleName = roleName.ToLowerInvariant();
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);
            var result = await (from r in metaData.IdentityRole
                where r.Name.ToLower() == roleName
                select r).FirstOrDefaultAsync(cancellationToken);
            return result;
        }

        /// <summary>
        /// Checks whether a role exists for the given role name.
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns true, if a role with the given role name exists, else false.</returns>
        public virtual async Task<bool> RoleNameExistsAsync(string roleName, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);
            var result = await (from r in metaData.IdentityRole
                where roleName.ToLower() == r.Name.ToLower()
                select r).FirstOrDefaultAsync(cancellationToken);

            _logger.LogDebug("{roleName} exists: {trueFalse}", roleName, result != null);
            return result != null;
        }

        public virtual async Task<IList<IdentityRoleClaimEntity>> GetRoleClaimsAsync(long roleId, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);

            var result = await (from uc in metaData.IdentityRoleClaim
                where uc.RoleId == roleId
                select uc).ToListAsync(cancellationToken);
                
            return result;
        }

        public virtual async Task<IdentityUserClaimEntity> GetUserClaimAsync(long userId, string claimType, string claimValue, CancellationToken cancellationToken)
        {
            using var da = _dbContext.GetNewAdapter();
            var metaData = new LinqMetaData(da);

            var result = await (from uc in metaData.IdentityUserClaim
                where uc.UserId == userId && uc.ClaimType == claimType && uc.ClaimValue == claimValue
                select uc).FirstOrDefaultAsync(cancellationToken);
                
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
}
