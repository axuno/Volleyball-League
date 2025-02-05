using System.ComponentModel.DataAnnotations;

namespace League.Models.TenantContent;

/// <summary>
/// Represents a content item for a tenant.
/// </summary>
public class ContentItem
{
    /// <summary>
    /// The place where the content item is displayed in the menu.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// The titel of the page.
    /// </summary>
    [Required]
    public string? PageTitel { get; set; }

    /// <summary>
    /// A short description of the content. This is used for SEO purposes.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The title of the menu item used for navigation.
    /// </summary>
    [Required]
    public string? MenuTitel { get; set; }

    /// <summary>
    /// The topic of the content item. It is used in the Url and the filename.
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Whether the content item is active. Inactive items will never be displayed.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The date when the content item was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date when the content item gets published.
    /// </summary>
    public DateTime PubDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Determines whether the content item is visible.
    /// </summary>
    /// <returns></returns>
    public bool IsVisible() => PubDate <= DateTime.UtcNow && IsActive;

    /// <summary>
    /// Lazy-reads the HTML content of the page from the file system.
    /// </summary>
    public Func<Task<string>>? ReadHtmlContentAsync { get; set; }
}
