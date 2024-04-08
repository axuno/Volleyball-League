using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask;

/// <summary>
/// Manages a <see cref="ConcurrentQueue{T}"/>, usually used for background task operations.
/// </summary>
public class BackgroundQueue : IBackgroundQueue
{
    private readonly Action<Exception> _onException;
    private readonly ILogger<BackgroundQueue> _logger;
    internal readonly ConcurrentQueue<IBackgroundTask> TaskItems = new();
    private readonly SemaphoreSlim _signal;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="logger">The logger to use.</param>
    public BackgroundQueue(IOptions<BackgroundQueueConfig> config, ILogger<BackgroundQueue> logger)
    {
        _onException = config.Value.OnException;
        _logger = logger;
        // https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=netstandard-2.0
        _signal = new SemaphoreSlim(0);
    }

    /// <summary>
    /// Queues a <see cref="IBackgroundTask"/>.
    /// </summary>
    /// <param name="taskItem">The <see cref="IBackgroundTask"/> to append to the queue.</param>
    public void QueueTask(IBackgroundTask taskItem)
    {
        ArgumentNullException.ThrowIfNull(taskItem);

        TaskItems.Enqueue(taskItem);
        _signal.Release(); // increase the semaphore count for each item
        _logger.LogDebug("Number of queued TaskItems is {taskItemCount}", _signal.CurrentCount);
    }

    /// <summary>
    /// De-queues the next <see cref="IBackgroundTask"/> from the queue.
    /// </summary>
    /// <returns>Returns the next <see cref="IBackgroundTask"/> from the queue.</returns>
    public IBackgroundTask DequeueTask()
    {
        if (TaskItems.TryDequeue(out var nextTaskItem)) 
            return nextTaskItem;

        _logger.LogDebug("No TaskItem could be dequeued.");
        return new BackgroundTaskEmpty();
    }

    /// <summary>
    /// Executes the given <see cref="IBackgroundTask"/>.
    /// </summary>
    /// <param name="task"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>Returns the <see cref="Task"/> which was started.</returns>
    public async Task RunTaskAsync(IBackgroundTask? task, CancellationToken cancellationToken)
    {
        if (task == null) return;

        try
        {
            await _signal.WaitAsync(cancellationToken);

            await CancelAfterAsync(task.RunAsync, task.Timeout, cancellationToken);
            _logger.LogDebug("Task completed.");
        }
        catch (Exception e) when (e is TaskCanceledException || e is OperationCanceledException)
        {
            _logger.LogError(e, $"Task canceled when executing a {nameof(IBackgroundTask)}.");
            // _onException will deliberately not be called
            throw;
        }
        catch (Exception e) when (e is TimeoutException)
        {
            _logger.LogError(e, $"Task timed out.");
            _onException?.Invoke(e);
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Exception when executing a {nameof(IBackgroundTask)}. ");
            _onException?.Invoke(e);
            throw;
        }
        finally
        {
            _signal.Release();
        }
    }

    /// <summary>
    /// Gets the number of items in the <see cref="BackgroundQueue"/>.
    /// </summary>
    public int Count => TaskItems.Count;

    /// <summary>
    /// Starts the task from the parameter and cancels it after the given <see cref="TimeSpan"/> if not completed earlier.
    /// </summary>
    /// <param name="startTask">The task to start</param>
    /// <param name="timeout">The timeout. Set it to TimeSpan.FromMilliseconds(-1) indicating an infinite timeout.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/></param>
    /// <returns>Returns the completed original task or throws a <see cref="TimeoutException"/></returns>
    /// <exception cref="TimeoutException"></exception>
    private static async Task CancelAfterAsync(Func<CancellationToken, Task> startTask, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var timeoutCancellation = new CancellationTokenSource();
        using var combinedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancellation.Token);
        var originalTask = startTask(combinedCancellation.Token);
        var delayTask = Task.Delay(timeout, timeoutCancellation.Token);
        var completedTask = await Task.WhenAny(originalTask, delayTask);
        // Cancel timeout to stop either task:
        // - Either the original task completed, so we need to cancel the delay task.
        // - Or the timeout expired, so we need to cancel the original task.
        // Canceling will not affect a task, that is already completed.
        timeoutCancellation.Cancel();
        if (completedTask == originalTask)
        {
            // original task completed
            await originalTask;
        }
        else
        {
            // timeout
            throw new TimeoutException();
        }
    }
}
