#nullable enable
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// A class to manage <see cref="ITenantContext"/>s.
    /// </summary>
    public class TenantStore : AbstractTenantStore<ITenantContext>
    {
        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public TenantStore(IConfiguration configuration, ILogger<TenantStore> logger) : base(configuration, logger)
        { }
        
        /// <summary>
        /// Loads tenant configurations from the file system using the <see cref="ITenantStore{T}.GetTenantConfigurationFiles"/> delegate.
        /// </summary>
        /// <returns>Returns the current implementation of an <see cref="ITenantStore{T}"/>.</returns>
        public override ITenantStore<ITenantContext> LoadTenants()
        {
            try
            {
                var store = base.LoadTenants();
                Logger.LogTrace($"Tenants loaded into the {nameof(TenantStore)}.");
                return store;
            }
            catch (Exception e)
            {
                Logger.LogCritical("Loading tenant configurations failed.", e);
                throw;
            }
        }
    }
}
