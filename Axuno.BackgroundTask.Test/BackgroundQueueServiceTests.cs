using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Axuno.UnitTest.TestComponents;
using Microsoft.Extensions.Options;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class BackgroundQueueServiceTests
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
        services.AddSingleton<ILogger<BackgroundQueue>>(new NUnitLogger<BackgroundQueue> { LogLevel = LogLevel.Trace });
        services.AddSingleton<ILogger<BackgroundQueueService>>(new NUnitLogger<BackgroundQueueService> { LogLevel = LogLevel.Trace });
        services.Configure<BackgroundQueueConfig>(config => config.OnException = e => ExceptionFromBackgroundQueue = e);
        services.AddSingleton<IBackgroundQueue, BackgroundQueue>();

        /*  Instead of AddHostedService<BackgroundQueueService>() we could also use:
         *  services.AddSingleton<BackgroundQueueService>();
         *  and in the tests:
         *  serviceProvider.GetRequiredService<BackgroundQueueService>(); */
        services.Configure<ConcurrentBackgroundQueueServiceConfig>(options => options.PollQueueDelay = TimeSpan.FromMilliseconds(5));
        services.AddBackgroundQueueService();

        return services.BuildServiceProvider();
    }

    private Exception ExceptionFromBackgroundQueue { get; set; }

    [Test]
    public async Task Tasks_Completing_Normally()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
        for (var i = 0; i < 8; i++)
        {
            var cnt = i + 1;
            queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); Console.WriteLine($"Task {cnt} completed."); return Task.CompletedTask; }));
        }
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
        });
    }

    [Test]
    public async Task Tasks_Throwing_Exception()
    {
        var itemCounter = 0;
        var cts = new CancellationTokenSource();

        var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); Console.WriteLine($"Task 1 completed."); return Task.CompletedTask; }));
        queue.QueueTask(new BgTsk(cancellationToken => { Interlocked.Increment(ref itemCounter); Console.WriteLine($"Task 2 completed."); return Task.CompletedTask; }));
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
    public async Task Stop_Service()
    {
        var bgTaskSvc = _serviceProvider.GetRequiredService<IHostedService>();

        await bgTaskSvc.StartAsync(CancellationToken.None);
            
        await Task.Delay(100, CancellationToken.None);
        var task = bgTaskSvc.StopAsync(CancellationToken.None);
            
        Assert.Multiple(() =>
        {
            Assert.IsFalse(task.IsFaulted);
        });
    }
}