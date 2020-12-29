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
        public TournamentManager.MultiTenancy.ITenant? Tenant { get; set; }
        /// <summary>
        /// The host name used for the site.
        /// </summary>
        public string HostName { get; set; } = string.Empty;
        /// <summary>
        /// The value of the Url segment used to identify the site. May be NULL or empty string.
        /// </summary>
        public string UrlSegmentValue { get; set; } = string.Empty;
        /// <summary>
        /// The folder name used for the site. This name must be set.
        /// It is used to located assets like views or files for a certain organization.
        /// </summary>
        public string FolderName { get; set; } = string.Empty;
        /// <summary>
        /// The cookie name used Asp.Net Identity.
        /// </summary>
        public string IdentityCookieName { get; set; } = string.Empty;
        /// <summary>
        /// The session name used for the site.
        /// </summary>
        public string SessionName { get; set; } = string.Empty;
        /// <summary>
        /// If true, the site will not be shown in the navigation menu.
        /// </summary>
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
        /// The Folder for team photos.
        /// </summary>
        public string TeamPhotoFolder { get; set; } = string.Empty;
        /// <summary>
        /// The name of the image file, which will be displayed if no team photo exists.
        /// </summary>
        public string TeamDefaultFilename { get; set; } = string.Empty;
        /// <summary>
        /// The Folder for photos of people.
        /// </summary>
        public string PeoplePhotoFolder { get; set; } = string.Empty;
        /// <summary>
        /// The name of the image file, which will be displayed if no person photo exists.
        /// </summary>
        public string PeopleDefaultFilename { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Email contact details for an organization.
    /// </summary>
    public class Email
    {
        /// <summary>
        /// "From" mailbox address for the contact form
        /// </summary>
        public MailAddress ContactFrom { get; set; } = new MailAddress();
        /// <summary>
        /// "To" mailbox address for the contact form
        /// </summary>
        public MailAddress ContactTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "To" mailbox address for emails generated programmatically
        /// </summary>
        public MailAddress GeneralTo { get; set; } = new MailAddress();
        /// <summary>
        /// General "From" mailbox address for emails generated programmatically
        /// </summary>
        public MailAddress GeneralFrom { get; set; } = new MailAddress();
        /// <summary>
        /// General "BCC" mailbox address for emails generated programmatically
        /// </summary>
        public MailAddress GeneralBcc { get; set; } = new MailAddress();
    }

    /// <summary>
    /// Email display name and address.
    /// </summary>
    public class MailAddress
    {
        /// <summary>
        /// The display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// The email address
        /// </summary>
        public string Address { get; set; } = string.Empty;
    }
}