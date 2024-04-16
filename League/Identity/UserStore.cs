using System.Security.Claims;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;
using TournamentManager.MultiTenancy;

#pragma warning disable CA2254 // Template should be a static expression
namespace League.Identity;

/// <summary>
/// This store is only partially implemented. It supports user creation and find methods.
/// </summary>
public class UserStore : IUserStore<ApplicationUser>, IUserEmailStore<ApplicationUser>, IUserPhoneNumberStore<ApplicationUser>, IUserPasswordStore<ApplicationUser>, IUserRoleStore<ApplicationUser>, IUserClaimStore<ApplicationUser>, IUserSecurityStampStore<ApplicationUser>, IUserLoginStore<ApplicationUser>, IUserAuthenticationTokenStore<ApplicationUser>, IUserLockoutStore<ApplicationUser>
{
    private readonly IAppDb _appDb;
    private readonly ILogger<UserStore> _logger;
    private readonly ILookupNormalizer _keyNormalizer;
    private readonly IdentityErrorDescriber _identityErrorDescriber;

    public UserStore(ITenantContext tenantContext, ILogger<UserStore> logger, ILookupNormalizer keyNormalizer, IdentityErrorDescriber identityErrorDescriber)
    {
        _appDb = tenantContext.DbContext.AppDb;
        _logger = logger;
        _keyNormalizer = keyNormalizer;
        _identityErrorDescriber = (MultiLanguageIdentityErrorDescriber) identityErrorDescriber;
    }

    #region ** IUserStore **

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var userEntity = new UserEntity
        {
            UserName = user.UserName,
            Email = user.Email,
            EmailConfirmedOn = user.EmailConfirmedOn,
            Email2 = user.Email2,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmedOn = user.PhoneNumberConfirmedOn,
            PhoneNumber2 = user.PhoneNumber2,
            Guid = string.IsNullOrEmpty(user.SecurityStamp) ? Guid.NewGuid().ToString("N") : user.SecurityStamp,
            Gender = user.Gender,
            Title = user.Title,
            FirstName = user.FirstName,
            Nickname = string.IsNullOrWhiteSpace(user.Nickname) ? null : user.Nickname,
            LastName = user.LastName
        };

        try
        {
            if (string.IsNullOrWhiteSpace(userEntity.UserName))
            {
                userEntity.UserName = Guid.NewGuid().ToString("N");
            }

            if (await _appDb.UserRepository.EmailExistsAsync(userEntity.Email))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Email is unavailable" });
            }

            if (await _appDb.UserRepository.UsernameExistsAsync(userEntity.UserName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Username is unavailable"});
            }

            await _appDb.GenericRepository.SaveEntityAsync(userEntity, true, false, cancellationToken);
            user.Id = userEntity.Id;
        }
        catch (Exception)
        {
            return IdentityResult.Failed();
        }

        return IdentityResult.Success;
    }

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        try
        {
            if (await _appDb.GenericRepository.DeleteEntityAsync(new UserEntity(user.Id), cancellationToken))
            {
                return IdentityResult.Success;
            }
            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Account for user id {userId} could not be deleted", user.Id);
            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }
    }

    private Task MapUserEntityToUser(ApplicationUser user, UserEntity userEntity)
    {
        user.Id = userEntity.Id;
        user.UserName = user.Name = userEntity.UserName;
        SetNormalizedUserNameAsync(user, userEntity.UserName, CancellationToken.None);
        user.Gender = userEntity.Gender;
        user.Title = userEntity.Title;
        user.FirstName = userEntity.FirstName;
        user.LastName = userEntity.LastName;
        user.Nickname = userEntity.Nickname;
        user.CompleteName = userEntity.CompleteName;
        user.Email = userEntity.Email;
        SetNormalizedEmailAsync(user, userEntity.Email, CancellationToken.None);
        user.EmailConfirmedOn = userEntity.EmailConfirmedOn;
        user.Email2 = userEntity.Email2 ?? string.Empty;
        user.PasswordHash = userEntity.PasswordHash;
        user.PhoneNumber = userEntity.PhoneNumber ?? string.Empty;
        user.PhoneNumberConfirmedOn = userEntity.PhoneNumberConfirmedOn;
        user.PhoneNumber2 = userEntity.PhoneNumber2 ?? string.Empty;
        user.SecurityStamp = userEntity.Guid;
        user.ModifiedOn = userEntity.ModifiedOn;
        return Task.CompletedTask;
    }
