namespace Axuno.BackgroundTask;

public interface IBackgroundQueue
{
    void QueueTask(IBackgroundTask taskItem);

    Task<IBackgroundTask> DequeueTaskAsync(CancellationToken token);

    Task RunTaskAsync(IBackgroundTask task, CancellationToken cancellationToken);

    int Count { get; }
}
