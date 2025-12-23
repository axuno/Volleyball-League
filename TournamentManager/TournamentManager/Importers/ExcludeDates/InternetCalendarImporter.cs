using Ical.Net.DataTypes;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Importers.ExcludeDates;

public class InternetCalendarImporter : IExcludeDateImporter
{
    private readonly StreamReader _iCalendarStreamReader;
    private readonly string _defaultTimeZoneId;
    private readonly ILogger _logger;

    public InternetCalendarImporter(Stream iCalendarStream, System.Text.Encoding streamEncoding, string defaultTimeZoneId, ILogger<InternetCalendarImporter> logger)
    {
        iCalendarStream.Position = 0;
        _iCalendarStreamReader = new(iCalendarStream, streamEncoding);
        _defaultTimeZoneId = defaultTimeZoneId;
        _logger = logger;
    }

    public InternetCalendarImporter(string calendarString, string defaultTimeZoneId, ILogger<InternetCalendarImporter> logger)
    {
        var encoding = System.Text.Encoding.UTF8;
        var iCalendarStream = new MemoryStream(encoding.GetBytes(calendarString))
        {
            Position = 0
        };
        _iCalendarStreamReader = new(iCalendarStream, encoding);
        _defaultTimeZoneId = defaultTimeZoneId;
        _logger = logger;
    }

    /// <summary>
    /// Imports the <seealso cref="Stream"/> representation of an iCalendar.
    /// </summary>
    /// <param name="fromToTimePeriod">The lower and upper UTC date limit to import.</param>
    /// <returns>Return an <see cref="IEnumerable{T}"/> of type  <see cref="ExcludeDateRecord"/> with imported dates.</returns>
    public IEnumerable<ExcludeDateRecord> Import(DateTimePeriod fromToTimePeriod)
    {
        _iCalendarStreamReader.BaseStream.Position = 0;
        var iCal = Ical.Net.Calendar.Load(_iCalendarStreamReader)!;
        _logger.LogInformation("Imported {Count} events from iCalendar", iCal.Events.Count);
        return Map(iCal, fromToTimePeriod);
    }

    private IEnumerable<ExcludeDateRecord> Map(Ical.Net.Calendar iCal, DateTimePeriod dateLimits)
    {
        // small come before big date ranges
        foreach (var calendarEvent in iCal.Events.OrderBy(e => e.DtStart!.Date).ThenBy(e => e.EffectiveDuration.Days))
        {
            var exclDate = CreateRecord(calendarEvent);
            if (dateLimits.Contains(exclDate.Period.Start) || dateLimits.Contains(exclDate.Period.End))
                yield return exclDate;
        }
    }

    /// <summary>
    /// Creates an <see cref="ExcludeDateRecord"/> from an <see cref="Ical.Net.CalendarComponents.CalendarEvent"/> if plausibility criteria are met.
    /// The <see cref="DateTime"/> <c>Start</c> and <see cref="DateTime"/> <c>End</c> are in UTC.
    /// </summary>
    /// <param name="calendarEvent"></param>
    /// <returns>Returns the <see cref="ExcludeDateRecord"/> created from the <see cref="Ical.Net.CalendarComponents.CalendarEvent"/>.</returns>
    private ExcludeDateRecord CreateRecord(Ical.Net.CalendarComponents.CalendarEvent calendarEvent)
    {
        if (calendarEvent.Start == null)
            throw new ArgumentException(@$"Could not create {nameof(ExcludeDateRecord)} from {nameof(Ical.Net.CalendarComponents.CalendarEvent)} Start={calendarEvent.Start}, End={calendarEvent.End}, Name={calendarEvent.Description}", nameof(calendarEvent));

        calendarEvent.Start = calendarEvent.Start.TzId is null ? new(calendarEvent.Start.Date, calendarEvent.Start.Time, _defaultTimeZoneId) : calendarEvent.Start;
        var start = calendarEvent.Start.AsUtc;

        DateTime end;
        if (calendarEvent.End != null)
        {
            calendarEvent.End = calendarEvent.End.TzId is null ? new(calendarEvent.End.Date, calendarEvent.End.Time, _defaultTimeZoneId) : calendarEvent.Start;
            end = calendarEvent.End.AsUtc.AddSeconds(-1);
        }
        else
            end = start.Date.AddDays(1).AddSeconds(-1);

        // Swap if necessary
        if (start > end) (start, end) = (end, start);

        return new(new(start, end), calendarEvent.Summary ?? string.Empty);
    }
}
