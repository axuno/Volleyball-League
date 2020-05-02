using System;
using System.Collections.Generic;
using System.Text;

namespace Axuno.BackgroundTask
{
    /// <summary>
    /// Contains the configuration for a <see cref="ConcurrentBackgroundQueueService"/>.
    /// </summary>
    public class ConcurrentBackgroundQueueServiceConfig
    {
        private TimeSpan _pollQueueDelay = TimeSpan.FromMilliseconds(100);
        private int _maxConcurrentCount = 5;

        /// <summary>
        /// Gets or sets the maximum number of tasks executed concurrently.
        /// Defaults to 5.
        /// </summary>
        public int MaxConcurrentCount
        {
            get => _maxConcurrentCount;
            set => _maxConcurrentCount = value > 0 ? value : 5;
        }
        /// <summary>
        /// The time to wait until the <see cref="IBackgroundQueue"/> is checked for new entries.
        /// Defaults to 100 milliseconds.
        /// </summary>
        public TimeSpan PollQueueDelay
        {
            get => _pollQueueDelay;
            set => _pollQueueDelay = value == default ? TimeSpan.FromMilliseconds(100) : value;
        }
    }
}
