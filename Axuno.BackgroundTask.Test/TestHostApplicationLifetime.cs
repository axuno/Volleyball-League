using Microsoft.Extensions.Hosting;

namespace Axuno.BackgroundTask.Tests;

internal sealed class TestHostApplicationLifetime : IHostApplicationLifetime
{
    private readonly CancellationTokenSource _started = new();
    private readonly CancellationTokenSource _stopping = new();
    private readonly CancellationTokenSource _stopped = new();

    public CancellationToken ApplicationStarted => _started.Token;
    public CancellationToken ApplicationStopping => _stopping.Token;
    public CancellationToken ApplicationStopped => _stopped.Token;

    public void StopApplication()
    {
        if (!_stopping.IsCancellationRequested)
        {
            _stopping.Cancel();
            _stopped.Cancel();
        }
    }

    public void NotifyStarted() => _started.Cancel();
}
