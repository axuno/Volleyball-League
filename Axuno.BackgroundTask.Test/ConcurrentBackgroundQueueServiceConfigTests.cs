using System;
using System.Collections.Generic;
using System.Text;
using Axuno.BackgroundTask;
using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class ConcurrentBackgroundQueueServiceConfigTests
{
    [Test]
    public void Set_Config_Default()
    {
        var config = new ConcurrentBackgroundQueueServiceConfig{ PollQueueDelay = default, MaxConcurrentCount = -1};
        Assert.Multiple(() =>
        {
            Assert.AreEqual(TimeSpan.FromMilliseconds(100), config.PollQueueDelay);
            Assert.AreEqual(5, config.MaxConcurrentCount);
        });
    }
}