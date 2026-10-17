using System.Globalization;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers.ExcludeDates;

[TestFixture]
public class InternetCalendarImporterTests
{
    [TestCase("2024-01-01", "2024-12-31", 7, "4.23:59:59")]
    [TestCase("2024-07-01", "2024-12-31", 4, "42.23:59:59")]
    public void Import_InternetCalender_From_String(DateTime from, DateTime to, int expectedCount, TimeSpan expectedDuration)
    {
        var icsFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "School_Holidays_Bavaria_2024.ics");
        // using CET as time zone
        _ = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);

        var icsImporter = new InternetCalendarImporter(File.ReadAllText(icsFilePath, Encoding.UTF8), "Europe/Berlin", NullLogger<InternetCalendarImporter>.Instance);

        var imported = icsImporter.Import(new(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
        Assert.That(imported[0].Period.Duration(), Is.EqualTo(expectedDuration));
    }

    [TestCase("2024-01-01", "2024-12-31", 7, "4.23:59:59")]
    [TestCase("2024-07-01", "2024-12-31", 4, "42.23:59:59")]
    public void Import_InternetCalender_From_Stream(DateTime from, DateTime to, int expectedCount, TimeSpan expectedDuration)
    {
        var icsFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "School_Holidays_Bavaria_2024.ics");

        var encoding = Encoding.UTF8;
        // Test with a stream
        var iCalendarStream = new MemoryStream(encoding.GetBytes(File.ReadAllText(icsFilePath, Encoding.UTF8)))
        {
            Position = 0
        };

        // using CET as time zone, unless dates in the calendar are flagged different
        var icsImporter = new InternetCalendarImporter(iCalendarStream, encoding, "Europe/Berlin", NullLogger<InternetCalendarImporter>.Instance);

        var imported = icsImporter.Import(new(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
        Assert.That(imported[0].Period.Duration(), Is.EqualTo(expectedDuration));
    }

    /// <summary>
    /// Builds a minimal, valid iCalendar text containing a single all-day <c>VEVENT</c>
    /// with a <c>DTSTART;VALUE=DATE</c> and no <c>DTEND</c>, so that
    /// <see cref="InternetCalendarImporter"/> falls back to computing the event's end
    /// as "start + 1 day - 1 second" (the branch without an explicit calendar End).
    /// </summary>
    private static string BuildSingleDayEventWithoutDtEnd(string dtStartDate)
    {
        return "BEGIN:VCALENDAR\r\n" +
               "VERSION:2.0\r\n" +
               "METHOD:PUBLISH\r\n" +
               "PRODID:-//Tests//InternetCalendarImporter//EN\r\n" +
               "BEGIN:VEVENT\r\n" +
               "CREATED:20240101T000000Z\r\n" +
               "LAST-MODIFIED:20240101T000000Z\r\n" +
               "DTSTAMP:20240101T000000Z\r\n" +
               "SUMMARY:Single day without DTEND\r\n" +
               $"DTSTART;VALUE=DATE:{dtStartDate}\r\n" +
               "TRANSP:TRANSPARENT\r\n" +
               "UID:no-dtend-test@example.com\r\n" +
               "END:VEVENT\r\n" +
               "END:VCALENDAR\r\n";
    }

    [Test]
    public void Import_Event_Without_DtEnd_Computes_EndOfDay()
    {
        // An all-day event with no DTEND, on a date without any DST transition.
        const string dtStartDate = "20240815";
        var ics = BuildSingleDayEventWithoutDtEnd(dtStartDate);

        var icsImporter = new InternetCalendarImporter(ics, "Europe/Berlin", NullLogger<InternetCalendarImporter>.Instance);

        var imported = icsImporter.Import(new(new DateTime(2024, 8, 1), new DateTime(2024, 8, 31))).ToList();

        Assert.That(imported, Has.Count.EqualTo(1));
        // Duration must be exactly 23:59:59 because End = Start + 1 day - 1 second
        Assert.That(imported[0].Period.Duration(), Is.EqualTo(TimeSpan.Parse("23:59:59")));
    }

    [Test]
    public void Import_Event_Without_DtEnd_On_DST_Transition_Date_Computes_EndOfDay()
    {
        // Europe/Berlin switches from CET (UTC+1) to CEST (UTC+2) on 2024-03-31 at 02:00 local time
        // (clocks jump from 02:00 to 03:00, so 02:00-03:00 does not exist that day).
        // An all-day event with no DTEND starting on this date exercises the
        // "end = start.LocalDateTime.PlusDays(1).PlusSeconds(-1).InZoneLeniently(start.Zone)"
        // branch across a DST boundary and must not throw, resolving leniently.
        const string dtStartDate = "20240331";
        var ics = BuildSingleDayEventWithoutDtEnd(dtStartDate);

        var icsImporter = new InternetCalendarImporter(ics, "Europe/Berlin", NullLogger<InternetCalendarImporter>.Instance);

        List<ExcludeDateRecord> imported = null!;
        Assert.DoesNotThrow(() =>
            imported = icsImporter.Import(new(new DateTime(2024, 3, 1), new DateTime(2024, 4, 30))).ToList());

        Assert.That(imported, Has.Count.EqualTo(1));
        // Even across the DST gap, the computed local end-of-day must still be
        // 23:59:59 after start (the gap is at 02:00, not at the end of the day).
        Assert.That(imported[0].Period.Duration(), Is.EqualTo(TimeSpan.Parse("23:59:59")));
    }
}
