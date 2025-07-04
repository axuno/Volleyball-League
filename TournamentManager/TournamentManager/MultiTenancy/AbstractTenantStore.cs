﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// An abstract class used to manage <see cref="ITenantContext"/>s and store them in memory.
/// </summary>
/// <typeparam name="T">The implementation of <see cref="ITenantContext"/> and <see cref="ITenant"/></typeparam>
public class AbstractTenantStore<T> : ITenantStore<T> where T: class, ITenantContext, ITenant
{
    protected readonly ConcurrentDictionary<string, T> Tenants = new();
    protected ILogger Logger;
    internal YAXLib.YAXSerializer<TenantContext?> TenantContextSerializer = new();

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="logger"></param>
    public AbstractTenantStore(IConfiguration configuration, ILogger logger)
    {
        Configuration = configuration;
        Logger = logger;
    }
        
    /// <summary>
    /// Gets a list of <typeparamref name="T"/>.
    /// </summary>
    /// <returns>Returns a list of <typeparamref name="T"/> tenant configurations.</returns>
    public virtual IReadOnlyDictionary<string, T> GetTenants()
    {
        return Tenants;
    }

    /// <summary>
    /// Gets the <see cref="ITenantContext"/> configuration for the tenant with the specified <paramref name="identifier"/>.
    /// </summary>
    /// <returns>Returns the <see cref="ITenantContext"/> configuration for the tenant with the specified <paramref name="identifier"/> or <see langword="null"/> if it could not be found.</returns>
    public virtual ITenantContext? GetTenantByIdentifier(string identifier)
    {
        return Tenants.GetValueOrDefault(identifier);
    }

    /// <summary>
    /// Gets the <see cref="ITenantContext"/> configuration for the tenant with the specified <paramref name="urlSegmentValue"/>.
    /// Neither <paramref name="urlSegmentValue"/> nor the <see cref="SiteContext.UrlSegmentValue"/> may be <see langword="null"/> or whitespace.
    /// </summary>
    /// <returns>Returns the <see cref="ITenantContext"/> configuration for the tenant with the specified <paramref name="urlSegmentValue"/> if found, or <see langword="null"/> if not found.</returns>
    public virtual ITenantContext? GetTenantByUrlSegment(string urlSegmentValue)
    {
        return Tenants.Values.FirstOrDefault(t =>
            !string.IsNullOrWhiteSpace(t.SiteContext.UrlSegmentValue) && !string.IsNullOrWhiteSpace(urlSegmentValue) &&
            t.SiteContext.UrlSegmentValue.Equals(urlSegmentValue, StringComparison.InvariantCultureIgnoreCase));
    }
        
    /// <summary>
    /// Gets the default <see cref="ITenantContext"/> configuration (i.e. <see cref="ITenant.IsDefault"/> is <see langword="true"/>).
    /// </summary>
    /// <returns>Returns the default <see cref="ITenantContext"/> configuration (i.e. <see cref="ITenant.IsDefault"/> is <see langword="true"/>) if found, or <seealso langword="null"/> if not found.</returns>
    public virtual ITenantContext? GetDefaultTenant()
    {
        return Tenants.Values.FirstOrDefault(t => t.IsDefault);
    }

    /// <summary>
    /// Attempts to add the <typeparamref name="T"/> to the <see cref="TenantStore"/>.
    /// </summary>
    /// <param name="tenant">The <typeparamref name="T"/> to add.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was added, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> already exists in the <see cref="TenantStore"/>.</returns>
    public virtual bool TryAddTenant(T tenant)
    {
        var success = Tenants.TryAdd(tenant.Identifier, tenant);
        Logger.LogTrace("Tenant with {Tenant} '{TenantIdentifier}' {SuccessMsg}.", nameof(tenant.Identifier), tenant.Identifier,  success ? "added" : "failed to add");
        return success;
    }
        
    /// <summary>
    /// Attempts to remove the <typeparamref name="T"/> instance from the <see cref="TenantStore"/>.
    /// </summary>
    /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was removed, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
    public virtual bool TryRemoveTenant(string identifier)
    {
        var success = Tenants.TryRemove(identifier, out _);
        Logger.LogTrace("Tenant with {Name} '{Identifier}' {SuccessMsg}.", nameof(identifier), identifier, success ? "removed" : "failed to remove");
        return success;
    }

