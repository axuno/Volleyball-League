using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask;

/// <summary>
/// Processes background tasks with a configurable upper bound on parallel executions
/// (<see cref="ConcurrentBackgroundQueueServiceConfig.MaxConcurrentCount"/>).
/// </summary>
public class ConcurrentBackgroundQueueService : BackgroundService
{
    private readonly ILogger<ConcurrentBackgroundQueueService> _logger;
    private int _concurrentTaskCount;
    private volatile bool _applicationStopping;
    private volatile bool _applicationStopped;

    public ConcurrentBackgroundQueueService(
        IBackgroundQueue taskQueue,
        ILogger<ConcurrentBackgroundQueueService> logger,
        IOptions<ConcurrentBackgroundQueueServiceConfig> config,
        IHostApplicationLifetime lifetime)
    {
        TaskQueue = taskQueue;
        _logger = logger;
        Config = config.Value;

        if (Config.MaxConcurrentCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(config), $"{nameof(Config.MaxConcurrentCount)} Must be > 0.");
        if (Config.PollQueueDelay <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(config), $"{nameof(Config.PollQueueDelay)}: Must be positive.");

        lifetime.ApplicationStopping.Register(() => _applicationStopping = true);
        lifetime.ApplicationStopped.Register(() => _applicationStopped = true);
    }

    public ConcurrentBackgroundQueueServiceConfig Config { get; }
    public IBackgroundQueue? TaskQueue { get; private set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await Task.Delay(Config.PollQueueDelay, stoppingToken).ConfigureAwait(false);

                if (!CanProcess())
                    continue;

                var started = StartAvailableTasks(stoppingToken);

                if (started > 0)
                {
                    _logger.LogDebug("{Service} started {Started} task(s). Active={Active}/{Limit}",
                        nameof(ConcurrentBackgroundQueueService),
                        started,
                        Volatile.Read(ref _concurrentTaskCount),
                        Config.MaxConcurrentCount);
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
                nameof(ConcurrentBackgroundQueueService), phase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service} failed.", nameof(ConcurrentBackgroundQueueService));
            TaskQueue = null;
        }
    }

    private bool CanProcess()
    {
        if (TaskQueue == null)
            return false;
        if (TaskQueue.Count == 0)
            return false;
        return Volatile.Read(ref _concurrentTaskCount) < Config.MaxConcurrentCount;
    }

    private int StartAvailableTasks(CancellationToken stoppingToken)
    {
        if (TaskQueue == null)
            return 0;

        var capacity = Config.MaxConcurrentCount - Volatile.Read(ref _concurrentTaskCount);
        if (capacity <= 0)
            return 0;

        var started = 0;

        while (capacity > 0 && TaskQueue.Count > 0)
        {
            IBackgroundTask taskItem;
            try
            {
                taskItem = TaskQueue.DequeueTask();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Service} failed while dequeuing task.", nameof(ConcurrentBackgroundQueueService));
                break;
            }

            _ = StartTaskExecutionAsync(taskItem, stoppingToken);
            started++;
            capacity--;
        }

        return started;
    }

    private Task StartTaskExecutionAsync(IBackgroundTask taskItem, CancellationToken stoppingToken)
    {
        Interlocked.Increment(ref _concurrentTaskCount);

        return ExecuteAsyncCore();

        async Task ExecuteAsyncCore()
        {
            try
            {
                if (TaskQueue == null)
                    throw new InvalidOperationException($"{nameof(TaskQueue)} must not be null when starting a task.");

                await TaskQueue.RunTaskAsync(taskItem, stoppingToken).ConfigureAwait(false);
                _logger.LogDebug("{Service} task completed. Active={Active}", //NOSONAR
                    nameof(ConcurrentBackgroundQueueService),
                    Volatile.Read(ref _concurrentTaskCount));
            }
            catch (OperationCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Service} task canceled (per-task cancellation). Active={Active}", //NOSONAR
                    nameof(ConcurrentBackgroundQueueService),
                    Volatile.Read(ref _concurrentTaskCount));
            }
            catch (TaskCanceledException) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("{Service} task canceled (TaskCanceledException). Active={Active}", //NOSONAR
                    nameof(ConcurrentBackgroundQueueService),
                    Volatile.Read(ref _concurrentTaskCount));
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host shutdown; swallow.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Service} task failed. Active={Active}",
                    nameof(ConcurrentBackgroundQueueService),
                    Volatile.Read(ref _concurrentTaskCount));
            }
            finally
            {
                Interlocked.Decrement(ref _concurrentTaskCount);
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Service} is stopping. Active={Active}",
            nameof(ConcurrentBackgroundQueueService),
            Volatile.Read(ref _concurrentTaskCount));

        await base.StopAsync(cancellationToken).ConfigureAwait(false);

        _logger.LogDebug("{Service} stopped. Remaining active tasks={Active}",
            nameof(ConcurrentBackgroundQueueService),
            Volatile.Read(ref _concurrentTaskCount));
    }
}
