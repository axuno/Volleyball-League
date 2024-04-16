using System.Security.Principal;

namespace League.Identity;

/// <summary>Represents a custom user in the AspNetCore Identity system</summary>
public class ApplicationUser : IdentityUser<long>, IIdentity
{
    public override string? UserName
    {
        get
        {
            return Name;
        }

        set
        {
            Name = value ?? string.Empty;
        }
    }

    public string Title { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public string CompleteName { get; set; } = string.Empty;
    public string Email2 { get; set; } = string.Empty;
    public string PhoneNumber2 { get; set; } = string.Empty;
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
    public string? AuthenticationType { get; set; } = null;
    public bool IsAuthenticated { get; set; } = false;
    public string Name { get; set; } = string.Empty;

    #endregion
}
