using Microsoft.Extensions.Logging;
using SD.LLBLGen.Pro.LinqSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.Linq;

namespace TournamentManager.Data;

public class UserRoleRepository
{
    private readonly ILogger _logger = AppLogging.CreateLogger<UserRoleRepository>();
    private readonly MultiTenancy.IDbContext _dbContext;

    public UserRoleRepository(MultiTenancy.IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public virtual async Task<IList<IdentityRoleEntity>> GetUserRolesAsync(long userId, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from ur in metaData.IdentityUserRole
            where ur.UserId == userId
            select ur.IdentityRole).ToListAsync(cancellationToken);
        _logger.LogDebug("{RoleCount} found for {UserId}", result.Count, userId);

        return result;
    }

    public virtual async Task<List<UserEntity>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var result = await (from ur in metaData.IdentityUserRole
            where ur.IdentityRole.Name == roleName
            select ur.User).ToListAsync(cancellationToken);

        return result;
    }

    public virtual async Task<bool> AddUserToRoleAsync(long userId, string roleName, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);

        var role = await (from r in metaData.IdentityRole where r.Name == roleName select r).FirstOrDefaultAsync(cancellationToken);
        if (role == null) throw new ArgumentException($"Role '{roleName}' does not exist");

        var ur = new IdentityUserRoleEntity {UserId = userId, RoleId = role.Id};
        return await da.SaveEntityAsync(ur, cancellationToken);
    }

    public virtual async Task<bool> RemoveUserFromRoleAsync(long userId, string roleName, CancellationToken cancellationToken)
    {
        using var da = _dbContext.GetNewAdapter();
        var metaData = new LinqMetaData(da);
                
        var role = await (from r in metaData.IdentityRole where r.Name == roleName select r).FirstOrDefaultAsync(cancellationToken);
        if (role == null) return false;
                
        var ur = new IdentityUserRoleEntity(role.Id, userId);
        return await da.DeleteEntityAsync(ur, cancellationToken);
    }
}
