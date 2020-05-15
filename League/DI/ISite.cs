
namespace League.DI
{
    /// <summary>
    /// Interface for site-specific settings
    /// </summary>
    public interface ISite
    {
        /// <summary>
        /// The host name used for the site.
        /// </summary>
        string HostName { get; set; }
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
        /// The organization key corresponding to the site. The same OrganizationKey is also in DbContextList.
        /// </summary>
        string OrganizationKey { get; set; }
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
    }
}