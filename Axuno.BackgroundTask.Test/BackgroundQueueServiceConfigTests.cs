using System;
using System.Collections.Generic;
using System.Text;
using Axuno.BackgroundTask;
using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class BackgroundQueueServiceConfigTests
{
    [Test]
    public void Set_Config_Default()
    {
        var config = new BackgroundQueueServiceConfig{ PollQueueDelay = default};
        Assert.AreEqual(TimeSpan.FromMilliseconds(100), config.PollQueueDelay);
    }
}