using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// An abstract class used to read configuration files for <see cref="ITenant"/>s and store them in memory.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AbstractTenantStore<T> where T: class, IManagerTenantContext 
    {
        protected readonly ConcurrentDictionary<string, T> Tenants = new ConcurrentDictionary<string, T>();
        
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="configuration"></param>
        public AbstractTenantStore(IConfiguration configuration)
        {
            Configuration = configuration;
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
        /// Loads tenant configurations from the file system using the <see cref="GetTenantConfigurationFiles"/> delegate.
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
        /// Sets the tenant context in child contexts of the tenant if type <see cref="T"/>.
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
