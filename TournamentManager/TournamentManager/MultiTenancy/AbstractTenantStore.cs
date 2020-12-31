using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// An abstract class used to read configuration files for <see cref="ITenant"/>s and store them in memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractTenantStore<T> where T: class, IManagerTenantContext 
    {
        protected readonly ConcurrentDictionary<string, T> Tenants = new ConcurrentDictionary<string, T>();
        protected ILogger Logger;

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
        /// Gets a list of <see cref="ITenant"/> configurations for all current tenants.
        /// </summary>
        /// <returns>Returns a list of <see cref="ITenant"/> configurations for all current tenants.</returns>
        public IReadOnlyDictionary<string, T> GetTenants()
        {
            return Tenants;
        }

        /// <summary>
        /// Gets the <see cref="ITenant"/> configuration for the tenant with the specified <see cref="identifier"/>.
        /// </summary>
        /// <returns>Returns the <see cref="ITenant"/> configuration for the tenant with the specified <see cref="identifier"/>.</returns>
        public T? GetTenantByIdentifier(string identifier)
        {
            return Tenants.ContainsKey(identifier) ? Tenants[identifier] : null;
        }

        /// <summary>
        /// Attempts to add the <see cref="ITenant"/> to the <seealso cref="TenantStore"/>.
        /// </summary>
        /// <param name="tenant">The <see cref="ITenant"/> to add.</param>
        /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was added, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> already exists in the <see cref="TenantStore"/>.</returns>
        public bool TryAddTenant(T tenant)
        {
            var success = Tenants.TryAdd(tenant.Identifier, tenant);
            Logger.LogTrace($"Tenant with {nameof(tenant.Identifier)} '{tenant.Identifier}' {(success ? "added" : "failed to add")}.");
            return success;
        }
        
        /// <summary>
        /// Attempts to remove the <see cref="ITenant"/> from the <seealso cref="TenantStore"/>.
        /// </summary>
        /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
        /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was removed, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
        public bool TryRemoveTenant(string identifier)
        {
            var success = Tenants.TryRemove(identifier, out _);
            Logger.LogTrace($"Tenant with {nameof(identifier)} '{identifier}' {(success ? "removed" : "failed to remove")}.");
            return success;
        }

        /// <summary>
        /// Attempts to update the <see cref="ITenant"/> with <see cref="ITenant.Identifier"/> in the <seealso cref="TenantStore"/>.
        /// </summary>
        /// <param name="identifier">The <see cref="ITenant.Identifier"/> to remove.</param>
        /// <param name="newTenant">The new <see cref="ITenant"/>.</param>
        /// <returns>Returns <see langword="true"/>, if the <see cref="ITenant"/> was updated, or <see langword="false"/> if the <see cref="ITenant.Identifier"/> was not found in the <see cref="TenantStore"/>.</returns>
        public bool TryUpdateTenant(string identifier, T newTenant)
        {
            var success = false;
            if (Tenants.TryGetValue(identifier, out T currentTenant))
            {
                success = Tenants.TryUpdate(identifier, newTenant, currentTenant);
            }
            Logger.LogTrace($"Tenant with {nameof(identifier)} '{identifier}' {(success ? "updated" : "failed to update")}.");
            return success;
        }
        
        /// <summary>
        /// Clears any existing <see cref="ITenant"/>s from the <see cref="AbstractTenantStore{T}"/> and loads tenant configurations from the file system using the <see cref="GetTenantConfigurationFiles"/> delegate.
        /// <see cref="DbContext.ConnectionString"/>s are added to the <see cref="DbContext"/> using the <see cref="DbContext.ConnectionKey"/>.
        /// </summary>
        /// <returns>Returns the current implementation of a <see cref="AbstractTenantStore{T}"/>.</returns>
        public AbstractTenantStore<T> LoadTenants()
        {
            Tenants.Clear();
            foreach (var configFile in GetTenantConfigurationFiles.Invoke())
            {
                var s = new YAXLib.YAXSerializer(typeof(T));
                var tc = (T) s.DeserializeFromFile(configFile);
                tc.Filename = configFile;
                tc.DbContext.ConnectionString =
                    Configuration.GetConnectionString(tc.DbContext.ConnectionKey) ?? string.Empty;
                SetTenantForChildContexts(tc);
                if (!Tenants.TryAdd(tc.Identifier, tc))
                {
                    throw new Exception($"Identifier '{tc.Identifier}' is already taken for another tenant");
                }
            }

            return this;
        }

        /// <summary>
        /// Sets the tenant context in child contexts of the tenant.
        /// </summary>
        /// <param name="tenantContext"></param>
        protected virtual void SetTenantForChildContexts(T tenantContext)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The <see cref="Func{T}"/> delegate to get the tenant configuration file names.
        /// </summary>
        /// <example>
        /// GetTenantConfigurationFiles = Directory.GetFiles(ConfigurationFolder, "Tenant.*.Production.config", SearchOption.TopDirectoryOnly);
        /// </example>
        public Func<string[]> GetTenantConfigurationFiles { get; set; } = () => new string[] { };
        
        /// <summary>
        /// Gets or sets the <see cref="IConfiguration"/> used for retrieving the connection strings for a tenant.
        /// </summary>
        public IConfiguration Configuration { get; set; }
    }
}
