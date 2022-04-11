using System;
using System.Threading;
using System.Threading.Tasks;

namespace Axuno.BackgroundTask
{
    /// <summary>
    /// An <see cref="IBackgroundTask"/> that does nothing.
    /// </summary>
    internal class BackgroundTaskEmpty : IBackgroundTask
    {
        /// <inheritdoc/>
        public Task RunAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 30);
    }
}
