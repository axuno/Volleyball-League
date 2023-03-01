using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Axuno.BackgroundTask.Tests;

public class BgTsk : IBackgroundTask
{
    public BgTsk(Func<CancellationToken, Task> work)
    {
        Work = work;
    }

    public TimeSpan Timeout { get; set; } = TimeSpan.FromMilliseconds(-1); // infinite

    public Task RunAsync(CancellationToken cancellation)
    {
        return Work(cancellation);
    }

    public Func<CancellationToken, Task> Work { get; set; }
}