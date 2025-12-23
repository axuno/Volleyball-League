using Axuno.Tools.FileSystem;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace TournamentManager.MultiTenancy;

/// <summary>
/// Watches for file system changes for tenant configuration files,
/// and modifies the <see cref="ITenantStore{T}"/> accordingly.
/// <para/>
/// The configuration changes have <b>immediate effect without a restart</b>.
/// </summary>
public class TenantConfigWatcher
{
    private readonly DelayedFileSystemWatcher _tenantFileWatcher;
    private readonly ITenantStore<ITenantContext> _tenantStore;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="tenantStore">The instance of the <see cref="ITenantStore{T}"/></param>
    /// <param name="configPath">The full path to the directory containing tenant configuration files.</param>
    /// <param name="typeFilter">The filter for file names that are watched, e.g. "*.config"</param>
    public TenantConfigWatcher(ITenantStore<ITenantContext> tenantStore, string configPath, string typeFilter)
    {
        _tenantFileWatcher = new(configPath, typeFilter)
            { ConsolidationInterval = 1000, EnableRaisingEvents = true };
        _tenantStore = tenantStore;

        _tenantFileWatcher.Changed += HandleCreatedOrChangedEvent;
        _tenantFileWatcher.Created += HandleCreatedOrChangedEvent;
        _tenantFileWatcher.Deleted += HandleDeletedEvent;
        _tenantFileWatcher.Renamed += HandleRenamedEvent;

        // Watch and handle configuration file (i.e. appsettings.json, credentials.json) changes
        ChangeToken.OnChange(() => tenantStore.Configuration.GetReloadToken(), OnSettingsChanged);
    }

    /// <summary>
    /// Updates the connection string of each tenant after a configuration change.
    /// </summary>
    private void OnSettingsChanged()
    {
        foreach (var t in _tenantStore.GetTenants().Values) UpdateConnectionString(t);
    }

    private void UpdateConnectionString(ITenantContext tenant)
    {
        var cs = _tenantStore.Configuration.GetConnectionString(tenant.DbContext.ConnectionKey) ?? string.Empty;
        if (cs != tenant.DbContext.ConnectionString)
        {
            tenant.DbContext.ConnectionString =
                _tenantStore.Configuration.GetConnectionString(tenant.DbContext.ConnectionKey) ?? string.Empty;
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="DelayedFileSystemWatcher"/>.
    /// </summary>
    /// <remarks>
    /// For unit tests.
    /// </remarks>
    internal DelayedFileSystemWatcher GetTenantFileWatcher() => _tenantFileWatcher;

    /// <summary>
    /// If a configuration files has been removed from the folder,
    /// the <see cref="ITenantContext"/> will be removed from the <see cref="ITenantStore{T}"/>.
    /// </summary>
    private void HandleDeletedEvent(object sender, FileSystemEventArgs e)
    {
        var tenant = _tenantStore.GetTenants().Values.FirstOrDefault(t => t.Filename == e.FullPath);
        if (tenant == null) return;

        _tenantStore.TryRemoveTenant(tenant.Identifier);
    }

    /// <summary>
    /// If a configuration file in the folder has been changed or created,
    /// the <see cref="ITenantContext"/> will updated or inserted in the <see cref="ITenantStore{T}"/>.
    /// </summary>
    private void HandleCreatedOrChangedEvent(object sender, FileSystemEventArgs e)
    {
        var tenant = _tenantStore.BuildTenantContext(e.FullPath);
        if (tenant == null) return;

        if (!_tenantStore.TryUpdateTenant(tenant.Identifier, tenant))
            _tenantStore.TryAddTenant(tenant);
    }

    /// <summary>
    /// If a configuration file in the folder has been renamed,
    /// and the new file name does not match the configuration file name pattern,
    /// the <see cref="ITenantContext"/> will be removed from the <see cref="ITenantStore{T}"/>.
    /// <para/>
    /// If a configuration file in the folder has been renamed,
    /// and the new file name matches the configuration file name pattern,
    /// the <see cref="ITenantContext"/> will be updated or inserted in the <see cref="ITenantStore{T}"/>.
    /// </summary>
    private void HandleRenamedEvent(object sender, FileSystemEventArgs e)
    {
        var tenant = _tenantStore.BuildTenantContext(e.FullPath);
        if (!_tenantStore.GetTenantConfigurationFiles.Invoke().Contains(e.FullPath))
        {
            if (tenant == null) return;

            _tenantStore.TryRemoveTenant(tenant.Identifier);
            return;
        }

        if (tenant == null) return;

        if (!_tenantStore.TryUpdateTenant(tenant.Identifier, tenant))
            _tenantStore.TryAddTenant(tenant);
    }
}
