using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Axuno.UnitTest.TestComponents;

namespace Axuno.BackgroundTask.Tests
{
    [TestFixture]
    public class BackgroundQueueTests
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
            services.Configure<BackgroundQueueConfig>(config => config.OnException = e => ExceptionFromBackgroundQueue = e);
            services.AddSingleton<IBackgroundQueue, BackgroundQueue>();
            return services.BuildServiceProvider();
        }

        private Exception ExceptionFromBackgroundQueue { get; set; }

        [Test]
        public void Tasks_Item_Is_Null_Should_Throw()
        {
            var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();

            Assert.Throws<ArgumentNullException>(() => queue.QueueTask(null));
        }

        [Test]
        public void Tasks_Timeout_Should_Throw()
        {
            // onException just sets the exception variable as an evidence for being called
            var queue = _serviceProvider.GetRequiredService<IBackgroundQueue>();
            var task = new BgTsk(async cancellationToken => { await Task.Delay(5000, cancellationToken); })
            {
                Timeout = TimeSpan.FromMilliseconds(100)
            };
            queue.QueueTask(task);
            
            Assert.Multiple(() =>
            {
                Assert.ThrowsAsync<TimeoutException>(async () => await queue.RunTaskAsync(queue.DequeueTask(), CancellationToken.None));
                Assert.IsTrue(ExceptionFromBackgroundQueue is TimeoutException);
            });

            
        }
    }
}
