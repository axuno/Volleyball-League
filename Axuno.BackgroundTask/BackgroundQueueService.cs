using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask;

public class BackgroundQueueService : BackgroundService
{
    private readonly ILogger<BackgroundQueueService> _logger;
    private volatile bool _applicationStopping;
    private volatile bool _applicationStopped;

    public BackgroundQueueService(
        IBackgroundQueue taskQueue,
        ILogger<BackgroundQueueService> logger,
        IOptions<BackgroundQueueServiceConfig> config,
        IHostApplicationLifetime lifetime)
    {
        TaskQueue = taskQueue;
        _logger = logger;
        Config = config.Value;

        lifetime.ApplicationStopping.Register(() => _applicationStopping = true);
        lifetime.ApplicationStopped.Register(() => _applicationStopped = true);
    }

    /// <summary>
    /// Gets the configuration for this service.
    /// </summary>
    public BackgroundQueueServiceConfig Config { get; }

    /// <summary>
    /// Gets the <see cref="IBackgroundQueue"/> of this service.
    /// </summary>
    public IBackgroundQueue TaskQueue { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();

                await Task.Delay(Config.PollQueueDelay, stoppingToken);

                if (TaskQueue.Count == 0)
                    continue;

                _logger.LogDebug("Dequeuing task item.");
                var taskItemReference = await TaskQueue.DequeueTaskAsync(stoppingToken);

                try
                {
                    _logger.LogDebug("Executing task item.");
                    await TaskQueue.RunTaskAsync(taskItemReference, stoppingToken);
                    _logger.LogDebug("Task item completed.");
                }
                catch (OperationCanceledException oce) when (!stoppingToken.IsCancellationRequested)
                {
                    // Per-task cancellation (e.g. timeout). Not a service cancellation.
                    _logger.LogDebug("{Service} task canceled (likely timeout or per-task cancellation): {Message}", //NOSONAR
                        nameof(BackgroundQueueService), oce.Message); 
                }
                catch (TaskCanceledException tce) when (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogDebug("{Service} task canceled (TaskCanceledException): {Message}", //NOSONAR
                        nameof(BackgroundQueueService), tce.Message); 
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred executing task item.");
                }
            }
        }
        catch (TaskCanceledException)
        {
            string phase;
            if (_applicationStopped)
            {
                phase = "application stopped";
            }
            else if (_applicationStopping)
            {
                phase = "application stopping";
            }
            else
            {
                phase = "service stopping";
            }

            _logger.LogInformation("{Service} canceled ({Phase}; expected host shutdown).", //NOSONAR
                nameof(BackgroundQueueService), phase); 
        }
        // NOTE: Do NOT catch a broad OperationCanceledException here; per-task OCEs must not stop the loop.
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service} failed unexpectedly.", nameof(BackgroundQueueService));
        }
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Service} is stopping.", nameof(BackgroundQueueService));
        await base.StopAsync(cancellationToken);
        _logger.LogDebug("{Service} stopped.", nameof(BackgroundQueueService));
    }
}
