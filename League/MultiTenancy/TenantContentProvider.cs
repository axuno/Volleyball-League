using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Axuno.Tools.FileSystem;
using League.Models.TenantContent;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy;

/// <summary>
/// Provides read/write/delete access to tenant-specific content items.
/// </summary>
public class TenantContentProvider : ITenantContentProvider
{
    public const string TenantsContentFolder = "pages";

    private readonly TenantStore _tenantStore;
    private readonly ILogger<TenantContentProvider> _logger;
    // Semaphore to ensure that only one thread at a time can access while reading or writing
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    // Keys are case-insensitive tenant identifiers
    private ConcurrentDictionary<string, List<ContentItem>>? _allTenantsContent;
    // Will be initialized in the constructor
    private DelayedFileSystemWatcher _tenantContentFileWatcher = null!;
    private readonly string _contentPath;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="webHostEnvironment"></param>
    /// <param name="tenantStore"></param>
    /// <param name="logger"></param>
    public TenantContentProvider(IWebHostEnvironment webHostEnvironment, TenantStore tenantStore,
        ILogger<TenantContentProvider> logger)
    {
        _tenantStore = tenantStore;
        _logger = logger;
        _contentPath = Path.Combine(webHostEnvironment.WebRootPath, TenantsContentFolder);

        EnsureContentsFolder();
        InitializeFileWatcher();
    }

    private void EnsureContentsFolder()
    {
        var folderExists = false;
        try
        {
            folderExists = Directory.Exists(_contentPath);
            if (folderExists) return;

            Directory.CreateDirectory(_contentPath);
            _logger.LogInformation("Missing tenant content folder created with {Path}", _contentPath);
        }
        finally
        {
            if (!folderExists) _logger.LogError("Creation of tenant content folder failed for {Path}", _contentPath);
        }
    }

    /// <summary>
    /// Initializes the file watcher for the tenant content folder.
    /// </summary>
    /// <remarks>
    /// Watch for changes in the tenant content folder, that are not initiated by this class.
    /// The watcher will not raise events for changes manged by this class.
    /// Only watch for changes of JSON files with metadata, because HTML is always read from storage.
    /// </remarks>
    private void InitializeFileWatcher()
    {
        _tenantContentFileWatcher =
            new(_contentPath)
            {
                Filters = { "*.json" },
                ConsolidationInterval = 1000,
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = false
            };

        _tenantContentFileWatcher.Changed += async (sender, args) => await OnFileChanged(args);
        _tenantContentFileWatcher.Created += async (sender, args) => await OnFileChanged(args);
        _tenantContentFileWatcher.Deleted += async (sender, args) => await OnFileChanged(args);
        _tenantContentFileWatcher.Renamed += async (sender, args) => await OnFileChanged(args);
        _tenantContentFileWatcher.Error += (sender, args) =>
        {
            _logger.LogError(args.GetException(), "Error in {FileWatcher}", nameof(_tenantContentFileWatcher));
        };
    }

    private async Task OnFileChanged(FileSystemEventArgs e)
    {
        _logger.LogInformation("File change detected: {ChangeType} {FullPath}", e.ChangeType, e.FullPath);
        // There are not many tenant files, so we can afford to reload all content
        await ReadAllTenantsContent(true);
    }

    private async Task ReadAllTenantsContent(bool force)
    {
        if (_allTenantsContent != null && !force)
        {
            return;
        }

        // Lock while creating a new dictionary
        await _semaphore.WaitAsync();
        _allTenantsContent = new(StringComparer.OrdinalIgnoreCase);
        _semaphore.Release();

        var tenants = _tenantStore.GetTenants();
        foreach (var tenantContext in tenants.Values)
        {
            var tenantContent = await ReadContentItems(tenantContext);

            _allTenantsContent.TryAdd(tenantContext.Identifier, tenantContent);
        }
    }

