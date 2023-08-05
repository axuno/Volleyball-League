using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using TournamentManager.MultiTenancy;

namespace TournamentManager.Tests.MultiTenancy;

[TestFixture]
public class TenantStoreTests
{
    private const string ConnectionStrings = "ConnectionStrings";
    private const string ConnKeyPrefix = "Conn";
    private const string ConnValuePrefix = "ConnValue";
    private readonly List<string> _tenantNames = new() {"Tenant1", "Tenant2", "Tenant3", "DefaultTenant"};
    private readonly TenantStore _store;
        
    public TenantStoreTests()
    {
        // Configure connection strings per tenant
        var connStr = new List<KeyValuePair<string, string>>();
        _tenantNames.ForEach(t => connStr.Add(new KeyValuePair<string, string>($"{ConnectionStrings}:{ConnKeyPrefix}{t}", $"{ConnValuePrefix}{t}")));
        // Build configuration
        var cb = new ConfigurationBuilder();
        cb.AddInMemoryCollection(connStr);
        var config = cb.Build();
            
        // Create the mocked store with only 1 method setup for moq
        var tenantStoreMock = new Mock<TenantStore>(config, new NullLoggerFactory()) {
            CallBase = true // IMPORTANT if original methods shall be used
        };
        // Just override one method:
        tenantStoreMock.Setup(s => s.ReadTenantConfigFile(It.IsAny<string>())).Returns((string tenantName) =>
        {
            var tenant = new TenantContext
            {
                Identifier = tenantName,
                IsDefault = tenantName.Equals("DefaultTenant"),
                Guid = Guid.NewGuid(),
                Filename = tenantName,
                SiteContext = {UrlSegmentValue = tenantName },
                DbContext = {ConnectionKey = ConnKeyPrefix + tenantName},
                OrganizationContext = {Name = "Long: " + tenantName, ShortName = "Short: " + tenantName}
            };
            return tenant.Serialize();
        });
        tenantStoreMock.Setup(s => s.LoadTenants());
        _store = tenantStoreMock.Object;
        _store.GetTenantConfigurationFiles = () => _tenantNames.ToArray();
    }

    [Test]
    public void LoadTenants_Test()
    {
        IReadOnlyDictionary<string, ITenantContext> tenants = new Dictionary<string, ITenantContext>();
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => tenants = _store.LoadTenants().GetTenants());
            Assert.AreEqual(_tenantNames.Count, tenants.Count);
            foreach (var name in _tenantNames)
            {
                var tenant = _store.GetTenantByIdentifier(name);
                Assert.AreEqual(name, tenant?.Identifier);
                Assert.AreEqual(name, tenant?.Filename);
                Assert.AreEqual(ConnKeyPrefix + name, tenant?.DbContext.ConnectionKey);
                Assert.AreEqual(ConnValuePrefix + name, tenant?.DbContext.ConnectionString);
                Assert.IsTrue(tenant != null && tenant.OrganizationContext.Name.StartsWith("Long") && tenant.OrganizationContext.ShortName.StartsWith("Short"));
                Assert.IsTrue(tenant?.OrganizationContext.Tenant != null && tenant.SiteContext.Tenant != null && tenant.TournamentContext.Tenant != null);
                if(tenant is { IsDefault: true }) Assert.AreEqual(tenant.Identifier, tenant.SiteContext.UrlSegmentValue);
            }
        });
    }

    [Test]
    public void GetTenantByIdentifier_Test()
    {
        Assert.DoesNotThrow(() => _ = _store.LoadTenants());
        var tenant = _store.GetTenantByIdentifier("Tenant1");
        Assert.AreEqual("Tenant1", tenant?.Identifier);
    }
        
    [Test]
    public void GetTenantByUrlSegment_Test()
    {
        Assert.DoesNotThrow(() => _ = _store.LoadTenants());
        var tenant = _store.GetTenantByUrlSegment("Tenant1");
        Assert.AreEqual("Tenant1", tenant?.SiteContext.UrlSegmentValue);
    }
        
    [Test]
    public void GetDefaultTenant_Test()
    {
        Assert.DoesNotThrow(() => _ = _store.LoadTenants());
        var tenant = _store.GetDefaultTenant();
        Assert.AreEqual("DefaultTenant", tenant?.Identifier);
    }
        
    [Test]
    public void TryAddTenant_Test()
    {
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _ = _store.LoadTenants());
            Assert.IsFalse(_store.TryAddTenant(new TenantContext {Identifier = "Tenant1"}));
            Assert.IsTrue(_store.TryAddTenant(new TenantContext {Identifier = "New_Tenant"}));
        });
    }
        
    [Test]
    public void TryRemoveTenant_Test()
    {
        Assert.Multiple(() =>
        {
            Assert.DoesNotThrow(() => _ = _store.LoadTenants());
            Assert.IsTrue(_store.TryRemoveTenant("Tenant1"));
            Assert.IsNull(_store.GetTenantByIdentifier("Tenant1"));
        });
    }
        
    [Test]
    public void TryUpdateTenant_Test()
    {
        var newGuid = Guid.Parse("99999999-9999-9999-9999-999999999999");
        _ = _store.LoadTenants();
        var updateTenant = _store.GetTenantByUrlSegment("Tenant1");
        if (updateTenant != null) updateTenant.Guid = newGuid;

        Assert.Multiple(() =>
        {
            Assert.IsTrue(_store.TryUpdateTenant("Tenant1", updateTenant!));
            Assert.AreEqual(newGuid, _store.GetTenantByIdentifier("Tenant1")?.Guid);
        });
    }
        
    [Test]
    public void TryUpdateTenant_WithWrongIdentifier_Test()
    {
        var newGuid = Guid.Parse("99999999-9999-9999-9999-999999999999");
        _ = _store.LoadTenants();
        var updateTenant = _store.GetTenantByUrlSegment("Tenant1");
        if (updateTenant != null)
        {
            updateTenant.Guid = newGuid;
            updateTenant.Identifier = "A-new-tenant"; // will be set to identifier by TryUpdateTenant
        }

        Assert.Multiple(() =>
        {
            Assert.IsTrue(_store.TryUpdateTenant("Tenant1", updateTenant!));
            Assert.AreEqual(newGuid, _store.GetTenantByIdentifier("Tenant1")?.Guid);
        });
    }
}
