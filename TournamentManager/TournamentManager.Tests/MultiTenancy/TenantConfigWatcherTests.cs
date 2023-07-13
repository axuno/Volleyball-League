using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.MultiTenancy;

[TestFixture]
public class TenantConfigWatcherTests
{
    private const string ConfigSearchPattern = "Tenant.*.config";
    private string _directoryToWatch = CreateTempPathFolder();

    // For TenantStore
    private const string ConnectionStrings = "ConnectionStrings";
    private const string ConnKeyPrefix = "Conn";
    private const string ConnValuePrefix = "ConnValue";
    private readonly List<string> _tenantNames = new() {"Tenant1", "Tenant2", "Tenant3", "DefaultTenant"};
    private readonly TenantStore _store;

    public TenantConfigWatcherTests()
    {
        var config = CreateTenantStoreConfig();
        _store = new TenantStore(config, new NullLoggerFactory());
        _store.GetTenantConfigurationFiles = () => Directory.GetFiles(
            _directoryToWatch, ConfigSearchPattern, SearchOption.TopDirectoryOnly);
        var watcher = GetTenantConfigWatcher();
        watcher.GetTenantFileWatcher().ConsolidationInterval = 10;
    }

    [Test]
    public async Task AddNewTenantFile()
    {
        _store.LoadTenants();
        var initialTenantCount = _store.GetTenants().Count;
        var tenantName = _tenantNames[0];
        CreateTenantContext(tenantName).SerializeToFile(Path.Combine(_directoryToWatch, $"Tenant.{tenantName}.config"));
        await Task.Delay(200);

        Assert.That(_store.GetTenants().Count, Is.EqualTo(initialTenantCount + 1));
        Assert.That(_store.GetTenantByIdentifier(tenantName)?.DbContext.ConnectionString, Does.Contain(tenantName));
    }

    [Test]
    public async Task DeleteTenantFile()
    {
        _store.LoadTenants();
        var initialTenantCount = _store.GetTenants().Count;
        if (initialTenantCount == 0)
        {
            var tenantName = _tenantNames[0];
            CreateTenantContext(tenantName).SerializeToFile(Path.Combine(_directoryToWatch, $"Tenant.{tenantName}.config"));
        }
        await Task.Delay(100);
        initialTenantCount = _store.GetTenants().Count;
        File.Delete(_store.GetTenants().First().Value.Filename);
        await Task.Delay(100);

        Assert.That(_store.GetTenants().Count, Is.EqualTo(initialTenantCount - 1));
    }

    [Test]
    public async Task RenameTenantFile_KeepSearchPattern()
    {
        _store.LoadTenants();
        var initialTenantCount = _store.GetTenants().Count;
        if (initialTenantCount == 0)
        {
            var tenantName = _tenantNames[0];
            CreateTenantContext(tenantName).SerializeToFile(Path.Combine(_directoryToWatch, $"Tenant.{tenantName}.config"));
        }
        await Task.Delay(100);
        var identifier = _store.GetTenants().First().Value.Identifier;
        var newPath = Path.Combine(_directoryToWatch, $"Tenant.xXx.config");
        File.Move(_store.GetTenantByIdentifier(identifier)!.Filename, newPath);
        await Task.Delay(200);

        Assert.That(_store.GetTenantByIdentifier(identifier)!.Filename, Is.EqualTo(newPath));
    }

    [Test]
    public async Task RenameTenantFile_NotInSearchPattern()
    {
        _store.LoadTenants();
        var initialTenantCount = _store.GetTenants().Count;
        if (initialTenantCount == 0)
        {
            var tenantName = _tenantNames[0];
            CreateTenantContext(tenantName).SerializeToFile(Path.Combine(_directoryToWatch, $"Tenant.{tenantName}.config"));
        }
        await Task.Delay(100);
        var identifier = _store.GetTenants().First().Value.Identifier;
        var newPath = Path.Combine(_directoryToWatch, $"__Tenant.{identifier}.config");
        File.Move(_store.GetTenantByIdentifier(identifier)!.Filename, newPath);
        await Task.Delay(200);

        Assert.That(_store.GetTenantByIdentifier(identifier), Is.Null);
    }

    private TenantConfigWatcher GetTenantConfigWatcher()
    {
        var watcher = new TenantConfigWatcher(_store, _directoryToWatch, ConfigSearchPattern);
        return watcher;
    }

    private IConfiguration CreateTenantStoreConfig()
    {
        // Configure connection strings per tenant
        var connStr = new List<KeyValuePair<string, string>>();
        _tenantNames.ForEach(t => connStr.Add(new KeyValuePair<string, string>($"{ConnectionStrings}:{ConnKeyPrefix}{t}", $"{ConnValuePrefix}{t}")));
        // Build configuration
        var cb = new ConfigurationBuilder();
        cb.AddInMemoryCollection(connStr);
        var config = cb.Build();

        return config;
    }

    private static string CreateTempPathFolder()
    {
        // Create folder in TempPath
        var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }

    [OneTimeTearDown]
    public void CleanUp()
    {
        Directory.Delete(_directoryToWatch, true);
    }

    private TenantContext CreateTenantContext(string tenantIdentifier)
    {
        var tenant = new TenantContext
        {
            Identifier = tenantIdentifier,
            IsDefault = tenantIdentifier.Equals("DefaultTenant"),
            Guid = Guid.NewGuid(),
            Filename = string.Empty,
            SiteContext = {UrlSegmentValue = tenantIdentifier },
            DbContext = {ConnectionKey = ConnKeyPrefix + tenantIdentifier},
            OrganizationContext = {Name = "Long: " + tenantIdentifier, ShortName = "Short: " + tenantIdentifier}
        };
        return tenant;
    }
}



