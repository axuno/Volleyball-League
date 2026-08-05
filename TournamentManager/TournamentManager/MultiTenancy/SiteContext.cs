using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using TournamentManager.JsonCSerializer;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// Provides website specific data.
/// </summary>
[JsonComment("Provides website specific data.")]
public class SiteContext : ISiteContext
{
    /// <summary>
    /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
    /// </summary>
    [JsonIgnore]
    public ITenant? Tenant { get; set; }

    /// <summary>
    /// Determines the position, for the <see cref="ITenant"/> within tenant lists (e.g. navigation).
    /// </summary>
    [JsonComment("The position of the tenant within tenant lists (e.g. navigation).")]
    public int Position { get; set; }

    /// <summary>
    /// The value of the Url segment used to identify the site. May be empty for the default tenant.
    /// </summary>
    [JsonComment("The Url segment used to identify the site. May be empty for the default tenant.")]
    public string UrlSegmentValue { get; set; } = string.Empty;

    /// <summary>
    /// The folder name used for a tenant. This name must be set.
    /// It is used to locate assets like views or files for a certain tenant.
    /// Use the <see cref="ITenant.Identifier"/> for tenant-specific files.
    /// </summary>
    [JsonComment("The folder name used for a tenant. This name must be set.")]
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// The cookie name used Asp.Net Identity.
    /// </summary>
    [JsonComment("The cookie name used Asp.Net Identity.")]
    public string IdentityCookieName { get; set; } = string.Empty;

    /// <summary>
    /// The session name used for the tenant.
    /// </summary>
    [JsonComment("The session name used for the tenant.")]
    public string SessionName { get; set; } = string.Empty;

    /// <summary>
    /// If true, the site will not be shown in the navigation menu.
    /// </summary>
    [JsonComment("If true, the site will not be shown in the navigation menu.")]
    public bool HideInMenu { get; set; }

    /// <inhertitdoc/>
    [JsonComment("Recipients for generated emails.")]
    public Collection<MailAddress> MailAddresses { get; set; } = [];

    /// <summary>
    /// Notifications sent before and after matches.
    /// </summary>
    [JsonComment("Notifications sent before and after matches.")]
    public MatchNotifications MatchNotifications { get; set; } = new();
}

/// <summary>
/// Category of email address.
/// </summary>
public enum MailKind
{
    None,
    ContactFrom,
    ContactTo,
    AutoMailFrom,
    AutoMailTo,
    AutoMailBcc
}

/// <summary>
/// Email display name and address.
/// </summary>
public class MailAddress
{
    /// <summary>
    /// The kind of email address.
    /// </summary>
    public MailKind Kind { get; set; } = MailKind.None;

    /// <summary>
    /// The email address of a recipient.
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The display name of a recipient.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
}


/// <summary>
/// Notifications sent before and after matches.
/// </summary>
public class MatchNotifications
{
    /// <summary>
    /// Number of days before the next match will be announced. 0 for none, negative number days.
    /// </summary>
    [JsonComment("Number of days before the next match will be announced. 0 for none, negative number days.")]
    public int DaysBeforeNextMatch { get; set; } = 0;
        
    /// <summary>
    /// Number of days to remind 1st time for missing match results. 0 for none, positive number of days.
    /// </summary>
    [JsonComment("Number of days to remind 1st time for missing match results. 0 for none, positive number of days.")]
    public int DaysForMatchResultReminder1 { get; set; } = 0;
        
    /// <summary>
    /// Number of days to remind 2nd time for missing match results. 0 for none, positive number of days.
    /// </summary>
    [JsonComment("Number of days to remind 2nd time for missing match results. 0 for none, positive number of days.")]
    public int DaysForMatchResultReminder2 { get; set; } = 0;
}
