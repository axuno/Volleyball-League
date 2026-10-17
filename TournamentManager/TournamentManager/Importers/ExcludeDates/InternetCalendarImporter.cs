using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.Extensions.Logging;
using NodaTime;

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
        _iCalendarStreamReader.DiscardBufferedData();

        var iCal = CalendarSerializer.Deserialize<Ical.Net.Calendar>(_iCalendarStreamReader.BaseStream);

        _logger.LogInformation("Imported {Count} events from iCalendar", iCal.Events.Count);
        return Map(iCal, fromToTimePeriod);
    }

    private IEnumerable<ExcludeDateRecord> Map(Ical.Net.Calendar iCal, DateTimePeriod dateLimits)
    {
        // small come before big date ranges
        foreach (var calendarEvent in iCal.Events.OrderBy(e => e.DtStart!.Date).ThenBy(e => e.Duration?.Days))
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

        calendarEvent.Start = calendarEvent.Start.TzId is null
            ? CalDateTime.FromDateTime(calendarEvent.Start.ToDateTimeUnspecified(), _defaultTimeZoneId)
            : calendarEvent.Start;
        var start = calendarEvent.Start.ToLocalDateTime().InUtc();

        ZonedDateTime end;
        if (calendarEvent.End != null)
        {
            calendarEvent.End = calendarEvent.End.TzId is null
                ? CalDateTime.FromDateTime(calendarEvent.End.ToDateTimeUnspecified(), _defaultTimeZoneId)
                : calendarEvent.End;
            end = calendarEvent.End.ToLocalDateTime().InUtc().PlusSeconds(-1);
        }
        else
            end = start.LocalDateTime.PlusDays(1).PlusSeconds(-1).InZoneLeniently(start.Zone);

        return new(new(start.ToDateTimeUtc(), end.ToDateTimeUtc()), calendarEvent.Summary ?? string.Empty);
    }
}
