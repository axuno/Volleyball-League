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
}
