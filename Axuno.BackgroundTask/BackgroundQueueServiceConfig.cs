using System;
using System.Collections.Generic;
using System.Text;

namespace Axuno.BackgroundTask
{
    /// <summary>
    /// Contains the configuration for a <see cref="BackgroundQueueService"/>.
    /// </summary>
    public class BackgroundQueueServiceConfig
    {
        private TimeSpan _pollQueueDelay = TimeSpan.FromMilliseconds(100);

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
