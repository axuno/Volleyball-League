using NUnit.Framework;

namespace TournamentManager.Tests;

[TestFixture]
public class DateTimePeriodTest
{
    [TestCase("2020-06-01", "2060-06-02")]
    [TestCase(null, null)]
    [TestCase("2020-06-01", null)]
    [TestCase(null, "2060-06-02")]
    public void CreationAndAssigningNullable(DateTime? date1, DateTime? date2)
    {
        var dtp = new DateTimePeriod(date1, date2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.Start, Is.EqualTo(date1));
            Assert.That(dtp.End, Is.EqualTo(date2));
        }
    }

    [Test]
    public void Create_With_Date_Kind_Specified()
    {
        var date1 = new DateTime(2024, 06, 01, 18, 18, 18, DateTimeKind.Utc);
        var date2 = new DateTime(2024, 06, 02, 18, 18, 18, DateTimeKind.Local);
        var dtp = new DateTimePeriod(date1, date2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.Start, Is.EqualTo(date1));
            Assert.That(dtp.End, Is.EqualTo(date2));
            Assert.That(dtp.Start?.Kind, Is.EqualTo(DateTimeKind.Utc));
            Assert.That(dtp.End?.Kind, Is.EqualTo(DateTimeKind.Local));
        }
    }

    [TestCase("2020-06-01", "2060-06-02")]
    public void CreationAndAssigning(DateTime date1, DateTime date2)
    {
        var dtp = new DateTimePeriod(date1, date2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.Start, Is.EqualTo(date1));
            Assert.That(dtp.End, Is.EqualTo(date2));
        }
    }

    [Test]
    public void CreationAndAssigningDefaultCtor()
    {
        var dtp = new DateTimePeriod();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.End, Is.EqualTo(dtp.Start));
            Assert.That(dtp.Start, Is.Null);
        }
    }

    [Test]
    public void CreationAndAssigningWithSwap()
    {
        // swap start and end so that always start <= end
        var date1 = new DateTime(2020, 06, 02);
        var date2 = new DateTime(2020, 06, 01);
        var dtp = new DateTimePeriod(date1, date2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.End, Is.EqualTo(date1));
            Assert.That(dtp.Start, Is.EqualTo(date2));
        }
    }

    [TestCase("2020-06-01", "1:00:00:00")]
    [TestCase(null, "1:00:00:00")]
    [TestCase("2020-06-01", null)]
    public void CreationAndAssigningWithTimeSpanNullable(DateTime? date1, TimeSpan? timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.Start, Is.EqualTo(date1));
            Assert.That(dtp.End, Is.EqualTo(timeSpan.HasValue ? date1?.Add(timeSpan.Value) : null));
        }
    }

    [TestCase("2020-06-01", "1:00:00:00")]
    public void CreationAndAssigningWithTimeSpan(DateTime date1, TimeSpan timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.Start, Is.EqualTo(date1));
            Assert.That(dtp.End, Is.EqualTo(date1.Add(timeSpan)));
        }
    }

    [TestCase("2020-06-01", "-1:00:00:00")]
    public void CreationAndAssigningWithTimeSpanAndSwap(DateTime? date1, TimeSpan? timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.End, Is.EqualTo(date1));
            Assert.That(dtp.Start, Is.EqualTo(timeSpan.HasValue ? date1?.Add(timeSpan.Value) : null));
        }
    }

    [Test]
    public void MaxDateTimePrecisionIsOneSecond()
    {
        var date1 = new DateTime(2020, 06, 01, 18, 18, 18).AddTicks(20);
        var date2 = new DateTime(2020, 06, 01, 18, 18, 18).AddTicks(30);
        var dtp = new DateTimePeriod(date1, date2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(dtp.End, Is.EqualTo(dtp.Start));
            Assert.That(dtp.Start, Is.Not.EqualTo(date1));
        }
    }

    [TestCase("2020-06-01", "2020-06-03", "2020-06-01", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-03", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-02", true)]
    [TestCase("2020-06-20", "2020-06-23", "2020-12-31", false)]
    [TestCase(null, null, null, false)]
    public void ContainsNullable(DateTime? date1, DateTime? date2, DateTime? contains, bool expected)
    {
        var dtp = new DateTimePeriod(date1, date2);
        Assert.That(dtp.Contains(contains), Is.EqualTo(expected));
    }

    [TestCase("2020-06-01", "2020-06-03", "2020-06-01", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-03", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-02", true)]
    [TestCase("2020-06-20", "2020-06-23", "2020-12-31", false)]
    [TestCase(null, null, "2020-12-31", false)]
    public void Contains(DateTime? date1, DateTime? date2, DateTime contains, bool expected)
    {
        var dtp = new DateTimePeriod(date1, date2);
        Assert.That(dtp.Contains(contains), Is.EqualTo(expected));
    }

    [TestCase(null, null, false)]
    [TestCase("2020-06-04", null, false)]
    [TestCase(null, "2020-07-30", false)]
    [TestCase("2020-06-05", "2020-07-30", false)]
    [TestCase("2020-05-01", "2020-05-30", false)]
    [TestCase("2020-05-01", "2020-06-01", true)]
    [TestCase("2020-05-01", "2020-07-30", true)]
    [TestCase("2020-06-02", "2020-06-03", true)]
    public void Overlaps(DateTime? dt1, DateTime? dt2, bool expected)
    {
        var dtp1 = new DateTimePeriod(new(2020, 06, 01), new DateTime(2020, 06, 04));
        var dtp2 = new DateTimePeriod(dt1, dt2);
        Assert.That(dtp1.Overlaps(dtp2), Is.EqualTo(expected));
    }

    [TestCase(null, null, null)]
    [TestCase("2020-06-04", null, null)]
    [TestCase(null, "2020-07-30", null)]
    [TestCase("2020-06-05", "2020-07-30", "55:00:00:00")]
    [TestCase("2020-06-05 00:00:00", "2020-06-05 23:59:59", "00:23:59:59")]
    public void DurationNullable(DateTime? dt1, DateTime? dt2, TimeSpan? expected)
    {
        var dtp = new DateTimePeriod(dt1, dt2);
        Assert.That(dtp.Duration(true), Is.EqualTo(expected));
    }

    [TestCase("2020-06-05", "2020-07-30", "55:00:00:00")]
    [TestCase("2020-06-05 00:00:00", "2020-06-05 23:59:59", "00:23:59:59")]
    public void Duration(DateTime dt1, DateTime dt2, TimeSpan expected)
    {
        var dtp = new DateTimePeriod(dt1, dt2);
        Assert.That(dtp.Duration(), Is.EqualTo(expected));
    }
}
