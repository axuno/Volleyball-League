using System.Text.Json;
using League.Models.TenantContent;
using League.MultiTenancy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TournamentManager.MultiTenancy;

namespace League.Tests;

[TestFixture]
public class TenantContentProviderTests
{
    private Mock<IWebHostEnvironment> _mockWebHostEnvironment;
    private Mock<ILogger<TenantContentProvider>> _mockLogger;
    private Mock<TenantStore> _mockTenantStore;
    private TenantContentProvider _tenantContentProvider;
    private string _tempDirectory;

    [SetUp]
    public void SetUp()
    {
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockLogger = new Mock<ILogger<TenantContentProvider>>();
        _mockTenantStore = new Mock<TenantStore>(Mock.Of<IConfiguration>(), Mock.Of<ILoggerFactory>());

        // Create a temporary directory
        _tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempDirectory);

        _mockWebHostEnvironment.Setup(env => env.WebRootPath).Returns(_tempDirectory);
        Directory.CreateDirectory(Path.Combine(_tempDirectory, "tenants-content"));

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Identifier).Returns("tenant1");
        tenantContext.Setup(t => t.SiteContext).Returns(new SiteContext { FolderName = "tenant1" });

        var tenants = new Dictionary<string, ITenantContext>
        {
            { "tenant1", tenantContext.Object }
        };

        _mockTenantStore.Setup(store => store.GetTenants()).Returns(tenants);

        _tenantContentProvider = new TenantContentProvider(
            _mockWebHostEnvironment.Object,
            _mockTenantStore.Object,
            _mockLogger.Object
        );
        
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up the temporary directory
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, true);
        }
    }

    [Test]
    public async Task GetContentItems_ShouldReturnContentItems()
    {
        // Arrange
        var tenantContext = new Mock<ITenantContext>();
        var siteContext = new SiteContext { FolderName = "tenant1" };
        tenantContext.Setup(t => t.Identifier).Returns("tenant1");
        tenantContext.Setup(t => t.SiteContext).Returns(siteContext);

        // Act
        await _tenantContentProvider.SaveContentItem(tenantContext.Object, new ContentItem {Topic = "test-topic" }, "<html></html>");
        var contentItems = await _tenantContentProvider.GetContentItems(tenantContext.Object);
        var contentItem = await _tenantContentProvider.GetContentItem(tenantContext.Object, "test-topic");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(contentItems.Count, Is.EqualTo(1));
            Assert.That(contentItems[0].Topic, Is.EqualTo("test-topic"));
            Assert.That(contentItem?.Topic, Is.EqualTo("test-topic"));
        });
    }

    [Test]
    public async Task SaveContentItem_ShouldSaveContentItem()
    {
        // Arrange
        var tenantContext = new Mock<ITenantContext>();
        var siteContext = new SiteContext { FolderName = "tenant1" };
        tenantContext.Setup(t => t.Identifier).Returns("tenant1");
        tenantContext.Setup(t => t.SiteContext).Returns(siteContext);

        var contentItem = new ContentItem { Topic = "test-topic" };
        var htmlContent = "<html></html>";

        // Act
        await _tenantContentProvider.SaveContentItem(tenantContext.Object, contentItem, htmlContent);

        // Assert
        var contentPath = Path.Combine(_tempDirectory, TenantContentProvider.TenantsContentFolder);
        var jsonFilePath = Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.json");
        var htmlFilePath = Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.html");

        var savedContentItem = JsonSerializer.Deserialize<ContentItem>(await File.ReadAllTextAsync(jsonFilePath));

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(File.Exists(jsonFilePath), Is.True);
            Assert.That(File.Exists(htmlFilePath), Is.True);

            Assert.That(savedContentItem?.Topic, Is.EqualTo("test-topic"));
            Assert.That(await File.ReadAllTextAsync(htmlFilePath), Is.EqualTo(htmlContent));
        });
    }

    [Test]
    public async Task DeleteContentItem_ShouldDeleteContentItem()
    {
        // Arrange
        var tenantContext = new Mock<ITenantContext>();
        var siteContext = new SiteContext { FolderName = "tenant1" };
        tenantContext.Setup(t => t.Identifier).Returns("tenant1");
        tenantContext.Setup(t => t.SiteContext).Returns(siteContext);

        var contentItem = new ContentItem { Topic = "test-topic" };
        var contentPath = Path.Combine(_tempDirectory, TenantContentProvider.TenantsContentFolder);
        Directory.CreateDirectory(contentPath);
        await File.WriteAllTextAsync(Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.json"), JsonSerializer.Serialize(contentItem));
        await File.WriteAllTextAsync(Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.html"), "<html></html>");

        // Act
        await _tenantContentProvider.DeleteContentItem(tenantContext.Object, "test-topic");

        // Assert
        var jsonFilePath = Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.json");
        var htmlFilePath = Path.Combine(contentPath, $"{siteContext.FolderName}_test-topic.html");

        Assert.Multiple(() =>
        {
            Assert.That(File.Exists(jsonFilePath), Is.False);
            Assert.That(File.Exists(htmlFilePath), Is.False);
            Assert.That(Directory.GetFiles(contentPath), Is.Empty);
        });
    }

    private class MockTenantStore : TenantStore
    {
        public MockTenantStore(IConfiguration configuration, ILoggerFactory? loggerFactory = null)
            : base(configuration, loggerFactory)
        {
        }

        public new IReadOnlyDictionary<string, ITenantContext> GetTenants()
        {
            var tenants = new Dictionary<string, ITenantContext>
            {
                { "tenant1", new Mock<ITenantContext>().Object }
            };
            return tenants;
        }
    }
}
