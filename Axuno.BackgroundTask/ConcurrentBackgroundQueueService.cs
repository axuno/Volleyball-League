using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask;

/// <summary>
/// Class for processing background tasks.
/// </summary>
public class ConcurrentBackgroundQueueService : BackgroundService
{
    private readonly ILogger<ConcurrentBackgroundQueueService> _logger;
    private readonly ManualResetEvent _resetEvent = new(true);
    private readonly object _locker = new();
    private int _concurrentTaskCount;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="taskQueue"></param>
    /// <param name="logger"></param>
    /// <param name="config"></param>
    public ConcurrentBackgroundQueueService(IBackgroundQueue taskQueue,
        ILogger<ConcurrentBackgroundQueueService> logger, IOptions<ConcurrentBackgroundQueueServiceConfig> config)
    {
        TaskQueue = taskQueue;
        _logger = logger;
        Config = config.Value ?? new ConcurrentBackgroundQueueServiceConfig();
    }

    /// <summary>
    /// Gets the configuration for this service.
    /// </summary>
    public ConcurrentBackgroundQueueServiceConfig Config { get; }

    /// <summary>
    /// Gets the <see cref="IBackgroundQueue"/> of this service.
    /// </summary>
    public IBackgroundQueue? TaskQueue { get; private set; }

    /// <summary>
    /// Execute queued tasks.
    /// </summary>
    /// <param name="stoppingToken"></param>
    /// <returns>A <see cref="Task"/>.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await WaitForStartSignal();

        var taskListReference = new Queue<IBackgroundTask>();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                stoppingToken.ThrowIfCancellationRequested();
                await Task.Delay(Config.PollQueueDelay, stoppingToken);

                EnqueuePendingTasks(taskListReference);
                await ExecuteTaskChunk(taskListReference, stoppingToken);
            }
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(ex, "{Service} was canceled.", nameof(ConcurrentBackgroundQueueService));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Service} failed.", nameof(ConcurrentBackgroundQueueService));
            TaskQueue = null; // we can't process the queue anymore
        }
        finally
        {
            SignalServiceStopped();
        }
    }

    private Task WaitForStartSignal()
    {
        lock (_locker)
        {
            _resetEvent.WaitOne();
        }

        return Task.CompletedTask;
    }

    private void EnqueuePendingTasks(Queue<IBackgroundTask> taskListReference)
    {
        if (_concurrentTaskCount >= Config.MaxConcurrentCount || TaskQueue == null || TaskQueue.Count == 0)
            return;

        Interlocked.Increment(ref _concurrentTaskCount);
        _logger.LogDebug("Num of tasks: {ConcurrentTaskCount}", _concurrentTaskCount);

        taskListReference.Enqueue(TaskQueue.DequeueTask());
    }

    private async Task ExecuteTaskChunk(Queue<IBackgroundTask> taskListReference, CancellationToken stoppingToken)
    {
        if (taskListReference.Count == 0)
            return;

        var taskChunk = new List<Task>();

        try
        {
            while (taskListReference.Count > 0)
            {
                stoppingToken.ThrowIfCancellationRequested();

                using var taskCancellation = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
                var task = (TaskQueue?.RunTaskAsync(taskListReference.Dequeue(), taskCancellation.Token)) ??
                           throw new NullReferenceException($"{nameof(TaskQueue)} cannot be null here.");

                if (task.Exception != null)
                    throw task.Exception;

                taskChunk.Add(task);
            }

            await ExecuteTaskChunkAndWait(taskChunk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing TaskItem.");
        }
        finally
        {
            Interlocked.Decrement(ref _concurrentTaskCount);
            taskListReference.Clear();
        }
    }

    private async Task ExecuteTaskChunkAndWait(List<Task> taskChunk)
    {
        if (taskChunk.Count == 0)
            return;

        try
        {
            await Task.WhenAll(taskChunk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Task chunk exception");
            var taskChunkExceptions = taskChunk.Where(task => task.Exception != null).Select(task => task.Exception!).ToList();
            if (taskChunkExceptions.Count > 0)
            {
                _logger.LogError(new AggregateException(taskChunkExceptions), "Task chunk aggregate exception");
            }
        }
    }

    private void SignalServiceStopped()
    {
        lock (_locker)
        {
            _resetEvent.Reset();
            _resetEvent.Set();
        }
    }


    /// <summary>
    /// Stops the service.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogDebug("{Service} is stopping.", nameof(ConcurrentBackgroundQueueService));
        await base.StopAsync(cancellationToken);
        _logger.LogDebug("{Service} stopped.", nameof(ConcurrentBackgroundQueueService));
    }
}
