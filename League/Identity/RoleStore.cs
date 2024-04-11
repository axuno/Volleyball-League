using System.Security.Claims;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

#pragma warning disable CA2254 // Template should be a static expression
namespace League.Identity;

/// <summary>
/// This store currently is not implemented.
/// </summary>
public class RoleStore : IRoleStore<ApplicationRole>, IRoleClaimStore<ApplicationRole>
{
    private readonly IAppDb _appDb;
    private readonly ILogger<UserStore> _logger;
    private readonly ILookupNormalizer _keyNormalizer;
    private readonly IdentityErrorDescriber _identityErrorDescriber;

    public RoleStore(ITenantContext tenantContext, ILogger<UserStore> logger, ILookupNormalizer keyNormalizer, IdentityErrorDescriber identityErrorDescriber)
    {
        _appDb = tenantContext.DbContext.AppDb;
        _logger = logger;
        _keyNormalizer = keyNormalizer;
        _identityErrorDescriber = (MultiLanguageIdentityErrorDescriber) identityErrorDescriber;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!Constants.RoleName.GetAllRoleValues<string>().Contains(role.Name))
        {
            return IdentityResult.Failed(_identityErrorDescriber.InvalidRoleName(role.Name));
        }

        var roleEntity = new IdentityRoleEntity {Name = role.Name};
        try
        {
            if (await _appDb.RoleRepository.RoleNameExistsAsync(role.Name, cancellationToken))
            {
                return IdentityResult.Failed(_identityErrorDescriber.DuplicateRoleName(role.Name));
            }

            await _appDb.GenericRepository.SaveEntityAsync(roleEntity, true, false, cancellationToken);
            role.Id = roleEntity.Id;
        }
        catch (Exception)
        {
            _logger.LogError("Role name '{roleName}' could not be created", role.Name);
            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!Constants.RoleName.GetAllRoleValues<string>().Contains(role.Name))
        {
            return IdentityResult.Failed(_identityErrorDescriber.InvalidRoleName(role.Name));
        }

        try
        {
            var roleEntity = await _appDb.RoleRepository.GetRoleByIdAsync(role.Id, cancellationToken);
            if (roleEntity == null)
            {
                return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
            }

            // There is another entry with the same role name
            var existingRole = await _appDb.RoleRepository.GetRoleByNameAsync(role.Name, cancellationToken);
            if (existingRole != null && existingRole.Id != role.Id)
            {
                return IdentityResult.Failed(_identityErrorDescriber.DuplicateRoleName(role.Name));
            }

            roleEntity.Name = role.Name;
            await _appDb.GenericRepository.SaveEntityAsync(roleEntity, false, false, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Role id '{roleId}' could not be updated", role.Id);
            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            if (await _appDb.GenericRepository.DeleteEntityAsync(new IdentityRoleEntity(role.Id), cancellationToken)                )
            {
                return IdentityResult.Success;
            }
        }
        catch (Exception)
        {
            _logger.LogError("Role id '{roleId}' could not be removed", role.Id);
        }

        return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
    }
#nullable disable annotations
    /// <summary>
    /// Returns the <see cref="ApplicationRole"/> for the <paramref name="roleId"/> if found, else <see langword="null"/>.
    /// </summary>
    public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!long.TryParse(roleId, out var id))
            return null;

        var roleEntity = await _appDb.RoleRepository.GetRoleByIdAsync(id, cancellationToken);
        if (roleEntity == null) return null;

        return new ApplicationRole {Id = roleEntity.Id, Name = roleEntity.Name};
    }

    /// <summary>
    /// Returns the <see cref="ApplicationRole"/> for the <paramref name="normalizedRoleName"/> if found, else <see langword="null"/>.
    /// </summary>
    public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
    {
        if (normalizedRoleName == null)
            throw new ArgumentNullException(nameof(normalizedRoleName));

        var roleEntity = await _appDb.RoleRepository.GetRoleByNameAsync(normalizedRoleName, cancellationToken);
        if (roleEntity == null) return null;

        return new ApplicationRole { Id = roleEntity.Id, Name = roleEntity.Name };
    }
#nullable enable annotations
    public Task<string> GetNormalizedRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return Task.FromResult(_keyNormalizer.NormalizeName(role.Name));
    }

    public Task SetNormalizedRoleNameAsync(ApplicationRole role, string normalizedName, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task<string> GetRoleIdAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return Task.FromResult(role.Id.ToString());
    }

    public Task<string> GetRoleNameAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        return Task.FromResult(role.Name);
    }

    public Task SetRoleNameAsync(ApplicationRole role, string roleName, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        role.Name = roleName ?? throw new ArgumentNullException(nameof(roleName));
        return Task.CompletedTask;
    }

    public async Task<IList<Claim>> GetClaimsAsync(ApplicationRole role, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        var claimEntities = await _appDb.RoleRepository.GetRoleClaimsAsync(role.Id, cancellationToken);
        return claimEntities.Select(claimEntity => new Claim(claimEntity.ClaimType, claimEntity.ClaimValue, claimEntity.ValueType, claimEntity.Issuer)).ToList();
    }

    public async Task AddClaimAsync(ApplicationRole role, Claim claim, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        try
        {
            var roleEntity = await _appDb.RoleRepository.GetRoleByIdAsync(role.Id, cancellationToken);
            if (roleEntity == null)
            {
                var msg = $"Role Id {role.Id} does not exist";
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }

            var existingClaims = await _appDb.RoleRepository.GetRoleClaimsAsync(role.Id, cancellationToken);
            // do nothing if the new claim already exists
            if (existingClaims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)) return;

            roleEntity.IdentityRoleClaims.Add(new IdentityRoleClaimEntity { ClaimType = claim.Type, ClaimValue = claim.Value, ValueType = claim.ValueType, Issuer = claim.Issuer });

            await _appDb.GenericRepository.SaveEntityAsync(roleEntity, false, true, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding role claim type '{claimType}' to role id '{roleId}'", claim.Type, role.Id);
            throw;
        }
    }

    public async Task RemoveClaimAsync(ApplicationRole role, Claim claim, CancellationToken cancellationToken)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        var msg = $"Error removing role claim type '{claim.Type}' from role id '{role.Id}'";
        try
        {
            var claimEntities = await _appDb.RoleRepository.GetRoleClaimsAsync(role.Id, cancellationToken);
            var claimToDelete = claimEntities.FirstOrDefault(ce => ce.ClaimType == claim.Type && ce.ClaimValue == claim.Value);
            if (claimToDelete == null) throw new InvalidOperationException(msg);

            if (!await _appDb.GenericRepository.DeleteEntityAsync(claimToDelete, cancellationToken))
            {
                throw new InvalidOperationException(msg);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(msg, e);
            throw;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore CA2254 // Template should be a static expression
