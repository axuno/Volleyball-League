using Microsoft.Extensions.Logging;

namespace TournamentManager.Importers.ExcludeDates;

public class GermanHolidayImporter : IExcludeDateImporter
{
    private readonly string? _specialHolidaysXmlFile;
    private readonly Predicate<Axuno.Tools.GermanHoliday> _holidayFilter;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<GermanHolidayImporter> _logger;

    public GermanHolidayImporter(string? specialHolidaysXmlFile, Predicate<Axuno.Tools.GermanHoliday> holidayFilter, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        ILogger<GermanHolidayImporter> logger)
    {
        _specialHolidaysXmlFile = specialHolidaysXmlFile;
        _holidayFilter = holidayFilter;
        _timeZoneConverter = timeZoneConverter;
        _logger = logger;
    }

    public IEnumerable<ExcludeDateRecord> Import(DateTimePeriod dateLimits)
    {
        if (dateLimits is not { Start: not null, End: not null })
            throw new ArgumentException(@"Lower and upper date limits must be set.", nameof(dateLimits));

        _logger.LogDebug("Starting import of German holidays for period {dateStart} to {dateEnd}", dateLimits.Start, dateLimits.End);

        var currentYear = ((DateTime) dateLimits.Start).Year;

        // Stores all holidays within the dateLimits
        // This data will be returned
        var holidays = new List<Axuno.Tools.GermanHoliday>();

        while (currentYear <= ((DateTime) dateLimits.End).Year)
        {
            _logger.LogDebug("Processing year '{currentYear}'", currentYear);
            var currentYearHolidays = new Axuno.Tools.GermanHolidays(currentYear);

            // This generates all holidays
            currentYearHolidays.GenerateGermanHolidays();
            _logger.LogDebug("Generated and added {count} holidays.", currentYearHolidays.Count);

            // Loading custom holidays expects that German holiday are already in the list,
            // because otherwise the "Replace" command will not succeed
            if (!string.IsNullOrEmpty(_specialHolidaysXmlFile))
            {
                // The holidays file must be imported **for each year**
                currentYearHolidays.Load(_specialHolidaysXmlFile);
                _logger.LogDebug("Holidays from file '{filename}' loaded. Now counts {holidaysCount} for year {year}", _specialHolidaysXmlFile, currentYearHolidays.Count, currentYear);
            }

            holidays.AddRange(currentYearHolidays.GetFiltered(_holidayFilter));
            currentYear++;
        }

        return Map(holidays, dateLimits);
    }

    private IEnumerable<ExcludeDateRecord> Map(List<Axuno.Tools.GermanHoliday> holidays, DateTimePeriod dateLimits)
    {
        // sort short date ranges before big ranges
        var holidayGroups = holidays.ConsecutiveRanges()
            .OrderBy(tuple => tuple.From.Date).ThenBy(tuple => (tuple.To - tuple.From).Days);

        foreach (var holidayGroup in holidayGroups)
        {
            if (!dateLimits.Contains(holidayGroup.From) && !dateLimits.Contains(holidayGroup.To)) continue;

            // convert from import time zone to UTC
            var from = _timeZoneConverter.ToUtc(holidayGroup.From.Date);
            var to = _timeZoneConverter.ToUtc(holidayGroup.To.AddDays(1).AddSeconds(-1));

            // Swap if needed
            if (from > to) (from, to) = (to, from);

            yield return new ExcludeDateRecord(new DateTimePeriod(from, to), holidayGroup.Name);
        }
    }
}