#nullable disable annotations
    /// <summary>
    /// Returns the <see cref="ApplicationUser"/> for the <paramref name="userId"/> if found, else <see langword="null"/>.
    /// </summary>
    public async Task<ApplicationUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullException(nameof(userId), @"Used ID is null or empty");

        if (!long.TryParse(userId, out var id))
            return null;

        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(id, cancellationToken);
        if (userEntity == null) return null;

        var user = new ApplicationUser();
        await MapUserEntityToUser(user, userEntity);
        return user;
    }

    public async Task<ApplicationUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (string.IsNullOrEmpty(normalizedUserName))
            throw new ArgumentNullException(nameof(normalizedUserName), @"Null or empty");

        var userEntity = await _appDb.UserRepository.GetLoginUserByUserNameAsync(normalizedUserName, cancellationToken);
        if (userEntity == null) return null;

        var user = new ApplicationUser();
        await MapUserEntityToUser(user, userEntity);
        return user;
    }
#nullable enable annotations
    public Task<string?> GetNormalizedUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.NormalizedUserName);
    }

    public Task<string> GetUserIdAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.Id.ToString());
    }

    public Task<string?> GetUserNameAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.UserName);
    }

    public Task SetNormalizedUserNameAsync(ApplicationUser user, string? normalizedName,
        CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(normalizedName))
            throw new ArgumentNullException(nameof(normalizedName), @"Null or empty");

        user.NormalizedUserName = _keyNormalizer.NormalizeName(normalizedName);
        return Task.CompletedTask;
    }

    public Task SetUserNameAsync(ApplicationUser user, string? userName, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(userName))
            throw new ArgumentNullException(nameof(userName), @"Null or empty");

        user.UserName = userName;
        SetNormalizedUserNameAsync(user, userName, cancellationToken);
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }

    public async Task<IdentityResult> UpdateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var userEntity = new UserEntity
        {
            IsNew = false,
            Id = user.Id,
            Gender = user.Gender,
            Title = user.Title,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Nickname = user.Nickname,
            UserName = user.UserName,
            Email = user.Email,
            EmailConfirmedOn = user.EmailConfirmedOn,
            Email2 = user.Email2,
            PasswordHash = user.PasswordHash,
            PhoneNumber = user.PhoneNumber,
            PhoneNumberConfirmedOn = user.PhoneNumberConfirmedOn,
            PhoneNumber2 = user.PhoneNumber2,
            Guid = string.IsNullOrEmpty(user.SecurityStamp)
                ? Guid.NewGuid().ToString("N")
                : user.SecurityStamp
        };

        try
        {
            await _appDb.GenericRepository.SaveEntityAsync<UserEntity>(userEntity, true, false, cancellationToken);
            return IdentityResult.Success;
        }
        catch (Exception)
        {
            return IdentityResult.Failed(_identityErrorDescriber.DefaultError());
        }
    }
    #endregion

    #region ** IUserEmailStore **
    public Task SetEmailAsync(ApplicationUser user, string? email, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(email))
            throw new ArgumentNullException(nameof(email), @"Null or empty");

        user.Email = email;
        SetNormalizedEmailAsync(user, email, cancellationToken);
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }

    public Task<string?> GetEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.Email);
    }

    public Task<bool> GetEmailConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.EmailConfirmed);
    }

    public Task SetEmailConfirmedAsync(ApplicationUser user, bool confirmed, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.EmailConfirmed = confirmed;
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }
#nullable disable annotations
    /// <summary>
    /// Returns the <see cref="ApplicationUser"/> for the <paramref name="normalizedEmail"/> if found, else <see langword="null"/>.
    /// </summary>
    public async Task<ApplicationUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrEmpty(normalizedEmail))
            throw new ArgumentNullException(nameof(normalizedEmail), @"Null or empty");

        var userEntity = await _appDb.UserRepository.GetLoginUserByEmailAsync(normalizedEmail, cancellationToken);
        if (userEntity == null) return null;

        var user = new ApplicationUser();
        await MapUserEntityToUser(user, userEntity);
        return user;
    }
