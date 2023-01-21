using System;
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
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.Start);
            Assert.AreEqual(date2, dtp.End);
        });
    }

    [TestCase("2020-06-01", "2060-06-02")]
    public void CreationAndAssigning(DateTime date1, DateTime date2)
    {
        var dtp = new DateTimePeriod(date1, date2);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.Start);
            Assert.AreEqual(date2, dtp.End);
        });
    }

    [Test]
    public void CreationAndAssigningDefaultCtor()
    {
        var dtp = new DateTimePeriod();
        Assert.Multiple(() =>
        {
            Assert.AreEqual(dtp.Start, dtp.End);
            Assert.IsNull(dtp.Start);
        });
    }

    [Test]
    public void CreationAndAssigningWithSwap()
    {
        // swap start and end so that always start <= end
        var date1 = new DateTime(2020, 06, 02);
        var date2 = new DateTime(2020, 06, 01);
        var dtp = new DateTimePeriod(date1, date2);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.End);
            Assert.AreEqual(date2, dtp.Start);
        });
    }

    [TestCase("2020-06-01", "1:00:00:00")]
    [TestCase(null, "1:00:00:00")]
    [TestCase("2020-06-01", null)]
    public void CreationAndAssigningWithTimeSpanNullable(DateTime? date1, TimeSpan? timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.Start);
            Assert.AreEqual(timeSpan.HasValue ? date1?.Add(timeSpan.Value) : null, dtp.End);
        });
    }

    [TestCase("2020-06-01", "1:00:00:00")]
    public void CreationAndAssigningWithTimeSpan(DateTime date1, TimeSpan timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.Start);
            Assert.AreEqual(date1.Add(timeSpan), dtp.End);
        });
    }

    [TestCase("2020-06-01", "-1:00:00:00")]
    public void CreationAndAssigningWithTimeSpanAndSwap(DateTime? date1, TimeSpan? timeSpan)
    {
        var dtp = new DateTimePeriod(date1, timeSpan);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(date1, dtp.End);
            Assert.AreEqual(timeSpan.HasValue ? date1?.Add(timeSpan.Value) : null, dtp.Start);
        });
    }

    [Test]
    public void MaxDateTimePrecisionIsOneSecond()
    {
        var date1 = new DateTime(2020, 06, 01, 18, 18, 18).AddTicks(20);
        var date2 = new DateTime(2020, 06, 01, 18, 18, 18).AddTicks(30);
        var dtp = new DateTimePeriod(date1, date2);
        Assert.Multiple(() =>
        {
            Assert.AreEqual(dtp.Start, dtp.End);
            Assert.AreNotEqual(date1, dtp.Start);
        });
    }

    [TestCase("2020-06-01", "2020-06-03", "2020-06-01", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-03", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-02", true)]
    [TestCase("2020-06-20", "2020-06-23", "2020-12-31", false)]
    [TestCase(null, null, null, false)]
    public void ContainsNullable(DateTime? date1, DateTime? date2, DateTime? contains, bool expected)
    {
        var dtp = new DateTimePeriod(date1, date2);
        Assert.AreEqual(expected, dtp.Contains(contains));
    }

    [TestCase("2020-06-01", "2020-06-03", "2020-06-01", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-03", true)]
    [TestCase("2020-06-01", "2020-06-03", "2020-06-02", true)]
    [TestCase("2020-06-20", "2020-06-23", "2020-12-31", false)]
    [TestCase(null, null, "2020-12-31", false)]
    public void Contains(DateTime? date1, DateTime? date2, DateTime contains, bool expected)
    {
        var dtp = new DateTimePeriod(date1, date2);
        Assert.AreEqual(expected, dtp.Contains(contains));
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
        var dtp1 = new DateTimePeriod(new DateTime(2020, 06, 01), new DateTime(2020, 06, 04));
        var dtp2 = new DateTimePeriod(dt1, dt2);
        Assert.AreEqual(expected, dtp1.Overlaps(dtp2));
    }

    [TestCase(null, null, null)]
    [TestCase("2020-06-04", null, null)]
    [TestCase(null, "2020-07-30", null)]
    [TestCase("2020-06-05", "2020-07-30", "55:00:00:00")]
    [TestCase("2020-06-05 00:00:00", "2020-06-05 23:59:59", "00:23:59:59")]
    public void DurationNullable(DateTime? dt1, DateTime? dt2, TimeSpan? expected)
    {
        var dtp = new DateTimePeriod(dt1, dt2);
        Assert.AreEqual(expected, dtp.Duration(true));
    }

    [TestCase("2020-06-05", "2020-07-30", "55:00:00:00")]
    [TestCase("2020-06-05 00:00:00", "2020-06-05 23:59:59", "00:23:59:59")]
    public void Duration(DateTime dt1, DateTime dt2, TimeSpan expected)
    {
        var dtp = new DateTimePeriod(dt1, dt2);
        Assert.AreEqual(expected, dtp.Duration());
    }
}