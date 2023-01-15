using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axuno.BackgroundTask;

/// <summary>
/// Interface for background tasks.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// The task will be started asynchronously. The caller is responsible for implementing,
    /// that the <see cref="Timeout"/> set for the task is respected.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task RunAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets or sets the timeout, after which e.g. a <see cref="TimeoutException"/> should be thrown by the calling <see cref="IBackgroundQueue"/>
    /// (or any other appropriate action).
    /// Set the timeout to <see cref="TimeSpan.FromMilliseconds"/> with value -1 indicating an infinite timeout.
    /// </summary>
    TimeSpan Timeout { get; set; }
}