#nullable enable annotations
    public Task<string?> GetNormalizedEmailAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.NormalizedEmail);
    }

    public Task SetNormalizedEmailAsync(ApplicationUser user, string? normalizedEmail,
        CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(normalizedEmail))
            throw new ArgumentNullException(nameof(normalizedEmail), @"Null or empty");

        user.NormalizedEmail = _keyNormalizer.NormalizeEmail(normalizedEmail);
        return Task.CompletedTask;
    }
    #endregion

    #region ** IUserPhoneNumberStore **
    public Task SetPhoneNumberAsync(ApplicationUser user, string? phoneNumber, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.PhoneNumber = phoneNumber ?? string.Empty;
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }

    public Task<string?> GetPhoneNumberAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.PhoneNumber);
    }

    public Task<bool> GetPhoneNumberConfirmedAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(user.PhoneNumberConfirmed);
    }

    public Task SetPhoneNumberConfirmedAsync(ApplicationUser user, bool confirmed,
        CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(user.PhoneNumber))
        {
            user.PhoneNumberConfirmed = false;
            return Task.CompletedTask;
        }
        user.PhoneNumberConfirmed = confirmed;
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }
    #endregion

    #region ** IUserPasswordStore **
    public Task SetPasswordHashAsync(ApplicationUser user, string? passwordHash, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.PasswordHash = passwordHash;
        SetSecurityStampAsync(user, null, cancellationToken);
        return Task.CompletedTask;
    }

    public Task<string?> GetPasswordHashAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(string.IsNullOrEmpty(user.PasswordHash) ? null : user.PasswordHash);
    }

    public Task<bool> HasPasswordAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
    }
    #endregion

    #region ** IUserRoleStore **
    public async Task AddToRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentNullException(nameof(roleName), @"Null or empty");

        if (Constants.RoleName.GetTeamRelatedRoles().Contains(roleName))
        {
            var msg = $"The role name '{roleName}' cannot be added explicitly.";

            _logger.LogError(msg);
            throw new InvalidOperationException(msg);
        }

        bool success;
        var exceptionMsg = $"Role '{roleName}' could not be added for UserId '{user.Id}'";
        try
        {
            success = await _appDb.UserRoleRepository.AddUserToRoleAsync(user.Id, roleName, cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(exceptionMsg, e);
            success = false;
        }
            
        if (!success)
        {
            _logger.LogError(exceptionMsg);
            throw new InvalidOperationException(exceptionMsg);
        }
    }

    public async Task RemoveFromRoleAsync(ApplicationUser user, string roleName, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentNullException(nameof(roleName), @"Null or empty");

        if (Constants.RoleName.GetTeamRelatedRoles().Contains(roleName))
        {
            var msg = $"The role name '{roleName}' cannot be removed explicitly.";
            _logger.LogError(msg);
            throw new InvalidOperationException(msg);
        }

        bool success;
        var exceptionMsg = $"Role '{roleName}' could not be removed for UserId '{user.Id}'";
        try
        {
            success = await _appDb.UserRoleRepository.RemoveUserFromRoleAsync(user.Id, roleName, cancellationToken);
        }
        catch(Exception e)
        {
            _logger.LogError(exceptionMsg, e);
            success = false;
        }

        if (!success)
        {
            _logger.LogError(exceptionMsg);
            throw new InvalidOperationException(exceptionMsg);
        }
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken);
        if (userEntity == null)
        {
            var ex = new ArgumentException($"User id '{user.Id}' does not exist");
            _logger.LogError(ex.Message, ex);
            throw ex;
        }

        var roles = new HashSet<string>();

        // Roles coming from table relations
        if (userEntity.IsPlayer) roles.Add(Constants.RoleName.Player);
        if (userEntity.IsTeamManager) roles.Add(Constants.RoleName.TeamManager);

        // Explicitly assigned roles
        foreach (var identityRoleEntity in await _appDb.UserRoleRepository.GetUserRolesAsync(user.Id, cancellationToken))
        {
            roles.Add(identityRoleEntity.Name);
        }

        return roles.ToList();
    }

    public async Task<bool> IsInRoleAsync(ApplicationUser user, string roleName,
        CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentNullException(nameof(roleName), @"Null or empty");

        var roles = await GetRolesAsync(user, cancellationToken);
        return roles.Any(r => r.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
    }

    public async Task<IList<ApplicationUser>> GetUsersInRoleAsync(string roleName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(roleName))
            throw new ArgumentNullException(nameof(roleName), "Null or empty");

        var applicationUsers = new List<ApplicationUser>();
        List<UserEntity>? userEntities = null;

        if (roleName == Constants.RoleName.TeamManager)
            userEntities = await _appDb.ManagerOfTeamRepository.GetTeamManagersAsync(cancellationToken);
        else if (roleName == Constants.RoleName.Player)
            userEntities = await _appDb.PlayerInTeamRepository.GetPlayersAsync(cancellationToken);

        if (userEntities != null)
        {
            foreach (var userEntity in userEntities)
            {
                var applicationUser = new ApplicationUser();
                await MapUserEntityToUser(applicationUser, userEntity);
                applicationUsers.Add(applicationUser);
            }

            return applicationUsers;
        }

        userEntities = await _appDb.UserRoleRepository.GetUsersInRoleAsync(roleName, cancellationToken);
        foreach (var userEntity in userEntities)
        {
            var applicationUser = new ApplicationUser();
            await MapUserEntityToUser(applicationUser, userEntity);

            applicationUsers.Add(applicationUser);
        }

        return applicationUsers;
    }

    #endregion

    #region ** IUserClaimStore **
    public async Task<IList<Claim>> GetClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var claimEntities = await _appDb.UserClaimRepository.GetUserClaimsAsync(user.Id, cancellationToken);
        // explicit claims
        var claims = claimEntities.Select(claimEntity => new Claim(claimEntity.ClaimType, claimEntity.ClaimValue, claimEntity.ValueType, claimEntity.Issuer)).ToList();
        // team related claims: managers
        (await _appDb.ManagerOfTeamRepository.GetTeamIdsOfManagerAsync(user.Id, cancellationToken)).ForEach(tid => claims.Add(new Claim(Constants.ClaimType.ManagesTeam, tid.ToString())));
        // team related claims: players
        (await _appDb.PlayerInTeamRepository.GetTeamIdsForPlayerAsync(user.Id, cancellationToken)).ForEach(tid => claims.Add(new Claim(Constants.ClaimType.PlaysInTeam, tid.ToString()))); ;

        return claims;
    }

    public async Task AddClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (claims == null)
            throw new ArgumentNullException(nameof(claims));

        // avoid multiple enumerations
        claims = claims.ToList();

        try
        {
            var existingClaims = await GetClaimsAsync(user, cancellationToken);

            var newClaimEntities = new EntityCollection<IdentityUserClaimEntity>();
            foreach (var claim in claims)
            {
                // only add claims that do not exist already
                if (existingClaims.Any(c => c.Type == claim.Type && c.Value == claim.Value)) continue;

                if (Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(claim.Type))
                {
                    await AddTeamRelatedClaimAsync(user, claim, cancellationToken);
                    continue;
                }

                if (Constants.ClaimType.GetProgrammaticClaimTypes().Contains(claim.Type))
                {
                    var ex = new ArgumentException($"Programmatic claim type '{claim.Type}' cannot be stored.");
                    _logger.LogError(ex.Message, ex);
                    throw ex;
                }

                newClaimEntities.Add(new IdentityUserClaimEntity
                {
                    UserId = user.Id, ClaimType = claim.Type, ClaimValue = claim.Value,
                    ValueType = claim.ValueType, Issuer = claim.Issuer
                });
            }

            if (newClaimEntities.Count > 0)
            {
                await _appDb.GenericRepository.SaveEntitiesAsync(newClaimEntities, false, false, cancellationToken);
            }
        }
        catch (Exception e)
        {
            var msg = $"Claim types '{string.Join(", ", claims.Select(c => c.Type))}' could not be added";
            _logger.LogError(msg, e);
            throw;
        }
    }

    private async Task AddTeamRelatedClaimAsync(ApplicationUser user, Claim claim, CancellationToken cancellationToken)
    {
        var teamId = long.Parse(claim.Value, NumberStyles.None);
        var exists = await _appDb.TeamRepository.TeamExistsAsync(teamId, cancellationToken);
        if (!exists)
        {
            var ex = new ArgumentException($"Claim type '{claim.Type}': Team Id '{claim.Value}' does not exist");
            _logger.LogError(ex.Message, ex);
            throw ex;
        }

        var errorMsg = $"Claim type '{claim.Type}' with value '{teamId}' could not be added";

        switch (claim.Type)
        {
            case Constants.ClaimType.ManagesTeam:
                await _appDb.GenericRepository.SaveEntityAsync(
                    new ManagerOfTeamEntity {TeamId = teamId, UserId = user.Id, IsNew = true}, false, false,
                    cancellationToken);
                return;
            case Constants.ClaimType.PlaysInTeam:
                await _appDb.GenericRepository.SaveEntityAsync(
                    new PlayerInTeamEntity {TeamId = teamId, UserId = user.Id, IsNew = true}, false, false,
                    cancellationToken);
                return;
            default:
                var ex = new NotImplementedException(errorMsg);
                _logger.LogError(errorMsg, ex);
                throw ex;
        }
    }

    public async Task ReplaceClaimAsync(ApplicationUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        if (newClaim == null)
            throw new ArgumentNullException(nameof(newClaim));

        if (Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(newClaim.Type))
        {
            var msg = $"The team related claim type '{newClaim.Type}' cannot be replaced.";
            _logger.LogError(msg);
            throw new ArgumentException(msg);
        }

        if (Constants.ClaimType.GetProgrammaticClaimTypes().Contains(claim.Type)
            || Constants.ClaimType.GetProgrammaticClaimTypes().Contains(newClaim.Type))
        {
            var ex = new ArgumentException($"Programmatic claim types cannot be replaced or stored. Current claim type: '{claim.Type}'. New claim type: '{newClaim.Type}'");
            _logger.LogError(ex.Message, ex);
            throw ex;
        }

        try
        {
            var existingClaimEntities = await _appDb.UserClaimRepository.GetUserClaimsAsync(user.Id, cancellationToken);

            // get the existing claim that should be replaced
            var claimEntity = existingClaimEntities.FirstOrDefault(ce => ce.ClaimType == claim.Type && ce.ClaimValue == claim.Value);
            if (claimEntity == null) return;

            // do nothing if the new claim already exists
            if (existingClaimEntities.Any(ce => ce.ClaimType == newClaim.Type && ce.ClaimValue == newClaim.Value)) return;

            claimEntity.ClaimType = newClaim.Type;
            claimEntity.ClaimValue = newClaim.Value;
            claimEntity.ValueType = newClaim.ValueType;
            claimEntity.Issuer = newClaim.Issuer;

            await _appDb.GenericRepository.SaveEntityAsync(claimEntity, false, false, cancellationToken);
        }
        catch (Exception e)
        {
            var msg = $"Claim type {claim.Type} could not be replaced";
            _logger.LogError(msg, e);
            throw;
        }
    }

    public async Task RemoveClaimsAsync(ApplicationUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (claims == null)
            throw new ArgumentNullException(nameof(claims));

        // avoid multiple enumerations
        claims = claims.ToArray();

        if (!claims.Any()) return;
        try
        {
            var claimEntities = await _appDb.UserClaimRepository.GetUserClaimsAsync(user.Id, cancellationToken);
            var claimEntitiesToRemove = new EntityCollection<IdentityUserClaimEntity>();
            foreach (var claim in claims)
            {
                if (Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(claim.Type))
                {
                    await RemoveTeamRelatedClaimAsync(user, claim, cancellationToken);
                    continue;
                }

                if (Constants.ClaimType.GetProgrammaticClaimTypes().Contains(claim.Type))
                {
                    var ex = new ArgumentException($"Programmatic claim type '{claim.Type}' cannot be removed.");
                    _logger.LogError(ex.Message, ex);
                    throw ex;
                }

                var found = claimEntities.FirstOrDefault(ce => ce.ClaimType == claim.Type && ce.ClaimValue == claim.Value);
                if (found != null) claimEntitiesToRemove.Add(found);
            }
            await _appDb.GenericRepository.DeleteEntitiesAsync(claimEntitiesToRemove, cancellationToken);
        }
        catch (Exception e)
        {
            var msg = $"Claim types '{string.Join(", ", claims.Select(c => c.Type))}' could not be removed";
            _logger.LogError(msg, e);
            throw;
        }
    }

    private async Task RemoveTeamRelatedClaimAsync(ApplicationUser user, Claim claim, CancellationToken cancellationToken)
    {
        var teamId = long.Parse(claim.Value, NumberStyles.None);
        if (!await _appDb.TeamRepository.TeamExistsAsync(teamId, cancellationToken))
        {
            throw new ArgumentException($"Claim type '{claim.Type}': Team Id '{claim.Value}' does not exist");
        }

        switch (claim.Type)
        {
            case Constants.ClaimType.ManagesTeam:
                await _appDb.GenericRepository.DeleteEntityAsync(new ManagerOfTeamEntity(user.Id, teamId){IsNew = false}, cancellationToken);
                return;
            case Constants.ClaimType.PlaysInTeam:
                await _appDb.GenericRepository.DeleteEntityAsync(new PlayerInTeamEntity(user.Id, teamId) { IsNew = false }, cancellationToken); return;
            default:
                var msg = $"Removing team related claim type '{claim.Type}' with value '{claim.Value}' is not implemented.";
                _logger.LogError(msg);
                throw new NotImplementedException(msg);
        }
    }

    public async Task<IList<ApplicationUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
    {
        if (claim == null)
            throw new ArgumentNullException(nameof(claim));

        var userEntities = new EntityCollection<UserEntity>();

        if (Constants.ClaimType.GetTeamRelatedClaimTypes().Contains(claim.Type))
        {
            var teamId = long.Parse(claim.Value, NumberStyles.None);
            switch (claim.Type)
            {
                case Constants.ClaimType.ManagesTeam:
                    userEntities.AddRange(await _appDb.ManagerOfTeamRepository.GetTeamManagersAsync(teamId, cancellationToken));
                    break;
                case Constants.ClaimType.PlaysInTeam:
                    userEntities.AddRange(await _appDb.PlayerInTeamRepository.GetPlayersInTeamAsync(teamId, cancellationToken));
                    break;
                default:
                    var errorMsg = $"Claim type '{claim.Type}' with value '{teamId}' could not be processed";
                    var e = new NotImplementedException(errorMsg);
                    _logger.LogError(errorMsg, e);
                    throw e;
            }
        }
        else
        {
            userEntities.AddRange(await _appDb.UserClaimRepository.GetUsersForClaimAsync(claim.Type, cancellationToken));
        }

        var applicationUsers = new List<ApplicationUser>();
        foreach (var userEntity in userEntities)
        {
            var applicationUser = new ApplicationUser();
            await MapUserEntityToUser(applicationUser, userEntity);
            applicationUsers.Add(applicationUser);
        }

        return applicationUsers;
    }
    #endregion
        
    #region ** IUserSecurityStampStore **
    public Task SetSecurityStampAsync(ApplicationUser user, string? stamp, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        user.SecurityStamp = string.IsNullOrEmpty(stamp) ? Guid.NewGuid().ToString("N") : stamp;
        return Task.CompletedTask;
    }

    public Task<string?> GetSecurityStampAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(user);

        return Task.FromResult(user.SecurityStamp);
    }
    #endregion

    #region ** IUserLoginStore **
    public async Task AddLoginAsync(ApplicationUser user, UserLoginInfo login, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (login == null)
            throw new ArgumentNullException(nameof(login));

        try
        {
            var userLoginEntity = new IdentityUserLoginEntity(login.LoginProvider, login.ProviderKey, user.Id) { ProviderDisplayName = login.ProviderDisplayName };
            await _appDb.GenericRepository.SaveEntityAsync(userLoginEntity, true, false, cancellationToken);
        }
        catch (Exception e)
        {
            var msg = $"LoginInfo for provider '{login.LoginProvider}' and user id '{user.Id}' could not be added";
            _logger.LogError(msg, e);
            throw;
        }
    }

    public async Task RemoveLoginAsync(ApplicationUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(loginProvider))
            throw new ArgumentNullException(nameof(loginProvider), @"Null or empty");

        if (string.IsNullOrEmpty(providerKey))
            throw new ArgumentNullException(nameof(providerKey), @"Null or empty");

        try
        {
            await RemoveAllTokensAsync(user, loginProvider, cancellationToken);
            var userLoginEntity = new IdentityUserLoginEntity(loginProvider, providerKey, user.Id);
            await _appDb.GenericRepository.DeleteEntityAsync(userLoginEntity, cancellationToken);
        }
        catch (Exception e)
        {
            var msg = $"LoginInfo for provider '{loginProvider}' and user id '{user.Id}' could not be removed";
            _logger.LogError(msg, e);
            throw;
        }
    }

    public async Task<IList<UserLoginInfo>> GetLoginsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var logins = await _appDb.UserLoginRepository.GetUserLoginsAsync(user.Id, cancellationToken);
        var loginInfoList = new List<UserLoginInfo>();
        logins.ForEach(li => loginInfoList.Add(new UserLoginInfo(li.LoginProvider, li.ProviderKey, li.ProviderDisplayName)));
        return loginInfoList;
    }
