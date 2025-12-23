using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
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
    private readonly List<string> _tenantNames = ["Tenant1", "Tenant2", "Tenant3", "DefaultTenant"];
    private readonly TenantStore _store;

    public TenantConfigWatcherTests()
    {
        var config = CreateTenantStoreConfig();
        _store = new TenantStore(config, new NullLoggerFactory())
        {
            GetTenantConfigurationFiles = () => Directory.GetFiles(
                _directoryToWatch, ConfigSearchPattern, SearchOption.TopDirectoryOnly)
        };
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_store.GetTenants(), Has.Count.EqualTo(initialTenantCount + 1));
            Assert.That(_store.GetTenantByIdentifier(tenantName)?.DbContext.ConnectionString, Does.Contain(tenantName));
        }
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

        Assert.That(_store.GetTenants(), Has.Count.EqualTo(initialTenantCount - 1));
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

    [Test]
    public void ChangeOfConnectionStringAppSetting()
    {
        var tenantName = _tenantNames[0];
        _store.LoadTenants();
        
        if (_store.GetTenantByIdentifier(tenantName) == null)
        {
            // the watcher will load the file automatically
            CreateTenantContext(tenantName).SerializeToFile(Path.Combine(_directoryToWatch, $"Tenant.{tenantName}.config"));
        }
        
        var initialConnString = _store.GetTenantByIdentifier(tenantName)!.DbContext.GetNewAdapter().ConnectionString;

        // Change the connection string in the IConfiguration settings 
        _store.Configuration.GetSection("ConnectionStrings")[ConnKeyPrefix + tenantName] = "TheNewConnectionString";
        // Trigger ChangeToken.OnChange for the IConfiguration
        // which should make the watcher update the connection strings of all tenants
        ((IConfigurationRoot) _store.Configuration).Reload();

        var newConnString = _store.GetTenantByIdentifier(tenantName)!.DbContext.ConnectionString;

        Assert.That(newConnString, Is.Not.EqualTo(initialConnString));
        Assert.That(newConnString, Is.EqualTo("TheNewConnectionString"));
    }

    private TenantConfigWatcher GetTenantConfigWatcher()
    {
        var watcher = new TenantConfigWatcher(_store, _directoryToWatch, ConfigSearchPattern);
        return watcher;
    }

    private IConfiguration CreateTenantStoreConfig()
    {
        // Configure connection strings per tenant
        var connStr = new List<KeyValuePair<string, string?>>();
        _tenantNames.ForEach(t => connStr.Add(new($"{ConnectionStrings}:{ConnKeyPrefix}{t}", $"{ConnValuePrefix}{t}")));
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

    private static TenantContext CreateTenantContext(string tenantIdentifier)
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



