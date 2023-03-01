using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Axuno.BackgroundTask;

/// <summary>
/// Extensions for BackgroundTask services.
/// </summary>
public static class BackgroundQueueServiceExtensions
{
    /// <summary>
    /// Add an <see cref="IHostedService"/> registration for a <see cref="ConcurrentBackgroundQueueService"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>Returns the <see cref="IServiceCollection"/> after the <see cref="ConcurrentBackgroundQueueService"/> has been registered.</returns>
    public static IServiceCollection AddConcurrentBackgroundQueueService(this IServiceCollection services, Action<ConcurrentBackgroundQueueServiceConfig>? config = null)
    {
        // Null-checks are part the extension methods:
        services.Configure<ConcurrentBackgroundQueueServiceConfig>(config ?? new Action<ConcurrentBackgroundQueueServiceConfig>(option => {}));
        services.AddHostedService<ConcurrentBackgroundQueueService>();

        return services;
    }

    /// <summary>
    /// Add an <see cref="IHostedService"/> registration for a <see cref="BackgroundQueueService"/>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <returns>Returns the <see cref="IServiceCollection"/> after the <see cref="BackgroundQueueService"/> has been registered.</returns>
    public static IServiceCollection AddBackgroundQueueService(this IServiceCollection services, Action<BackgroundQueueServiceConfig>? config = null)
    {
        // Null-checks are part of the extension methods:
        services.Configure<BackgroundQueueServiceConfig>(config ?? new Action<BackgroundQueueServiceConfig>(options => { }));
        services.AddHostedService<BackgroundQueueService>();

        return services;
    }
}