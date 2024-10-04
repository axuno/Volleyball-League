﻿using System.Globalization;
using Axuno.Tools;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using TournamentManager.Importers.ExcludeDates;

namespace TournamentManager.Tests.Importers.ExcludeDates;

[TestFixture]
public class GermanHolidayImporterTests
{
    [TestCase("2023-01-01", "2023-12-31", 11)]
    [TestCase("2024-01-01", "2024-12-31", 11)]
    [TestCase("2023-01-01", "2024-12-31", 22)]
    public void Import_HolidaysInAllFederalStates(DateTime from, DateTime to, int expectedCount)
    {
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);
        var holidayFilter = new Predicate<GermanHoliday>(h =>
            h.Type == GermanHolidays.Type.Public && h.PublicHolidayStateIds.Count == new GermanFederalStates().Count);
        var hImporter =
            new GermanHolidayImporter(holidayFilter, tzConverter, NullLogger<GermanHolidayImporter>.Instance);

        var imported = hImporter.Import(new DateTimePeriod(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
    }

    [TestCase("2023-01-01", "2023-12-31", 14)]
    [TestCase("2024-01-01", "2024-12-31", 14)]
    [TestCase("2023-01-01", "2024-12-31", 28)]
    public void Import_HolidaysInBavaria(DateTime from, DateTime to, int expectedCount)
    {
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);
        var holidayFilter = new Predicate<GermanHoliday>(h =>
            h.Type == GermanHolidays.Type.Public && h.PublicHolidayStateIds.Contains(GermanFederalStates.Id.Bayern));
        var hImporter =
            new GermanHolidayImporter(holidayFilter, tzConverter, NullLogger<GermanHolidayImporter>.Instance);

        var imported = hImporter.Import(new DateTimePeriod(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
    }

    [TestCase("2023-01-01", "2023-12-31", 19)]
    [TestCase("2024-01-01", "2024-12-31", 19)]
    [TestCase("2023-01-01", "2024-12-31", 38)]
    public void Import_Holidays_Volleyball_League_Augsburg(DateTime from, DateTime to, int expectedCount)
    {
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);
        var holidayFilter = new Predicate<GermanHoliday>(
            h =>
                h.Type == GermanHolidays.Type.Public &&
                h.PublicHolidayStateIds.Contains(GermanFederalStates.Id.Bayern)
                // add 5 more local holidays
                || h.Id == GermanHolidays.Id.AugsburgerFriedensfest || h.Id == GermanHolidays.Id.HeiligerAbend ||
                h.Id == GermanHolidays.Id.RosenMontag || h.Id == GermanHolidays.Id.FaschingsDienstag ||
                h.Id == GermanHolidays.Id.Silvester);

        var hImporter =
            new GermanHolidayImporter(holidayFilter, tzConverter, NullLogger<GermanHolidayImporter>.Instance);

        var imported = hImporter.Import(new DateTimePeriod(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
    }

    [TestCase("2019-09-01", "2020-06-30", 6)]
    public void Import_With_Custom_School_Holidays(DateTime from, DateTime to, int expectedCount)
    {
        // Note: Custom_Holidays_Sample.xml contains 6 school holidays.
        // But as the command for them is "Merge", existing holidays persist unchanged,
        // and additional holiday periods are added.
        var customHolidayFilePath =
            Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Custom_Holidays_Sample.xml");
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);

        var holidayFilter = new Predicate<GermanHoliday>(h => h.Type == GermanHolidays.Type.School);
        var hImporter = new GermanHolidayImporter(customHolidayFilePath, holidayFilter, tzConverter,
            NullLogger<GermanHolidayImporter>.Instance);

        // The period from 2019-09-01 to 2020-06-30 contains 6 imported school holidays.
        var imported = hImporter.Import(new DateTimePeriod(from, to)).ToList();

        Assert.That(imported, Has.Count.EqualTo(expectedCount));
    }

    [Test]
    public void Add_And_Remove_Holiday()
    {
        // Note: Custom_Holidays_Sample.xml contains 6 school holidays.
        // But as the command for them is "Merge", existing holidays persist unchanged,
        // and additional holiday periods are added.
        var addHolidayFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Single_Holiday_To_Add.xml");
        var removeHolidayFilePath = Path.Combine(TestContext.CurrentContext.TestDirectory, "Assets", "Single_Holiday_To_Remove.xml");
        // using CET as time zone
        var tzConverter = new Axuno.Tools.DateAndTime.TimeZoneConverter(
            "Europe/Berlin", CultureInfo.CurrentCulture);

        var holidayFilter = new Predicate<GermanHoliday>(h => h.Type == GermanHolidays.Type.Public);
        var hImporter = new GermanHolidayImporter(new[] { removeHolidayFilePath, addHolidayFilePath }, holidayFilter,
            tzConverter, NullLogger<GermanHolidayImporter>.Instance);

        var imported = hImporter.Import(new DateTimePeriod(new DateTime(2024, 1, 1), new DateTime(2024, 1, 1))).ToList();


        Assert.Multiple(() =>
        {
            Assert.That(imported, Has.Count.EqualTo(1));
            Assert.That(imported[0].Reason, Is.EqualTo("Sample New Year"));
        });
        
    }
}
