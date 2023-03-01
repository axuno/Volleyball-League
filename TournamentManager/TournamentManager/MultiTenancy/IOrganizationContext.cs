#nullable enable

namespace TournamentManager.MultiTenancy;

public interface IOrganizationContext
{
    /// <summary>
    /// The <see cref="ITenant"/> corresponding to this context.
    /// </summary>
    ITenant? Tenant { get; set; }

    /// <summary>
    /// The full name of the organization.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// The short version of the organization's name.
    /// </summary>
    string ShortName { get; set; }

    /// <summary>
    /// A description of the organization.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// The homepage for the organization. E.g. used in emails, ics-calendars
    /// </summary>
    string HomepageUrl { get; set; }

    /// <summary>
    /// Bank details of the organization, e.g. for payments of participation fees.
    /// </summary>
    BankDetails Bank { get; set; }
}