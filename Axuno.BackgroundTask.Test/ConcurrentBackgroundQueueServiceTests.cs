using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Axuno.UnitTest.TestComponents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class ConcurrentBackgroundQueueServiceTests
{
    private ServiceProvider _serviceProvider;

    [SetUp]
    public void Setup()
    {
        _serviceProvider = CreateServiceProvider();
        ExceptionFromBackgroundQueue = null;
    }

    private ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddSingleton<ILogger<BackgroundQueue>>(new NUnitLogger<BackgroundQueue> {LogLevel = LogLevel.Trace});
        services.AddSingleton<ILogger<ConcurrentBackgroundQueueService>>(new NUnitLogger<ConcurrentBackgroundQueueService> { LogLevel = LogLevel.Trace });
        services.Configure<BackgroundQueueConfig>(config => config.OnException = e => ExceptionFromBackgroundQueue = e);
        services.AddSingleton<IBackgroundQueue, BackgroundQueue>();

        /*  Instead of AddHostedService<ConcurrentBackgroundQueueService>() we could also use:
         *  services.AddSingleton<ConcurrentBackgroundQueueService>();
         *  and in the tests:
         *  serviceProvider.GetRequiredService<ConcurrentBackgroundQueueService>(); */
        services.Configure<ConcurrentBackgroundQueueServiceConfig>(options => {});
        services.AddConcurrentBackgroundQueueService();

        return services.BuildServiceProvider();
    }

    private Exception ExceptionFromBackgroundQueue { get; set; }

    [TestCase(false)]
    [TestCase(true)]
    public async Task Tasks_Completing_Normally(bool abortBeforeCompletion)
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
        // will create enough tasks to exceed the maximum number of parallel tasks
        for (var i = 1; i < 10; i++)
        {
            var cnt = i;
            queue.QueueTask(new BgTsk(async cancellationToken => { await Task.Delay(100 * (cnt % 2 + 1), cancellationToken); Interlocked.Increment(ref itemCounter); Console.WriteLine($"Task {cnt} completed."); }));
        }
        var expected = queue.Count; 
        var task = bgTaskSvc.StartAsync(cts.Token);
        while (itemCounter < (abortBeforeCompletion ? 4 : expected))
        {
            await Task.Delay(10, CancellationToken.None);
        }
        await bgTaskSvc.StopAsync(cts.Token);

        Assert.AreEqual(abortBeforeCompletion, expected != itemCounter);
    }

    [Test]
    public async Task Tasks_Throwing_Exception()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
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

        Assert.Multiple(() =>
        {
            Assert.AreEqual(expected, itemCounter);
            Assert.AreEqual(ExceptionFromBackgroundQueue.GetType(), typeof(AmbiguousImplementationException));
        });
    }
        
    [Test]
    public async Task Tasks_Throwing_OperationCanceledException_And_Canceling()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
        var task = bgTaskSvc.StartAsync(cts.Token);
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); throw new OperationCanceledException(); }));
        while (itemCounter < 1)
        {
            await Task.Delay(20, CancellationToken.None);
        }
        if (itemCounter > 0) cts.Cancel();

        Assert.AreEqual(1, itemCounter);
    }

    [Test]
    public async Task Canceled_Queue_Task_Should_Not_Cancel_Service()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
        var task = bgTaskSvc.StartAsync(cts.Token);
            
        queue.QueueTask(new BgTsk(async cancellationToken => { Interlocked.Increment(ref itemCounter); await Task.Delay(1000, cancellationToken); }){Timeout = TimeSpan.FromMilliseconds(1) });
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); return Task.CompletedTask; }));
        await Task.Delay(500, cts.Token);
        await bgTaskSvc.StopAsync(cts.Token);

        Assert.AreEqual(2, itemCounter);
    }

    [Test]
    public async Task Failing_Service_Should_Set_Queue_To_Null()
    {
        var cts = new CancellationTokenSource();

        var bgTaskSvc = (ConcurrentBackgroundQueueService) _serviceProvider.GetRequiredService<IHostedService>();
        await bgTaskSvc.StartAsync(cts.Token);

        // Set the backing field for the Config property by reflection
        // in order to make the service throw
        bgTaskSvc.GetType().GetField($"<{nameof(ConcurrentBackgroundQueueService.Config)}>" + "k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(bgTaskSvc, null);
        Assert.IsNull(bgTaskSvc.Config);
    }

    [Test]
    public async Task Stop_Service()
    {
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();

        await bgTaskSvc.StartAsync(CancellationToken.None);
        await Task.Delay(100, CancellationToken.None);
            
        Assert.DoesNotThrowAsync(() => bgTaskSvc.StopAsync(CancellationToken.None));
    }
}