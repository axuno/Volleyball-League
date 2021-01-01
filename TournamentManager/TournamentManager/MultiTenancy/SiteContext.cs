#nullable enable
namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// Provides site-specific data.
    /// </summary>
    public class SiteContext : ISiteContext
    {
        /// <summary>
        /// The <see cref="TournamentManager.MultiTenancy.ITenant"/> corresponding to this context.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public ITenant? Tenant { get; set; }

        /// <summary>
        /// The value of the Url segment used to identify the site. May be empty for the default tenant.
        /// </summary>
        [YAXLib.YAXComment("The Url segment used to identify the site. May be empty for the default tenant.")]
        public string UrlSegmentValue { get; set; } = string.Empty;
        /// <summary>
        /// The folder name used for a tenant. This name must be set.
        /// It is used to locate assets like views or files for a certain tenant.
        /// Use the <see cref="ITenant.Identifier"/> for tenant-specific files.
        /// </summary>
        [YAXLib.YAXComment("The folder name used for a tenant. This name must be set.")]
        public string FolderName { get; set; } = string.Empty;
        /// <summary>
        /// The cookie name used Asp.Net Identity.
        /// </summary>
        [YAXLib.YAXComment("The cookie name used Asp.Net Identity.")]
        public string IdentityCookieName { get; set; } = string.Empty;
        /// <summary>
        /// The session name used for the tenant.
        /// </summary>
        [YAXLib.YAXComment("The session name used for the tenant.")]
        public string SessionName { get; set; } = string.Empty;
        /// <summary>
        /// If true, the site will not be shown in the navigation menu.
        /// </summary>
        [YAXLib.YAXComment("If true, the site will not be shown in the navigation menu.")]
        public bool HideInMenu { get; set; }
        /// <summary>
        /// Location for photos.
        /// </summary>
        public Photos Photos { get; set; } = new Photos();
        /// <summary>
        /// Email contact details for an organization.
        /// </summary>
        public Email Email { get; set; } = new Email();
    }
    
    public class Photos
    {
        /// <summary>
        /// The folder for team photos.
        /// </summary>
        [YAXLib.YAXComment("The folder for team photos.")]
        public string TeamPhotoFolder { get; set; } = string.Empty;

        /// <summary>
        /// The Folder for photos of people.
        /// </summary>
        [YAXLib.YAXComment("The folder for photos of people.")]
        public string PeoplePhotoFolder { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Email contact details for an organization.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// "From" mailbox address for the contact form
        /// </summary>
        [YAXLib.YAXComment("")]
        public MailAddress ContactFrom { get; set; } = new MailAddress();
        /// <summary>
        /// "To" mailbox address for the contact form
        /// </summary>
        [YAXLib.YAXComment("\"From\" mailbox address for the contact form")]
        public MailAddress ContactTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "To" mailbox address for emails generated programmatically
        /// </summary>
        [YAXLib.YAXComment("General \"To\" mailbox address for emails generated programmatically")]
        public MailAddress GeneralTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "From" mailbox address for emails generated programmatically
        /// </summary>
        [YAXLib.YAXComment("General \"From\" mailbox address for emails generated programmatically")]
        public MailAddress GeneralFrom { get; set; } = new MailAddress();
        /// <summary>
        /// General "BCC" mailbox address for emails generated programmatically
        /// </summary>
        [YAXLib.YAXComment("General \"BCC\" mailbox address for emails generated programmatically")]
        public MailAddress GeneralBcc { get; set; } = new MailAddress();
    }

    /// <summary>
    /// Email display name and address.
    /// </summary>
    public class MailAddress
    {
        /// <summary>
        /// The display name of a recipient.
        /// </summary>
        [YAXLib.YAXComment("The display name of a recipient.")]
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The email address of a recipient.
        /// </summary>
        [YAXLib.YAXComment("The email address of a recipient.")]
        public string Address { get; set; } = string.Empty;
    }
}