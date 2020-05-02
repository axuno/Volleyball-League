using System;
using System.Collections.Generic;
using System.Text;

namespace Axuno.BackgroundTask
{
    /// <summary>
    /// Contains the configuration for a <see cref="BackgroundQueue"/>.
    /// </summary>
    public class BackgroundQueueConfig
    {
        /// <summary>
        /// Gets or sets the <see cref="Action"/> when an <see cref="Exception"/> is thrown. May be null.
        /// </summary>
        public Action<Exception> OnException { get; set; }
    }
}
