using System.Runtime;
using Axuno.UnitTest.TestComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class BackgroundQueueServiceTests
{
    private ServiceProvider? _serviceProvider;

    [SetUp]
    public void Setup()
    {
        _serviceProvider = CreateServiceProvider();
        ExceptionFromBackgroundQueue = null;
    }

    [TearDown]
    public void DisposeObjects()
    {
        _serviceProvider?.Dispose();
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<BackgroundQueue>>(new NUnitLogger<BackgroundQueue> { LogLevel = LogLevel.Trace });
        services.AddSingleton<ILogger<BackgroundQueueService>>(new NUnitLogger<BackgroundQueueService> { LogLevel = LogLevel.Trace });
        services.Configure<BackgroundQueueConfig>(config => config.OnException = e => ExceptionFromBackgroundQueue = e);
        services.AddSingleton<IBackgroundQueue, BackgroundQueue>();
        // Add test IHostApplicationLifetime implementation (align with Concurrent* tests)
        services.AddSingleton<IHostApplicationLifetime, TestHostApplicationLifetime>();

        // Configure the correct service config (was Concurrent* before)
        services.Configure<BackgroundQueueServiceConfig>(options =>
        {
            options.PollQueueDelay = TimeSpan.FromMilliseconds(5);
        });

        services.AddBackgroundQueueService();

        return services.BuildServiceProvider();
    }

    private Exception? ExceptionFromBackgroundQueue { get; set; }

    [Test]
    public async Task Tasks_Completing_Normally()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var hosted = _serviceProvider!.GetRequiredService<IHostedService>();

        for (var i = 0; i < 8; i++)
        {
            var cnt = i + 1;
            queue.QueueTask(new BgTsk(_ =>
            {
                Interlocked.Increment(ref itemCounter);
                Console.WriteLine($"Task {cnt} completed.");
                return Task.CompletedTask;
            }));
        }

        var expected = queue.Count;
        await hosted.StartAsync(cts.Token);

        while (itemCounter != expected)
        {
            await Task.Delay(25, CancellationToken.None);
        }

        await hosted.StopAsync(cts.Token);

        Assert.That(itemCounter, Is.EqualTo(expected));
    }

    [Test]
    public async Task Tasks_Throwing_Exception()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var hosted = _serviceProvider!.GetRequiredService<IHostedService>();

        queue.QueueTask(new BgTsk(_ => { Interlocked.Increment(ref itemCounter); Console.WriteLine("Task 1 completed."); return Task.CompletedTask; }));
        queue.QueueTask(new BgTsk(_ => { Interlocked.Increment(ref itemCounter); Console.WriteLine("Task 2 completed."); return Task.CompletedTask; }));
        queue.QueueTask(new BgTsk(_ =>
        {
            Interlocked.Increment(ref itemCounter);
            throw new AmbiguousImplementationException("TaskItem 3 exception");
        }));

        var expected = queue.Count;
        await hosted.StartAsync(cts.Token);

        while (itemCounter != expected)
        {
            await Task.Delay(25, CancellationToken.None);
        }

        await hosted.StopAsync(cts.Token);

        Assert.Multiple(() =>
        {
            Assert.That(itemCounter, Is.EqualTo(expected));
            Assert.That(ExceptionFromBackgroundQueue?.GetType(), Is.EqualTo(typeof(AmbiguousImplementationException)));
        });
    }

    [Test]
    public async Task Tasks_Throwing_OperationCanceledException_And_Canceling()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var hosted = _serviceProvider!.GetRequiredService<IHostedService>();

        await hosted.StartAsync(cts.Token);

        queue.QueueTask(new BgTsk(_ =>
        {
            Interlocked.Increment(ref itemCounter);
            throw new OperationCanceledException();
        }));

        while (itemCounter < 1)
        {
            await Task.Delay(10, CancellationToken.None);
        }

        // Service will treat this as cancellation path; ensure we can still call StopAsync safely.
        await hosted.StopAsync(CancellationToken.None);

        Assert.That(itemCounter, Is.EqualTo(1));
    }

    [Test]
    public async Task Canceled_Queue_Task_Should_Not_Cancel_Service()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var hosted = _serviceProvider!.GetRequiredService<IHostedService>();

        await hosted.StartAsync(cts.Token);

        // First task times out quickly (Timeout handled within queue implementation)
        queue.QueueTask(new BgTsk(async token =>
            {
                Interlocked.Increment(ref itemCounter);
                await Task.Delay(1000, token);
            })
            { Timeout = TimeSpan.FromMilliseconds(5) });

        // Second task should still execute
        queue.QueueTask(new BgTsk(_ =>
        {
            Interlocked.Increment(ref itemCounter);
            return Task.CompletedTask;
        }));

        // Allow processing
        await Task.Delay(300, CancellationToken.None);
        await hosted.StopAsync(CancellationToken.None);

        Assert.That(itemCounter, Is.EqualTo(2));
    }

    [Test]
    public async Task Stop_Service()
    {
        var hosted = _serviceProvider!.GetRequiredService<IHostedService>();

        await hosted.StartAsync(CancellationToken.None);
        await Task.Delay(50, CancellationToken.None);

        Assert.DoesNotThrowAsync(() => hosted.StopAsync(CancellationToken.None));
    }
}
