using System;

namespace TournamentManager.MultiTenancy
{
    public interface ITenantContext : ITenant
    {
        /// <summary>
        /// Deserializes an instance of <see cref="TenantContext"/> to XML.
        /// </summary>
        /// <param name="filename"></param>
        void SerializeToFile(string filename);

        /// <summary>
        /// Deserializes an instance of <see cref="TenantContext"/> to a string.
        /// </summary>
        string Serialize();

        /// <summary>
        /// Provides site-specific data.
        /// </summary>
        SiteContext SiteContext { get; set; }

        /// <summary>
        /// Provides organization-specific data.
        /// </summary>
        OrganizationContext OrganizationContext { get; set; }

        /// <summary>
        /// Provides database-specific properties and methods.
        /// </summary>
        DbContext DbContext { get; set; }

        /// <summary>
        /// Provides configuration data for a tournament.
        /// </summary>
        TournamentContext TournamentContext { get; set; }
    }
}