#nullable disable annotations
    /// <summary>
    /// Returns the <see cref="ApplicationUser"/> for the <paramref name="loginProvider"/> and <paramref name="providerKey"/> if found, else <see langword="null"/>.
    /// </summary>
    public async Task<ApplicationUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(loginProvider))
            throw new ArgumentNullException(nameof(loginProvider), @"Null or empty");

        if (string.IsNullOrEmpty(providerKey))
            throw new ArgumentNullException(nameof(providerKey), @"Null or empty");

        var userEntity = await _appDb.UserLoginRepository.GetUserByLoginAsync(loginProvider, providerKey, cancellationToken);
        if (userEntity == null) return null;
            
        var user = new ApplicationUser();
        await MapUserEntityToUser(user, userEntity);
        return user;
    }
#nullable enable annotations
    #endregion

    #region ** IUserAuthenticationTokenStore **
    public async Task<string?> GetTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(loginProvider))
            throw new ArgumentNullException(nameof(loginProvider), @"Null or empty");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), @"Null or empty");

        var userTokenEntity = await _appDb.UserTokenRepository.GetTokenAsync(user.Id, loginProvider, name, cancellationToken);
        return userTokenEntity?.Value;
    }

    public async Task SetTokenAsync(ApplicationUser user, string loginProvider, string name, string? value,
        CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(loginProvider))
            throw new ArgumentNullException(nameof(loginProvider), @"Null or empty");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), @"Null or empty");

        if (string.IsNullOrEmpty(value))
            throw new ArgumentNullException(nameof(value), @"Null or empty");

        try
        {
            var userTokenEntity = await _appDb.UserTokenRepository.GetTokenAsync(user.Id, loginProvider, name, cancellationToken) ?? new IdentityUserTokenEntity();
            userTokenEntity.UserId = user.Id;
            userTokenEntity.LoginProvider = loginProvider;
            userTokenEntity.Name = name;
            userTokenEntity.Value = value;
            await _appDb.GenericRepository.SaveEntityAsync(userTokenEntity, false, false, cancellationToken);

        }
        catch (Exception e)
        {
            var msg = $"AuthenticationToken with name '{name}' for provider '{loginProvider}' and user id '{user.Id}' could not be set";
            _logger.LogError(msg, e);
            throw;
        }
    }

    public async Task RemoveTokenAsync(ApplicationUser user, string loginProvider, string name, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        if (string.IsNullOrEmpty(loginProvider))
            throw new ArgumentNullException(nameof(loginProvider), @"Null or empty");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), @"Null or empty");

        var msg = $"AuthenticationToken with name '{name}' for provider '{loginProvider}' and user id '{user.Id}' could not be removed";
        try
        {
            var userTokenEntity = new IdentityUserTokenEntity(loginProvider, name, user.Id);
            if (!await _appDb.GenericRepository.DeleteEntityAsync(userTokenEntity, cancellationToken))
            {
                _logger.LogError(msg);
                throw new InvalidOperationException(msg);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(msg, e);
            throw;
        }
    }

    private async Task<int> RemoveAllTokensAsync(ApplicationUser user, string loginProvider, CancellationToken cancellationToken)
    {
        var filter = new PredicateExpression(IdentityUserTokenFields.UserId == user.Id & IdentityUserTokenFields.LoginProvider == loginProvider);
        return await _appDb.GenericRepository.DeleteEntitiesUsingConstraintAsync<IdentityUserTokenEntity>(filter, cancellationToken);
    }

    #endregion

    #region ** IUserLockoutStore **

    public async Task<DateTimeOffset?> GetLockoutEndDateAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        cancellationToken.ThrowIfCancellationRequested();
        var userEntity = _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken).Result;
        if (userEntity?.LockoutEndDateUtc == null) return await Task.FromResult<DateTimeOffset?>(null);

        return userEntity.LockoutEndDateUtc.Value;
    }

    public async Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        cancellationToken.ThrowIfCancellationRequested();

        // do nothing if lockout is not enabled for the user
        if (!await GetLockoutEnabledAsync(user, CancellationToken.None))
            return;

        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken);
        if (userEntity == null) throw new InvalidOperationException($"No user entity found for user ID '{user.Id}'");

        if (lockoutEnd.HasValue)
            userEntity.LockoutEndDateUtc = lockoutEnd.Value.UtcDateTime;
        else
            userEntity.LockoutEndDateUtc = null;
        await _appDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
    }

    public async Task<int> IncrementAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        cancellationToken.ThrowIfCancellationRequested();

        // do nothing if lockout is not enabled for the user
        if (!await GetLockoutEnabledAsync(user, CancellationToken.None))
            return 0;

        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken);
        if (userEntity == null) throw new InvalidOperationException($"No user entity found for user ID '{user.Id}'");

        var count = ++userEntity.AccessFailedCount;
        await _appDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
        return count;
    }

    public async Task ResetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        cancellationToken.ThrowIfCancellationRequested();
        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken);
        if (userEntity == null) return;

        userEntity.AccessFailedCount = 0;
        await _appDb.GenericRepository.SaveEntityAsync(userEntity, false, false, cancellationToken);
    }

    public async Task<int> GetAccessFailedCountAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        cancellationToken.ThrowIfCancellationRequested();
        var userEntity = await _appDb.UserRepository.GetLoginUserAsync(user.Id, cancellationToken);
        return userEntity?.AccessFailedCount ?? 0;
    }

    public async Task<bool> GetLockoutEnabledAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var enabled = !await IsInRoleAsync(user, Constants.RoleName.SystemManager, cancellationToken);
        return enabled;
    }

    public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled, CancellationToken cancellationToken)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        return Task.CompletedTask;
    }
    #endregion

    public void Dispose()
    {
        // Nothing to dispose.
        GC.SuppressFinalize(this);
    }
}
#pragma warning restore CA2254 // Template should be a static expression
