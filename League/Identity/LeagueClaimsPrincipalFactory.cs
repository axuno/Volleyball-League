using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace League.Identity;

/// <summary>
/// Use the <see cref="LeagueClaimsPrincipalFactory"/> by adding the service <b>**after**</b> services.AddIdentity&lt;ApplicationUser, ApplicationRole&gt;:
/// services.AddScoped&lt;IUserClaimsPrincipalFactory&lt;ApplicationUser&gt;, LeagueClaimsPrincipalFactory&gt;();
/// </summary>
public class LeagueClaimsPrincipalFactory : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>
{
    /// <summary>
    /// Add claims to the <see cref="IdentityUser"/> which are not retrieved from the default factory
    /// plus from the <see cref="IRoleStore{TRole}"/> or <see cref="IUserClaimStore{TUser}"/>
    /// (if SupportsUserClaim is true).
    /// </summary>
    /// <param name="userManager"><see cref="UserManager{ApplicationUser}"/>, usually supplied by DI</param>
    /// <param name="roleManager"><see cref="RoleManager{ApplicationRole}"/>, usually supplied by DI</param>
    /// <param name="optionsAccessor"><see cref="IOptions{IdentityOptions}"/>, usually supplied by DI</param>
    /// <remarks>
    /// ASP.NET Core Identity has a <see cref="SignInManager{TUser}"/> responsible for signing users into the application.
    /// Internally it uses a <see cref="IUserClaimsPrincipalFactory{TUser}"/> to generate a <see cref="ClaimsPrincipal"/> from the user.
    /// This default implementation only includes claims for username and user id.
    /// To add additional default claims (e.g. coming from <see cref="ApplicationUser"/> properties,
    /// we can create an implementation of <see cref="IUserClaimsPrincipalFactory{TUser}"/>
    /// or derive from the default <see cref="UserClaimsPrincipalFactory{TUser,TRole}"/>.
    /// </remarks>
    public LeagueClaimsPrincipalFactory(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager,
        IOptions<IdentityOptions> optionsAccessor)
        : base(userManager, roleManager, optionsAccessor)
    {}

    /// <summary>
    /// Will be invoked by <see cref="SignInManager{TUser}"/>.
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
    {
        var claimPrincipal = await base.CreateAsync(user);

        // add custom claims
        if (claimPrincipal.Identity is ClaimsIdentity claimsIdentity)
        {
            if (!string.IsNullOrEmpty(user.Email)) claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            if (!string.IsNullOrEmpty(user.Gender)) claimsIdentity.AddClaim(new Claim(ClaimTypes.Gender, user.Gender));
            if (!string.IsNullOrEmpty(user.FirstName)) claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, user.FirstName));
            if (!string.IsNullOrEmpty(user.LastName)) claimsIdentity.AddClaim(new Claim(ClaimTypes.Surname, user.LastName));
        }

        return claimPrincipal;
    }
}