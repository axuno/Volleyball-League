using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace League.Identity
{
    /// <summary>
    /// Settings to be used by <see cref="LeagueUserValidator{TUser}"/>
    /// </summary>
    public class LeagueUserValidatorOptions
    {
        /// <summary>
        /// Minimum username length, defaults to 6 characters.
        /// </summary>
        public int RequiredUsernameLength { get; set; } = 6;
    }

    /// <summary>
    /// The <see cref="LeagueUserValidator{TUser}"/> is designed for additional validation besides
    /// the Identity default validator.
    /// </summary>
    /// <remarks>
    /// This custom validator must be added besides (!) Identity default validator which is still needed for other validations
    /// like email or already existing usernames. To replace the Identity default validator we could code:
    /// services.Replace(ServiceDescriptor.Scoped&lt;IUserValidator&lt;ApplicationUser&gt;, LeagueUserValidator&lt;ApplicationUser&gt;&gt;>());
    /// MultiLanguageIdentityErrorDescriber must be added to .AddIdentity() in StartUp
    /// </remarks>
    /// <typeparam name="TUser"></typeparam>
    public class LeagueUserValidator<TUser> : IUserValidator<TUser> where TUser : ApplicationUser
    {
        public LeagueUserValidator(IOptions<LeagueUserValidatorOptions> serviceConfig, IdentityErrorDescriber describer)
        {
            // errors is expected to be of type MultiLanguageIdentityErrorDescriber
            Describer = describer as MultiLanguageIdentityErrorDescriber;
            RequiredUsernameLength = serviceConfig.Value.RequiredUsernameLength;
        }

        /// <summary>
        /// The error describer takes care of text error messages.
        /// </summary>
        public MultiLanguageIdentityErrorDescriber Describer { get; }

        /// <summary>
        /// The minimum length of a username.
        /// </summary>
        public int RequiredUsernameLength { get; set; }
        
        ///<inheritdoc/>
        public virtual async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user)
        {
            _ = manager ?? throw new ArgumentNullException(nameof(manager));
            _ = user ?? throw new ArgumentNullException(nameof(user));
            
            var errors = new List<IdentityError>();
            await ValidateUserName(manager, user, errors);

            return errors.Count > 0 ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success;
        }

        private async Task ValidateUserName(UserManager<TUser> manager, TUser user, ICollection<IdentityError> errors)
        {
            var userName = await manager.GetUserNameAsync(user);

            if (userName?.Length < RequiredUsernameLength)
            {
                errors.Add(Describer.UsernameTooShort());
            }
        }
    }
}
