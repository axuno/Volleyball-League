using League.Models.TenantContent;
using TournamentManager.MultiTenancy;

namespace League.MultiTenancy;

public interface ITenantContentProvider
{
    Task<List<ContentItem>> GetContentItems(ITenantContext tenant);
    Task<ContentItem?> GetContentItem(ITenantContext tenant, string topic);
    Task SaveContentItem(ITenantContext tenantContext, ContentItem contentItem, string html);
    Task DeleteContentItem(ITenantContext tenantContext, string topic);
}
