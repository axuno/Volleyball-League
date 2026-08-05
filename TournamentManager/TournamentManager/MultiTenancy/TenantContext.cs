using System.Text.Json.Serialization;
using TournamentManager.JsonCSerializer;

namespace TournamentManager.MultiTenancy;

[JsonComment("Configuration data for a tenant.")]
public class TenantContext : ITenantContext
{
    /// <summary>
    /// CTOR.
    /// </summary>
    public TenantContext()
    {
        SiteContext.Tenant = OrganizationContext.Tenant =
            DbContext.Tenant = TournamentContext.Tenant = this;
    }
        
    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    [JsonComment("Identifies the tenant. Value is also used for tenant-specific file names.")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant GUID.
    /// </summary>
    [JsonComment("The tenant GUID.")]
    public Guid Guid { get; set; } = Guid.NewGuid();
        
    /// <summary>
    /// If <see langword="true"/>, this is the default tenant.
    /// </summary>
    [JsonComment("May only be true for a single tenant in a tenant store.")]
    public bool IsDefault { get; set; }
        
    /// <summary>
    /// Gets or sets the filename of the tenant configuration.
    /// </summary>
    [JsonIgnore]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Provides site-specific data.
    /// </summary>
    [JsonComment("Provides website specific data.")]
    public SiteContext SiteContext { get; set; } = new();

    /// <summary>
    /// Provides organization-specific data.
    /// </summary>
    [JsonComment("All configuration data for an organization.")]
    public OrganizationContext OrganizationContext { get; set; } = new();

    /// <summary>
    /// Provides database access specific properties and methods.
    /// </summary>
    [JsonComment("Database access specific settings.")]
    public DbContext DbContext { get; set; } = new();

    /// <summary>
    /// Provides configuration data for a tournament.
    /// </summary>
    [JsonComment("Configuration data for a tournament")]
    public TournamentContext TournamentContext { get; set; } = new();
}