    private async Task<List<ContentItem>> ReadContentItems(ITenantContext tenantContext)
    {
        var contentPath = _contentPath;

        var files = Directory.GetFiles(contentPath, $"{tenantContext.SiteContext.FolderName}_*.json");
        var contentItems = new List<ContentItem>();
        foreach (var file in files)
        {
            var contentMeta = await File.ReadAllTextAsync(file);
            var contentItem = JsonSerializer.Deserialize<ContentItem?>(contentMeta);
            if (contentItem == null)
            {
                _logger.LogWarning("Error deserializing content item from file: {File}", file);
                continue;
            }

            contentItem.ReadHtmlContentAsync = async () =>
                await File.ReadAllTextAsync(Path.Combine(contentPath, Path.GetFileNameWithoutExtension(file) + ".html"));
            contentItems.Add(contentItem);
        }

        return contentItems;
    }

    /// <summary>
    /// Gets all content items for a tenant.
    /// </summary>
    public async Task<List<ContentItem>> GetContentItems(ITenantContext tenant)
    {
        if (_allTenantsContent == null)
        {
            await ReadAllTenantsContent(false);
        }
        return _allTenantsContent!.TryGetValue(tenant.Identifier, out var contentItems)
            ? contentItems
            : [];
    }

    /// <summary>
    /// Gets a tenant content item by topic.
    /// </summary>
    public async Task<ContentItem?> GetContentItem(ITenantContext tenant, string topic)
    {
        var allItems = await GetContentItems(tenant);
        return allItems.FirstOrDefault(c => c.Topic.Equals(topic, StringComparison.InvariantCultureIgnoreCase));
    }

    /// <summary>
    /// Saves the content item as JSON and the HTML content to the file system.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="contentItem"></param>
    /// <param name="html"></param>
    /// <returns></returns>
    public async Task SaveContentItem(ITenantContext tenantContext, ContentItem contentItem, string html)
    {
        var contentPath = _contentPath;

        try
        {
            // Disable while we have a managed content change
            _tenantContentFileWatcher.EnableRaisingEvents = false;

            await ReadAllTenantsContent(true);

            var fileBasePath = Path.Combine(contentPath, $"{tenantContext.SiteContext.FolderName}_{contentItem.Topic}");
            var contentMeta = JsonSerializer.Serialize(contentItem);
            try
            {
                await _semaphore.WaitAsync();
                await File.WriteAllTextAsync($"{fileBasePath}.html", html, Encoding.UTF8);
                await File.WriteAllTextAsync($"{fileBasePath}.json", contentMeta, Encoding.UTF8);

                if (_allTenantsContent != null && _allTenantsContent.TryGetValue(tenantContext.Identifier, out var contentItems))
                {
                    var existingItem = contentItems.FirstOrDefault(c => c.Topic == contentItem.Topic);
                    if (existingItem != null)
                    {
                        contentItems.Remove(existingItem);
                    }
                    contentItems.Add(contentItem);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        finally
        {
            _tenantContentFileWatcher.EnableRaisingEvents = true;
        }
    }

    /// <summary>
    /// Deletes a content item by topic.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="topic"></param>
    /// <returns></returns>
    public async Task DeleteContentItem(ITenantContext tenantContext, string topic)
    {
        var contentPath = _contentPath;
        var jsonFilePath = Path.Combine(contentPath, $"{tenantContext.SiteContext.FolderName}_{topic}.json");
        var htmlFilePath = Path.Combine(contentPath, $"{tenantContext.SiteContext.FolderName}_{topic}.html");

        await _semaphore.WaitAsync();
        try
        {
            // Disable while we have a managed deletion
            _tenantContentFileWatcher.EnableRaisingEvents = false;
            if (File.Exists(jsonFilePath))
            {
                File.Delete(jsonFilePath);
                File.Delete(htmlFilePath);
            }

            if (_allTenantsContent != null && _allTenantsContent.TryGetValue(tenantContext.Identifier, out var contentItems))
            {
                var contentItem = contentItems.FirstOrDefault(c => c.Topic == topic);
                if (contentItem != null)
                {
                    contentItems.Remove(contentItem);
                }
            }
        }
        finally
        {
            _tenantContentFileWatcher.EnableRaisingEvents = true;
            _semaphore.Release();
        }
    }
}
