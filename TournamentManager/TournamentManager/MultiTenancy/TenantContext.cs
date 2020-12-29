using System;

#nullable enable

namespace TournamentManager.MultiTenancy
{
    [YAXLib.YAXSerializeAs(nameof(TenantContext))]
    public class TenantContext : ITenantContext
    {
        #region *** Serialization ***

        /// <summary>
        /// Deserializes an XML file to an instance of <see cref="TenantContext"/>.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static TenantContext DeserializeFromFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(TenantContext));
            var tc = (TenantContext) s.DeserializeFromFile(filename);
            tc.SiteContext.Tenant = tc.OrganizationContext.Tenant = tc.DbContext.Tenant = tc.TournamentContext.Tenant = tc;
            return tc;
        }

        /// <summary>
        /// Deserializes an instance of <see cref="TenantContext"/> to an XML file.
        /// </summary>
        /// <param name="filename"></param>
        public void SerializeToFile(string filename)
        {
            var s = new YAXLib.YAXSerializer(typeof(TenantContext));
            s.SerializeToFile(this, filename);
        }

        #endregion
        
        /// <summary>
        /// Gets or sets the unique tenant identifier.
        /// </summary>
        public string Identifier { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the tenant GUID.
        /// </summary>
        public Guid Guid { get; set; } = new Guid();
        
        /// <summary>
        /// If <see langword="true"/>, this is the default tenant.
        /// </summary>
        public bool IsDefault { get; set; }
        
        /// <summary>
        /// Gets or sets the filename of the tenant configuration.
        /// </summary>
        [YAXLib.YAXDontSerialize]
        public string Filename { get; set; } = string.Empty;
        
        /// <summary>
        /// Provides site-specific data.
        /// </summary>
        public SiteContext SiteContext { get; set; } = new SiteContext();

        /// <summary>
        /// Provides organization-specific data.
        /// </summary>
        public OrganizationContext OrganizationContext { get; set; } = new OrganizationContext();

        /// <summary>
        /// Provides database-specific properties and methods.
        /// </summary>
        public TournamentManager.MultiTenancy.DbContext DbContext { get; set; } = new TournamentManager.MultiTenancy.DbContext();

        /// <summary>
        /// Provides configuration data for a tournament.
        /// </summary>
        public TournamentManager.MultiTenancy.TournamentContext TournamentContext { get; set; } = new TournamentManager.MultiTenancy.TournamentContext();
    }
}
