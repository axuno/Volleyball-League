using NUnit.Framework;

namespace Axuno.BackgroundTask.Tests;

[TestFixture]
public class BackgroundQueueServiceConfigTests
{
    [Test]
    public void Set_Config_Default()
    {
        var config = new BackgroundQueueServiceConfig{ PollQueueDelay = default};
        Assert.That(config.PollQueueDelay, Is.EqualTo(TimeSpan.FromMilliseconds(100)));
    }
}
