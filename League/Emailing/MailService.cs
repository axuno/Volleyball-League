using System;
using MailMergeLib.MessageStore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TournamentManager.MultiTenancy;

namespace MailMergeLib.AspNet;

public interface IMailMergeService
{
    IMessageStore MessageStore { get; }
    Settings Settings { get; }
    MailMergeSender Sender { get; }
    MailMergeMessage CreateStandardMessage();
}

public class MailMergeServiceConfig
{
    public Settings? Settings { get; set; }
    public IMessageStore? MessageStore { get; set; }
}

public class MailMergeService : IMailMergeService
{
    private readonly ITenantContext _tenantContext;

    public MailMergeService(IOptions<MailMergeServiceConfig> serviceConfig, ITenantContext tenantContext )
    {
        Settings = serviceConfig.Value.Settings!;
        MessageStore = serviceConfig.Value.MessageStore!;
        _tenantContext = tenantContext;
    }
    public Settings Settings { get; }
    public IMessageStore MessageStore { get; }
    public MailMergeSender Sender => new() { Config = Settings.SenderConfig };
    public MailMergeMessage CreateStandardMessage()
    {
        var mmm = new MailMergeMessage
        {
            Config = Settings.MessageConfig,
            PlainText = string.Empty,
            HtmlText = string.Empty,
            Subject = string.Empty,
        };

        return mmm;
    }
}

/// <summary>
/// Extension methods for adding MailMerge services to the DI container.
/// </summary>
public static class MailMergeServiceCollectionExtensions
{
    /// <summary>
    /// Adds a transient <see cref="IMailMergeService"/> to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> the service will be added to.</param>
    /// <param name="config">The <see cref="MailMergeServiceConfig"/> which will configure the <see cref="IMailMergeService"/>.</param>
    /// <returns>The <see cref="T:IServiceCollection" /> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMailMergeService(this IServiceCollection services, Action<MailMergeServiceConfig> config)
    {
        // Null-checks are part the extension methods:
        services.Configure<MailMergeServiceConfig>(config);
        services.TryAddTransient<IMailMergeService, MailMergeService>();
        return services;
    }
}
