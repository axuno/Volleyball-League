using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers.ExcludeDates;

[TestFixture]
public class ExcelImporterTests
{
    [TestCase("2020-11-01", "2021-05-31", 24)] // 2020-2021
    [TestCase("2021-01-01", "2021-12-31", 17)] // only 2021
    [TestCase("2020-12-24", "2020-12-24", 1)] // 1 day
    [TestCase("2022-05-01", "2022-05-01", 0)] // out-of-range
    public void Import(DateTime from, DateTime to, int expectedCount)
    {
        var xlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "ExcludedDates.xlsx");
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);
        var xlImporter = new ExcelImporter(xlFilePath, tzConverter, NullLogger<ExcelImporter>.Instance);

        var imported = xlImporter.Import(new(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
        if (expectedCount == 1)
            Assert.That(imported[0].Period.Duration(), Is.EqualTo(new TimeSpan(23, 59, 59)));
    }

    [TestCase("2022-01-01", "2022-01-01")] // one day that overlaps
    [TestCase("2022-02-01", "2022-02-01")] // one day that overlaps
    public void ImportShouldSwapFromTo(DateTime from, DateTime to)
    {
        var xlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "ExcludedDates.xlsx");
        // Using UTC as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "UTC", CultureInfo.CurrentCulture);
        var xlImporter = new ExcelImporter(xlFilePath, tzConverter, NullLogger<ExcelImporter>.Instance);

        var imported = xlImporter.Import(new(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(1));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(imported[0].Period.Start, Is.EqualTo(new DateTime(2022, 1, 1)));
            Assert.That(imported[0].Period.End, Is.EqualTo(new DateTime(2022, 2, 1, 23, 59, 59)));
        }
    }
}
