﻿#nullable enable
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TournamentManager.MultiTenancy;

namespace TournamentManager.MultiTenancy
{
    /// <summary>
    /// A class used to read configuration files for <see cref="ITenant"/>s and store them in memory.
    /// </summary>
    public class TenantStore : AbstractTenantStore<TenantContext>
    {
        private readonly ILogger<TenantStore> _logger;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public TenantStore(IConfiguration configuration, ILogger<TenantStore> logger) : base(configuration)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Sets the tenant context in child contexts of the tenant if type <see cref="TenantContext"/>.
        /// </summary>
        /// <param name="tenantContext"></param>
        protected override void SetTenantForChildContexts(TenantContext tenantContext)
        {
            tenantContext.SiteContext.Tenant = tenantContext.OrganizationContext.Tenant =
                tenantContext.DbContext.Tenant = tenantContext.TournamentContext.Tenant = tenantContext;
        }

        /// <summary>
        /// Loads tenant configurations from the file system using the <see cref="AbstractTenantStore&lt;T&gt;.GetTenantConfigurationFiles"/> delegate.
        /// </summary>
        /// <returns>Returns the current implementation of an <see cref="AbstractTenantStore{T}"/>, which is <see cref="TenantStore"/>.</returns>
        public new TenantStore LoadTenants()
        {
            try
            {
                return (TenantStore) base.LoadTenants();
            }
            catch (Exception e)
            {
                _logger.LogCritical("Loading tenant configurations failed.", e);
                throw;
            }
        }
    }
}
