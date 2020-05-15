namespace League.DI
{
    /// <summary>
    /// Site-specific settings
    /// </summary>
    public class Site : ISite
    {
        public string HostName { get; set; }
        public string UrlSegmentValue { get; set; }
        public string FolderName { get; set; }
        public string OrganizationKey { get; set; }
        public string IdentityCookieName { get; set; }
        public string SessionName { get; set; }
        public bool HideInMenu { get; set; }
    }
}
