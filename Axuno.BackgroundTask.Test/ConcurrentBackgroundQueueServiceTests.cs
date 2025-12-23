using System.Reflection;
using System.Runtime;
using Axuno.UnitTest.TestComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class ConcurrentBackgroundQueueServiceTests
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
        services.AddSingleton<ILogger<ConcurrentBackgroundQueueService>>(new NUnitLogger<ConcurrentBackgroundQueueService> { LogLevel = LogLevel.Trace });
        services.Configure<BackgroundQueueConfig>(config => config.OnException = e => ExceptionFromBackgroundQueue = e);
        services.AddSingleton<IBackgroundQueue, BackgroundQueue>();
        // Add test IHostApplicationLifetime implementation
        services.AddSingleton<IHostApplicationLifetime, TestHostApplicationLifetime>();

        /*  Instead of AddHostedService<ConcurrentBackgroundQueueService>() we could also use:
         *  services.AddSingleton<ConcurrentBackgroundQueueService>();
         *  and in the tests:
         *  serviceProvider.GetRequiredService<ConcurrentBackgroundQueueService>(); */
        services.Configure<ConcurrentBackgroundQueueServiceConfig>(options => { });
        services.AddConcurrentBackgroundQueueService();

        return services.BuildServiceProvider();
    }

    private Exception? ExceptionFromBackgroundQueue { get; set; }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Tasks_Completing_Normally(bool abortBeforeCompletion)
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider!.GetRequiredService<IHostedService>();
        for (var i = 1; i < 10; i++)
        {
            var cnt = i;
            queue.QueueTask(new BgTsk(async cancellationToken =>
            {
                await Task.Delay(100 * (cnt % 2 + 1), cancellationToken);
                Interlocked.Increment(ref itemCounter);
                Console.WriteLine($"Task {cnt} completed.");
            }));
        }
        var expected = queue.Count;
        var task = bgTaskSvc.StartAsync(cts.Token);
        while (itemCounter < (abortBeforeCompletion ? 4 : expected))
        {
            await Task.Delay(10, CancellationToken.None);
        }
        await bgTaskSvc.StopAsync(cts.Token);

        Assert.That(expected != itemCounter, Is.EqualTo(abortBeforeCompletion));
    }

    [Test]
    public async Task Tasks_Throwing_Exception()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider!.GetRequiredService<IHostedService>();
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); Console.WriteLine("Task 1 completed."); return Task.CompletedTask; }));
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); Console.WriteLine("Task 2 completed."); return Task.CompletedTask; }));
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); throw new AmbiguousImplementationException("TaskItem 3 exception"); }));
        var expected = queue.Count;
        var task = bgTaskSvc.StartAsync(cts.Token);
        while (itemCounter != expected)
        {
            await Task.Delay(50, CancellationToken.None);
        }
        await bgTaskSvc.StopAsync(cts.Token);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(itemCounter, Is.EqualTo(expected));
            Assert.That(ExceptionFromBackgroundQueue?.GetType(), Is.EqualTo(typeof(AmbiguousImplementationException)));
        }
    }

    [Test]
    public async Task Tasks_Throwing_OperationCanceledException_And_Canceling()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider!.GetRequiredService<IHostedService>();
        var task = bgTaskSvc.StartAsync(cts.Token);
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); throw new OperationCanceledException(); }));
        while (itemCounter < 1)
        {
            await Task.Delay(20, CancellationToken.None);
        }
        if (itemCounter > 0) cts.Cancel();

        Assert.That(itemCounter, Is.EqualTo(1));
    }

    [Test]
    public async Task Canceled_Queue_Task_Should_Not_Cancel_Service()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider!.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider!.GetRequiredService<IHostedService>();
        var task = bgTaskSvc.StartAsync(cts.Token);

        queue.QueueTask(new BgTsk(async cancellationToken =>
            {
                Interlocked.Increment(ref itemCounter);
                await Task.Delay(1000, cancellationToken);
            })
            { Timeout = TimeSpan.FromMilliseconds(1) });
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); return Task.CompletedTask; }));
        await Task.Delay(500, cts.Token);
        await bgTaskSvc.StopAsync(cts.Token);

        Assert.That(itemCounter, Is.EqualTo(2));
    }

    [Test]
    public async Task Failing_Service_Should_Set_Queue_To_Null()
    {
        var cts = new CancellationTokenSource();

        var bgTaskSvc = (ConcurrentBackgroundQueueService)_serviceProvider!.GetRequiredService<IHostedService>();
        await bgTaskSvc.StartAsync(cts.Token);

        bgTaskSvc.GetType()
            .GetField($"<{nameof(ConcurrentBackgroundQueueService.Config)}>" + "k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(bgTaskSvc, null);
        Assert.That(bgTaskSvc.Config, Is.Null);
    }

    [Test]
    public async Task Stop_Service()
    {
        var bgTaskSvc = _serviceProvider!.GetRequiredService<IHostedService>();

        await bgTaskSvc.StartAsync(CancellationToken.None);
        await Task.Delay(100, CancellationToken.None);

        Assert.DoesNotThrowAsync(() => bgTaskSvc.StopAsync(CancellationToken.None));
    }

    [Test]
    public async Task ApplicationLifetime_Flags_Set_On_Stop()
    {
        var hosted = (ConcurrentBackgroundQueueService) _serviceProvider!.GetRequiredService<IHostedService>();
        var lifetime = (TestHostApplicationLifetime) _serviceProvider!.GetRequiredService<IHostApplicationLifetime>();

        // Start service
        await hosted.StartAsync(CancellationToken.None);

        // Reflect private fields
        var stoppingField = typeof(ConcurrentBackgroundQueueService).GetField("_applicationStopping",
            BindingFlags.NonPublic | BindingFlags.Instance);
        var stoppedField = typeof(ConcurrentBackgroundQueueService).GetField("_applicationStopped",
            BindingFlags.NonPublic | BindingFlags.Instance);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(stoppingField, Is.Not.Null);
            Assert.That(stoppedField, Is.Not.Null);

            // Initially false
            Assert.That((bool) stoppingField!.GetValue(hosted)!, Is.False);
            Assert.That((bool) stoppedField!.GetValue(hosted)!, Is.False);
        }

        // Trigger lifetime stop (TestHostApplicationLifetime sets both tokens)
        lifetime.StopApplication();

        // Wait until flags set or timeout
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (sw.Elapsed < TimeSpan.FromSeconds(1))
        {
            if ((bool) stoppingField.GetValue(hosted)! && (bool) stoppedField.GetValue(hosted)!)
                break;
            await Task.Delay(10);
        }

        using (Assert.EnterMultipleScope())
        {
            // Assert flags set
            Assert.That((bool) stoppingField.GetValue(hosted)!, Is.True,
                "_applicationStopping should be true after StopApplication()");
            Assert.That((bool) stoppedField.GetValue(hosted)!, Is.True,
                "_applicationStopped should be true after StopApplication()");
        }

        // Now stop service normally
        await hosted.StopAsync(CancellationToken.None);
    }
}
