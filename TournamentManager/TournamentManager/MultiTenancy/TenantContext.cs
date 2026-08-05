using System.Text.Json.Serialization;
using TournamentManager.JsonCSerializer;

namespace TournamentManager.MultiTenancy;

[YAXLib.Attributes.YAXSerializeAs(nameof(TenantContext))]
[YAXLib.Attributes.YAXComment("Configuration data for a tenant.")]
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
        
    #region *** Serialization ***

    /// <summary>
    /// Deserializes an XML file to an instance of <see cref="TenantContext"/>.
    /// </summary>
    /// <param name="filename"></param>
    /// <returns>An instance of <see cref="TenantContext"/>.</returns>
    public static TenantContext DeserializeFromFile(string filename)
    {
        return Deserialize(File.ReadAllText(filename));
    }
        
    /// <summary>
    /// Deserializes XML to an instance of <see cref="TenantContext"/>.
    /// </summary>
    /// <param name="content"></param>
    /// <returns>An instance of <see cref="TenantContext"/>.</returns>
    public static TenantContext Deserialize(string content)
    {
        var s = new YAXLib.YAXSerializer(typeof(TenantContext));
        var tc = (TenantContext) s.Deserialize(content)!;
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
        
    /// <summary>
    /// Deserializes an instance of <see cref="TenantContext"/> to a string.
    /// </summary>
    public string Serialize()
    {
        var s = new YAXLib.YAXSerializer(typeof(TenantContext));
        return s.Serialize(this);
    }

    #endregion
        
    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Identifies the tenant. Value is also used for tenant-specific file names.")]
    [JsonComment("Identifies the tenant. Value is also used for tenant-specific file names.")]
    public string Identifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the tenant GUID.
    /// </summary>
    [YAXLib.Attributes.YAXComment("The tenant GUID.")]
    [JsonComment("The tenant GUID.")]
    public Guid Guid { get; set; } = Guid.NewGuid();
        
    /// <summary>
    /// If <see langword="true"/>, this is the default tenant.
    /// </summary>
    [YAXLib.Attributes.YAXComment("May only be true for a single tenant in a tenant store.")]
    [JsonComment("May only be true for a single tenant in a tenant store.")]
    public bool IsDefault { get; set; }
        
    /// <summary>
    /// Gets or sets the filename of the tenant configuration.
    /// </summary>
    [YAXLib.Attributes.YAXDontSerialize]
    [JsonIgnore]
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Provides site-specific data.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Provides website specific data.")]
    [JsonComment("Provides website specific data.")]
    public SiteContext SiteContext { get; set; } = new();

    /// <summary>
    /// Provides organization-specific data.
    /// </summary>
    [YAXLib.Attributes.YAXComment("All configuration data for an organization.")]
    [JsonComment("All configuration data for an organization.")]
    public OrganizationContext OrganizationContext { get; set; } = new();

    /// <summary>
    /// Provides database access specific properties and methods.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Database access specific settings.")]
    [JsonComment("Database access specific settings.")]
    public DbContext DbContext { get; set; } = new();

    /// <summary>
    /// Provides configuration data for a tournament.
    /// </summary>
    [YAXLib.Attributes.YAXComment("Configuration data for a tournament")]
    [JsonComment("Configuration data for a tournament")]
    public TournamentContext TournamentContext { get; set; } = new();
}
