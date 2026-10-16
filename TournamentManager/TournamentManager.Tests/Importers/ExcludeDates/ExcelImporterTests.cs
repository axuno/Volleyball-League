using System.Globalization;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers.ExcludeDates;

[TestFixture]
public class ExcelImporterTests
{
    [TestCase("2020-11-01", "2021-05-31")] // 2020-2021
    [TestCase("2021-01-01", "2021-12-31")] // only 2021
    [TestCase("2020-12-24", "2020-12-24")] // 1 day
    [TestCase("2022-05-01", "2022-05-01")] // out-of-range
    public void ImportShouldThrowWhenFromIsLaterThanToInWorkbook(DateTime from, DateTime to)
    {
        var xlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "ExcludedDates.xlsx");
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);
        var xlImporter = new ExcelImporter(xlFilePath, tzConverter, NullLogger<ExcelImporter>.Instance);

        // Row 26 in the workbook contains a 'From' date later than the 'To' date,
        // which must raise an exception regardless of the requested date range.
        Assert.Throws<InvalidOperationException>(() => xlImporter.Import(new(from, to)).ToList());
    }

    [TestCase("2022-01-01", "2022-01-01")] // one day that overlaps
    [TestCase("2022-02-01", "2022-02-01")] // one day that overlaps
    public void ImportShouldThrowWhenFromIsLaterThanTo(DateTime from, DateTime to)
    {
        var xlFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "ExcludedDates.xlsx");
        // Using UTC as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "UTC", CultureInfo.CurrentCulture);
        var xlImporter = new ExcelImporter(xlFilePath, tzConverter, NullLogger<ExcelImporter>.Instance);

        Assert.Throws<InvalidOperationException>(() => xlImporter.Import(new(from, to)).ToList());
    }
}
