using NUnit.Framework;
using TournamentManager.ExtensionMethods;

namespace TournamentManager.Tests.ExtensionMethods;

[TestFixture]
public class TimeSpanExtensionTests
{
    [Test]
    public void To_Short_Time_String()
    {
        var ts = new TimeSpan(14, 13, 12);
        Assert.That(ts.ToShortTimeString(), Is.EqualTo(new DateTime(2020, 01,01, 14, 13, 12).ToShortTimeString()));
    }

    [Test]
    public void To_Long_Time_String()
    {
        var ts = new TimeSpan(14, 13, 12);
        Assert.That(ts.ToLongTimeString(), Is.EqualTo(new DateTime(2020, 01, 01, 14, 13, 12).ToLongTimeString()));
    }
}