namespace Axuno.BackgroundTask;

public interface IBackgroundQueue
{
    void QueueTask(IBackgroundTask taskItem);

    IBackgroundTask DequeueTask();

    Task RunTaskAsync(IBackgroundTask task, CancellationToken cancellationToken);

    int Count { get; }
}