    /// <summary>
    /// Attempts to update the <typeparamref name="T"/> instance with <see cref="ITenant.Identifier"/> in the <see cref="TenantStore"/>.
    /// </summary>
    /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
    /// <param name="newTenant">The new <typeparamref name="T"/> instances.</param>
    /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was updated, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
    public virtual bool TryUpdateTenant(string identifier, T newTenant)
    {
        var success = false;
        if (Tenants.TryGetValue(identifier, out var currentTenant))
        {
            // Make sure the dictionary key and the tenant identifier are always equal
            newTenant.Identifier = identifier;
            success = Tenants.TryUpdate(identifier, newTenant, currentTenant);
        }
        Logger.LogTrace("Tenant with {Name} '{Identifier}' {SuccessMsg}.", nameof(identifier), identifier, success ? "updated" : "failed to update");
        return success;
    }
        
    /// <summary>
    /// Clears any existing <typeparamref name="T"/> instances from the <see cref="ITenantStore{T}"/> and loads tenant configurations from the file system using the <see cref="GetTenantConfigurationFiles"/> delegate.
    /// <see cref="IDbContext.ConnectionString"/>s are added to the <see cref="DbContext"/> using the <see cref="DbContext.ConnectionKey"/>.
    /// </summary>
    /// <returns>Returns the current implementation of a <see cref="AbstractTenantStore{T}"/>.</returns>
    public virtual ITenantStore<T> LoadTenants()
    {
        Tenants.Clear();
        var configFiles = GetTenantConfigurationFiles.Invoke();
        Logger.LogInformation("Tenant config files: {Config}", configFiles.ToList());

        foreach (var configFile in configFiles)
        {
            var tc = BuildTenantContext(configFile)
                     ?? throw new InvalidOperationException($"{nameof(ITenantContext)} could not be built from file '{configFile}'.");

            if (!Tenants.TryAdd(tc.Identifier, (T) tc))
            {
                throw new InvalidOperationException($"Identifier '{tc.Identifier}' is already taken for another tenant");
            }
        }

        return this;
    }

    /// <summary>
    /// Loads the tenant configuration file and builds an <see cref="ITenantContext"/>.
    /// <see cref="IDbContext.ConnectionString"/>s are added to the <see cref="DbContext"/> using the <see cref="DbContext.ConnectionKey"/>.
    /// </summary>
    /// <param name="filePath">The full path to the file</param>
    /// <returns>Returns the <see cref="ITenantContext"/>, if the tenant could be built, else <see langword="null"/>.</returns>
    public virtual ITenantContext? BuildTenantContext(string filePath)
    {
        var tc = (ITenantContext?) TenantContextSerializer.Deserialize(ReadTenantConfigFile(filePath));
        if (tc == null) return null;

        tc.Filename = filePath;
        tc.DbContext.ConnectionString =
            Configuration.GetConnectionString(tc.DbContext.ConnectionKey) ?? string.Empty;
        SetTenantForChildContexts((T) tc);
        return tc;
    }

    /// <summary>
    /// Reads a TenantContext configuration file.
    /// </summary>
    /// <param name="filename">The full path to the file</param>
    /// <returns>The content of the TenantContext configuration file.</returns>
    /// <remarks>Implemented mainly for unit testing.</remarks>
    public virtual string ReadTenantConfigFile(string filename)
    {
        return File.ReadAllText(filename);
    }

    /// <summary>
    /// Sets the tenant context in child contexts of the tenant.
    /// </summary>
    /// <param name="tenantContext"></param>
    public virtual void SetTenantForChildContexts(T tenantContext)
    {
        tenantContext.SiteContext.Tenant = tenantContext.OrganizationContext.Tenant =
            tenantContext.DbContext.Tenant = tenantContext.TournamentContext.Tenant = tenantContext;
    }

    /// <summary>
    /// The <see cref="Func{T}"/> delegate to get the tenant configuration file names.
    /// </summary>
    /// <example>
    /// GetTenantConfigurationFiles = Directory.GetFiles(ConfigurationFolder, "Tenant.*.Production.config", SearchOption.TopDirectoryOnly);
    /// </example>
    public Func<string[]> GetTenantConfigurationFiles { get; set; } = Array.Empty<string>;
        
    /// <summary>
    /// Gets or sets the <see cref="IConfiguration"/> used for retrieving the connection strings for a tenant.
    /// </summary>
    public IConfiguration Configuration { get; set; }
}
