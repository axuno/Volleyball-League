using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// A class to manage <see cref="ITenantContext"/>s.
/// </summary>
public class TenantStore : AbstractTenantStore<ITenantContext>
{
    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="loggerFactory">The <see cref="ILoggerFactory"/> to use, or <see langword="null"/> to use <see cref="TournamentManager"/>.<see cref="AppLogging"/>.</param>
    public TenantStore(IConfiguration configuration, ILoggerFactory? loggerFactory = null)
        : base(configuration, loggerFactory?.CreateLogger<TenantStore>() ?? AppLogging.CreateLogger<TenantStore>())
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
            Logger.LogCritical("Loading tenant configurations failed. {Exception}", e);
            throw;
        }
    }
}
