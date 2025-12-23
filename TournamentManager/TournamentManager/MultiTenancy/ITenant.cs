namespace TournamentManager.MultiTenancy;

/// <summary>
/// Interface for tenant specific data.
/// </summary>
public interface ITenant
{
    /// <summary>
    /// Gets or sets the unique tenant identifier.
    /// </summary>
    string Identifier { get; set; }
    /// <summary>
    /// Gets or sets the tenant GUID.
    /// </summary>
    Guid Guid { get; set; }
        
    /// <summary>
    /// Gets or sets the filename of the tenant configuration.
    /// </summary>
    string Filename { get; set; }
        
    /// <summary>
    /// If <see langword="true"/>, this is the default tenant.
    /// </summary>
    bool IsDefault { get; set; }
}
