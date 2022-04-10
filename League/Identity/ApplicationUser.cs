using System;
using System.Security.Principal;
using Microsoft.AspNetCore.Identity;

namespace League.Identity
{
    /// <summary>Represents a custom user in the AspNetCore Identity system</summary>
    public class ApplicationUser : IdentityUser<long>, IIdentity
    {
        public override string UserName
        {
            get
            {
                return Name;
            }

            set
            {
                Name = value;
            }
        }

        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Nickname { get; set; }
        public string Gender { get; set; }
        public string CompleteName { get; set; }
        public string Email2 { get; set; }
        public string PhoneNumber2 { get; set; }
        public DateTime? EmailConfirmedOn { get; set; }
        public override bool EmailConfirmed
        {
            get
            {
                return EmailConfirmedOn.HasValue;
            }

            set
            {
                EmailConfirmedOn = value ? DateTime.UtcNow : default(DateTime?);
            }
        }
        public DateTime? PhoneNumberConfirmedOn { get; set; }
        public override bool PhoneNumberConfirmed
        {
            get
            {
                return PhoneNumberConfirmedOn.HasValue;
            }

            set
            {
                PhoneNumberConfirmedOn = value ? DateTime.UtcNow : default(DateTime?);
            }
        }
        public DateTime ModifiedOn { get; internal set; }

        #region ** IIdentity **
        public string AuthenticationType { get; set; } = null;
        public bool IsAuthenticated { get; set; } = false;
        public string Name { get; set; }

        #endregion
    }
}
