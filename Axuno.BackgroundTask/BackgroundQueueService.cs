using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask;

public class BackgroundQueueService : BackgroundService
{
    private readonly ILogger<BackgroundQueueService> _logger;

    public BackgroundQueueService(IBackgroundQueue taskQueue,
        ILogger<BackgroundQueueService> logger, IOptions<BackgroundQueueServiceConfig> config)
    {
        TaskQueue = taskQueue;
        _logger = logger;
        Config = config.Value ?? new BackgroundQueueServiceConfig();
    }

    /// <summary>
    /// Gets the configuration for this service.
    /// </summary>
    public BackgroundQueueServiceConfig Config { get; }

    /// <summary>
    /// Gets the <see cref="IBackgroundQueue"/> of this service.
    /// </summary>
    public IBackgroundQueue TaskQueue { get; }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Config.PollQueueDelay, cancellationToken);
                _logger.LogDebug($"TaskItem dequeuing.");
                var taskItemReference = TaskQueue.DequeueTask();
                _logger.LogDebug($"TaskItem start executing.");
                await TaskQueue.RunTaskAsync(taskItemReference, cancellationToken);
                _logger.LogDebug($"TaskItem completed.");
            }
            catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
            {
                _logger.LogError(e, $"{nameof(BackgroundQueueService)} canceled.");
                //break;
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error occurred executing TaskItem.");
            }
        }
    }

    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug($"{nameof(BackgroundQueueService)} is stopping.");
        await base.StopAsync(cancellationToken);
        _logger.LogDebug($"{nameof(BackgroundQueueService)} stopped.");
    }
}