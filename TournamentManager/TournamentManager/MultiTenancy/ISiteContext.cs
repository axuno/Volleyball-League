using System.Collections.ObjectModel;

namespace TournamentManager.MultiTenancy;

public interface ISiteContext
{
    /// <summary>
    /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
    /// </summary>
    ITenant? Tenant { get; set; }

    /// <summary>
    /// Determines the position, for the <see cref="ITenant"/> within tenant lists (e.g. navigation).
    /// </summary>
    int Position { get; set; }

    /// <summary>
    /// The value of the Url segment used to identify the site. May be NULL or empty string.
    /// </summary>
    string UrlSegmentValue { get; set; }

    /// <summary>
    /// The folder name used for the site. This name must be set.
    /// It is used to located assets like views or files for a certain organization.
    /// </summary>
    string FolderName { get; set; }

    /// <summary>
    /// The cookie name used Asp.Net Identity.
    /// </summary>
    string IdentityCookieName { get; set; }

    /// <summary>
    /// The session name used for the site.
    /// </summary>
    string SessionName { get; set; }

    /// <summary>
    /// If true, the site will not be shown in the navigation menu.
    /// </summary>
    bool HideInMenu { get; set; }

    /// <summary>
    /// Email contact details for an organization.
    /// </summary>
    Collection<MailAddress> MailAddresses { get; set; }

    /// <summary>
    /// Notifications sent before and after matches.
    /// </summary>
    MatchNotifications MatchNotifications { get; set; }
}
