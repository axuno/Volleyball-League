using System.Collections.ObjectModel;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Importers.ExcludeDates;

public class GermanHolidayImporter : IExcludeDateImporter
{
    private readonly ICollection<string> _specialHolidaysXmlFiles;
    private readonly Predicate<Axuno.Tools.GermanHoliday> _holidayFilter;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<GermanHolidayImporter> _logger;

    public GermanHolidayImporter(Predicate<Axuno.Tools.GermanHoliday> holidayFilter, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        ILogger<GermanHolidayImporter> logger)
    {
        _specialHolidaysXmlFiles = new Collection<string>();
        _holidayFilter = holidayFilter;
        _timeZoneConverter = timeZoneConverter;
        _logger = logger;
    }

    public GermanHolidayImporter(ICollection<string> specialHolidaysXmlFiles, Predicate<Axuno.Tools.GermanHoliday> holidayFilter, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        ILogger<GermanHolidayImporter> logger) : this(holidayFilter, timeZoneConverter, logger)
    {
        _specialHolidaysXmlFiles = specialHolidaysXmlFiles;
    }

    public GermanHolidayImporter(string specialHolidaysXmlFile, Predicate<Axuno.Tools.GermanHoliday> holidayFilter, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        ILogger<GermanHolidayImporter> logger) : this(holidayFilter, timeZoneConverter, logger)
    {
        _specialHolidaysXmlFiles = string.IsNullOrEmpty(specialHolidaysXmlFile)
            ? Array.Empty<string>()
            : new Collection<string> { specialHolidaysXmlFile };
    }

    public IEnumerable<ExcludeDateRecord> Import(DateTimePeriod fromToTimePeriod)
    {
        if (fromToTimePeriod is not { Start: not null, End: not null })
            throw new ArgumentException(@"Lower and upper date limits must be set.", nameof(fromToTimePeriod));

        _logger.LogDebug("Starting import of German holidays for period {DateStart} to {DateEnd}", fromToTimePeriod.Start, fromToTimePeriod.End);

        var currentYear = ((DateTime) fromToTimePeriod.Start).Year;

        // Stores all holidays within the dateLimits
        // This data will be returned
        var holidays = new List<Axuno.Tools.GermanHoliday>();

        while (currentYear <= ((DateTime) fromToTimePeriod.End).Year)
        {
            _logger.LogDebug("Processing year '{CurrentYear}'", currentYear);
            var currentYearHolidays = new Axuno.Tools.GermanHolidays(currentYear);

            // This generates all holidays
            currentYearHolidays.GenerateGermanHolidays();
            _logger.LogDebug("Generated and added {Count} holidays.", currentYearHolidays.Count);

            // Loading custom holidays expects that German holiday are already in the list,
            // because otherwise the "Replace" command will not succeed
            foreach (var file in _specialHolidaysXmlFiles)
            {
                // The holidays file must be imported **for each year**
                currentYearHolidays.Load(file);
                _logger.LogDebug("Holidays from file '{Filename}' loaded. Now counts {HolidaysCount} for year {Year}", _specialHolidaysXmlFiles, currentYearHolidays.Count, currentYear);
            }

            holidays.AddRange(currentYearHolidays.GetFiltered(_holidayFilter));
            currentYear++;
        }

        return Map(holidays, fromToTimePeriod);
    }

    private IEnumerable<ExcludeDateRecord> Map(List<Axuno.Tools.GermanHoliday> holidays, DateTimePeriod dateLimits)
    {
        // sort short date ranges before big ranges after consecutive ranges are calculated
        var holidayGroups =
            holidays.ConsecutiveRanges()
            .OrderBy(tuple => tuple.From.Date)
            .ThenBy(tuple => (tuple.To - tuple.From).Days);

        foreach (var holidayGroup in holidayGroups)
        {
            if (!dateLimits.Contains(holidayGroup.From) && !dateLimits.Contains(holidayGroup.To)) continue;

            // convert from import time zone to UTC
            var from = _timeZoneConverter.ToUtc(holidayGroup.From.Date);
            var to = _timeZoneConverter.ToUtc(holidayGroup.To.AddDays(1).AddSeconds(-1));

            // Swap if needed
            if (from > to) (from, to) = (to, from);

            yield return new(new(from, to), holidayGroup.Name);
        }
    }
}
