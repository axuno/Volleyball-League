using Microsoft.Extensions.Logging;
using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.Importers.ExcludedDates;

[Obsolete("User ExcelImporter instead", true)]
public class GermanHolidayImporter
{
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<GermanHolidayImporter> _logger;

    public GermanHolidayImporter(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter,
        ILogger<GermanHolidayImporter> logger)
    {
        _timeZoneConverter = timeZoneConverter;
        _logger = logger;
    }

    public IEnumerable<ExcludeMatchDateEntity> Import(DateTimePeriod dateLimits,
        Predicate<Axuno.Tools.GermanHoliday> holidayFilter)
    {
        return Import(null, dateLimits, holidayFilter);
    }

    public IEnumerable<ExcludeMatchDateEntity> Import(string? specialHolidaysXmlFile, 
        DateTimePeriod dateLimits, Predicate<Axuno.Tools.GermanHoliday> holidayFilter)
    {
        if (!(dateLimits.Start.HasValue && dateLimits.End.HasValue))
        {
            throw new ArgumentException("Lower and upper date limits must be set.", nameof(dateLimits));
        }

        _logger.LogDebug("Starting import of German holidays for period {dateStart} to {dateEnd}", dateLimits.Start, dateLimits.End);

        var currentYear = ((DateTime) dateLimits.Start).Year;

        // stores all holidays within the dateLimits
        var holidays = new List<Axuno.Tools.GermanHoliday>();

        while (currentYear <= ((DateTime) dateLimits.End).Year)
        {
            _logger.LogDebug("Processing year '{currentYear}'", currentYear);
            var currentYearHolidays = new Axuno.Tools.GermanHolidays(currentYear);
            _logger.LogDebug("Generated {count} holidays.", currentYearHolidays.Count);

            // The holidays file must be imported **for each year**
            if (!string.IsNullOrEmpty(specialHolidaysXmlFile))
            {
                currentYearHolidays.Load(specialHolidaysXmlFile);
                _logger.LogDebug("Holidays from file '{filename}' loaded. Now counts {holidaysCount} for year {year}", specialHolidaysXmlFile, currentYearHolidays.Count, currentYear);
            }

            holidays.AddRange(currentYearHolidays.GetFiltered(holidayFilter));
                
            // Filter is now an argument to Import method:
            // filter by federal or Bavarian holidays, which are public, custom or school holidays
            /*holidays.AddRange(
                currentYearHolidays.GetFiltered(
                    h =>
                        (h.PublicHolidayStateIds.Count == 0 ||
                         h.PublicHolidayStateIds.Contains(Axuno.Tools.GermanFederalStates.Id.Bayern)) &&
                        (h.Type == Axuno.Tools.GermanHolidays.Type.Public ||
                         h.Type == Axuno.Tools.GermanHolidays.Type.Custom ||
                         h.Type == Axuno.Tools.GermanHolidays.Type.School)));
            */
            currentYear++;
        }

        return Map(holidays, dateLimits);
    }

    private IEnumerable<ExcludeMatchDateEntity> Map(List<Axuno.Tools.GermanHoliday> holidays, DateTimePeriod dateLimits)
    {
        // sort short date ranges before big ranges
        var holidayGroups = holidays.ConsecutiveRanges()
            .OrderBy(tuple => tuple.From.Date).ThenBy(tuple => (tuple.To - tuple.From).Days);

        foreach (var holidayGroup in holidayGroups)
        {
            var entity = CreateEntity(holidayGroup);
            if (!dateLimits.Contains(entity.DateFrom) && !dateLimits.Contains(entity.DateTo)) continue;
                
            // convert from import time zone to UTC
            entity.DateFrom = _timeZoneConverter.ToUtc(entity.DateFrom.Date);
            entity.DateTo = _timeZoneConverter.ToUtc(entity.DateTo.AddDays(1).AddSeconds(-1));

            yield return entity;
        }
    }

    /// <summary>
    /// Creates an <see cref="ExcludeMatchDateEntity"/> from an <see cref="Ical.Net.CalendarComponents.CalendarEvent"/> if plausibility criteria are met.
    /// The <see cref="ExcludeMatchDateEntity.DateFrom"/> and <see cref="ExcludeMatchDateEntity.DateTo"/> are in UTC.
    /// </summary>
    /// <param name="holidayGroup">Value Tuple with from date, to date and holiday name</param>
    /// <exception cref="ArgumentException"></exception>
    private static ExcludeMatchDateEntity CreateEntity((DateTime From, DateTime To, string Name) holidayGroup)
    {
        var excludeMatchDate = new ExcludeMatchDateEntity();

        if (holidayGroup.From == DateTime.MinValue || holidayGroup.To == DateTime.MinValue ||
            string.IsNullOrWhiteSpace(holidayGroup.Name))
        {
            throw new ArgumentException($"Could not create {nameof(ExcludeMatchDateEntity)} from {nameof(ValueTuple<Axuno.Tools.GermanHoliday>)} From={holidayGroup.From.Date}, To={holidayGroup.To.Date}, Name={holidayGroup.Name}", nameof(holidayGroup));
        }

        // Swap if necessary
        if (holidayGroup.From > holidayGroup.To) (holidayGroup.From, holidayGroup.To) = (holidayGroup.To, holidayGroup.From);
            
        excludeMatchDate.DateFrom = holidayGroup.From.Date;
        excludeMatchDate.DateTo = holidayGroup.To.Date;
        excludeMatchDate.Reason = holidayGroup.Name;

        return excludeMatchDate;
    }
}