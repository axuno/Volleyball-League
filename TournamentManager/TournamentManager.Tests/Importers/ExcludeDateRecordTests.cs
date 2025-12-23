using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers;

[TestFixture]
internal class ExcludeDateRecordTests
{
    [Test]
    public void Create_Should_Succeed()
    {
        var dateTime = DateTime.UtcNow;
        // DateTimePeriod truncates milliseconds
        var expected = new DateTime(
            dateTime.Ticks - (dateTime.Ticks % TimeSpan.TicksPerSecond),
            dateTime.Kind
        );

        var record = new ExcludeDateRecord {
            Period = new(dateTime, dateTime.AddDays(1)),
            Reason = "Any reason"
        };

        using (Assert.EnterMultipleScope())
        {
            Assert.That(record.Period.Start, Is.EqualTo(expected));
            Assert.That(record.Period.End, Is.EqualTo(expected.AddDays(1)));
            Assert.That(record.Reason, Is.EqualTo("Any reason"));
            Assert.That(record.ToString(), Is.EqualTo($"{expected:yyyy-MM-dd HH:mm:ss} - {expected.AddDays(1):yyyy-MM-dd HH:mm:ss}: Any reason"));
        }
    }

    [Test]
    public void Conversion_To_ExcludeMatchDate_Should_Succeed()
    {
        var dateTime = new DateTime(2024, 1, 31, 12, 58, 59);

        var record = new ExcludeDateRecord
        {
            Period = new(dateTime, dateTime.AddDays(1)),
            Reason = "Any reason"
        };

        var excludeMatchDate = record.ToExcludeMatchDateEntity();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(excludeMatchDate.DateFrom, Is.EqualTo(dateTime));
            Assert.That(excludeMatchDate.DateTo, Is.EqualTo(dateTime.AddDays(1)));
            Assert.That(excludeMatchDate.Reason, Is.EqualTo("Any reason"));
        }
    }

    [Test]
    public void Conversion_With_Invalid_Data_Should_Fail()
    {
        var dateTime = DateTime.MinValue;

        var record = new ExcludeDateRecord
        {
            Period = new(dateTime, dateTime),
            Reason = ""
        };

        Assert.That(() => record.ToExcludeMatchDateEntity(), Throws.TypeOf<ArgumentException>());
    }
}


