using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class ConcurrentBackgroundQueueServiceConfigTests
{
    [Test]
    public void Set_Config_Default()
    {
        var config = new ConcurrentBackgroundQueueServiceConfig { PollQueueDelay = TimeSpan.Zero, MaxConcurrentCount = -1 };
        using (Assert.EnterMultipleScope())
        {
            Assert.That(config.PollQueueDelay, Is.EqualTo(TimeSpan.FromMilliseconds(100)));
            Assert.That(config.MaxConcurrentCount, Is.EqualTo(50));
        }
    }
}
