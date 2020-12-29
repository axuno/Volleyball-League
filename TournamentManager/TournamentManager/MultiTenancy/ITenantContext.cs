#nullable enable
using TournamentManager.MultiTenancy;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// The class keeps all tenant-specific data for the <see cref="League"/>.
    /// </summary>
    public interface ITenantContext : TournamentManager.MultiTenancy.IManagerTenantContext
    {
        /// <summary>
        /// Provides site-specific data.
        /// </summary>
        public SiteContext SiteContext { get; set; }
        
        /// <summary>
        /// Provides organization-specific data.
        /// </summary>
        public OrganizationContext OrganizationContext { get; set; }
    }
}
