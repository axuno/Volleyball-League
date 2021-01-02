using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.MultiTenancy;

namespace League.Identity
{
    /// <summary>
    /// This store currently is not implemented.
    /// </summary>
    public class RoleStore : IRoleStore<ApplicationRole>, IRoleClaimStore<ApplicationRole>
    {
        private readonly TournamentManager.MultiTenancy.AppDb _appDb;
        private readonly ILogger<UserStore> _logger;
        private readonly ILookupNormalizer _keyNormalizer;
        private readonly IdentityErrorDescriber _identityErrorDescriber;

        public RoleStore(ITenantContext tenantContext, ILogger<UserStore> logger, ILookupNormalizer keyNormalizer, IdentityErrorDescriber identityErrorDescriber)
        {
            _appDb = tenantContext.DbContext.AppDb;
            _logger = logger;
            _keyNormalizer = keyNormalizer;
            _identityErrorDescriber = identityErrorDescriber as MultiLanguageIdentityErrorDescriber;
        }

        public async Task<IdentityResult> CreateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!Constants.RoleName.GetAllValues<string>().Contains(role.Name))
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
                _logger.LogError($"Role name '{role.Name}' could not be created");
                return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
            }

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(ApplicationRole role, CancellationToken cancellationToken)
        {
            if (role == null)
                throw new ArgumentNullException(nameof(role));

            if (!Constants.RoleName.GetAllValues<string>().Contains(role.Name))
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
                _logger.LogError($"Role id '{role.Id}' could not be updated", e);
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
                _logger.LogError($"Role id '{role.Id}' could not be removed");
            }

            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }

        public async Task<ApplicationRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!long.TryParse(roleId ?? string.Empty, out var id))
                return null;

            var roleEntity = await _appDb.RoleRepository.GetRoleByIdAsync(id, cancellationToken);
            if (roleEntity == null) return null;

            return new ApplicationRole {Id = roleEntity.Id, Name = roleEntity.Name};
        }

        public async Task<ApplicationRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            if (normalizedRoleName == null)
                throw new ArgumentNullException(nameof(normalizedRoleName));

            var roleEntity = await _appDb.RoleRepository.GetRoleByNameAsync(normalizedRoleName, cancellationToken);
            if (roleEntity == null) return null;

            return new ApplicationRole { Id = roleEntity.Id, Name = roleEntity.Name };
        }

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
                    throw new Exception(msg);
                }

                var existingClaims = await _appDb.RoleRepository.GetRoleClaimsAsync(role.Id, cancellationToken);
                // do nothing if the new claim already exists
                if (existingClaims.Any(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)) return;

                roleEntity.IdentityRoleClaims.Add(new IdentityRoleClaimEntity { ClaimType = claim.Type, ClaimValue = claim.Value, ValueType = claim.ValueType, Issuer = claim.Issuer });

                await _appDb.GenericRepository.SaveEntityAsync(roleEntity, false, true, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error adding role claim type '{claim.Type}' to role id '{role.Id}'", e);
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
                if (claimToDelete == null) throw new Exception(msg);

                if (!await _appDb.GenericRepository.DeleteEntityAsync(claimToDelete, cancellationToken))
                {
                    throw new Exception(msg);
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
            // Nothing to dispose.
        }
    }
}
