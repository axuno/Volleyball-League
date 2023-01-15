using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// Interface for an <see cref="ITenantStore{T}"/> store to manage class instances implementing <see cref="ITenantContext"/> and <seealso cref="ITenant"/>.
/// </summary>
/// <typeparam name="T">A class of type <see cref="ITenantStore{T}"/>,</typeparam>
public interface ITenantStore<T> where T: class, ITenantContext, ITenant
{
    /// <summary>
    /// Sets the tenant context in child contexts of the tenant of type <see cref="ITenantContext"/>.
    /// </summary>
    /// <param name="tenantContext"></param>
    void SetTenantForChildContexts(T tenantContext);

    /// <summary>
    /// Gets a list of <see cref="ITenant"/> configurations for all current tenants.
    /// </summary>
    /// <returns>Returns a list of <see cref="ITenant"/> configurations for all current tenants.</returns>
    IReadOnlyDictionary<string, T> GetTenants();

    /// <summary>
    /// Gets the <see cref="ITenant"/> configuration for the tenant with the specified <see cref="identifier"/>.
    /// </summary>
    /// <returns>Returns the <see cref="ITenantContext"/> configuration for the tenant with the specified <see cref="identifier"/>.</returns>
    ITenantContext? GetTenantByIdentifier(string identifier);

    /// <summary>
    /// Attempts to add the <see cref="ITenant"/> to the <seealso cref="TenantStore"/>.
    /// </summary>
    /// <param name="tenant">The <see cref="ITenantContext"/> to add.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was added, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> already exists in the <see cref="TenantStore"/>.</returns>
    bool TryAddTenant(T tenant);

    /// <summary>
    /// Attempts to remove the <see cref="ITenant"/> from the <seealso cref="TenantStore"/>.
    /// </summary>
    /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was removed, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
    bool TryRemoveTenant(string identifier);

    /// <summary>
    /// Attempts to update the <see cref="ITenant"/> with <see cref="ITenant.Identifier"/> in the <seealso cref="TenantStore"/>.
    /// </summary>
    /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
    /// <param name="newTenant">The new <see cref="ITenantContext"/>.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was updated, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
    bool TryUpdateTenant(string identifier, T newTenant);

    /// <summary>
    /// Clears any existing <see cref="ITenant"/>s from the <see cref="AbstractTenantStore{T}"/> and loads tenant configurations from the file system using the <see cref="AbstractTenantStore{T}.GetTenantConfigurationFiles"/> delegate.
    /// <see cref="DbContext.ConnectionString"/>s are added to the <see cref="DbContext"/> using the <see cref="DbContext.ConnectionKey"/>.
    /// </summary>
    /// <returns>Returns the current implementation of a <see cref="AbstractTenantStore{T}"/>.</returns>
    ITenantStore<T> LoadTenants();

    /// <summary>
    /// Reads a TenantContext configuration file.
    /// </summary>
    /// <param name="filename">The full path to the file</param>
    /// <returns>The content of the TenantContext configuration file.</returns>
    /// <remarks>Implemented mainly for unit testing.</remarks>
    string ReadTenantConfigFile(string filename);

    /// <summary>
    /// The <see cref="Func{T}"/> delegate to get the tenant configuration file names.
    /// </summary>
    /// <example>
    /// GetTenantConfigurationFiles = Directory.GetFiles(ConfigurationFolder, "Tenant.*.Production.config", SearchOption.TopDirectoryOnly);
    /// </example>
    Func<string[]> GetTenantConfigurationFiles { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IConfiguration"/> used for retrieving the connection strings for a tenant.
    /// </summary>
    IConfiguration Configuration { get; set; }
}