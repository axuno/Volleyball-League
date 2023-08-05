using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.Importers.ExcludedDates;

public class InternetCalendarImporter
{
    /// <summary>
    /// Imports the <see langword="string"/> representation of an iCalendar.
    /// <see cref="ExcludeMatchDateEntity.TournamentId"/>, <see cref="ExcludeMatchDateEntity.RoundId"/> and <see cref="ExcludeMatchDateEntity.TeamId"/> will not be set.
    /// </summary>
    /// <param name="iCalendarString"></param>
    /// <param name="defaultTimeZoneId">The time zone to use, if it is not included in the start and end date/time of the event.</param>
    /// <param name="dateLimits">The lower and upper UTC date limit to import.</param>
    /// <returns>Return an <see cref="EntityCollection"/> of type  <see cref="ExcludeMatchDateEntity"/> with imported dates.</returns>
    public static EntityCollection<ExcludeMatchDateEntity> Import(string iCalendarString, string defaultTimeZoneId, DateTimePeriod dateLimits)
    {
        var iCal = Ical.Net.Calendar.Load(iCalendarString);
        return Map(iCal, defaultTimeZoneId, dateLimits);
    }

    /// <summary>
    /// Imports the <seealso cref="Stream"/> representation of an iCalendar.
    /// <see cref="ExcludeMatchDateEntity.TournamentId"/>, <see cref="ExcludeMatchDateEntity.RoundId"/> and <see cref="ExcludeMatchDateEntity.TeamId"/> will not be set.
    /// </summary>
    /// <param name="iCalendarStream">The <seealso cref="Stream"/>.</param>
    /// <param name="encoding">The <seealso cref="System.Text.Encoding"/> to use.</param>
    /// <param name="defaultTimeZoneId">The time zone to use, if it is not included in the start and end date/time of the event.</param>
    /// <param name="dateLimits">The lower and upper UTC date limit to import.</param>
    /// <returns>Return an <see cref="EntityCollection"/> of type  <see cref="ExcludeMatchDateEntity"/> with imported dates.</returns>
    public static EntityCollection<ExcludeMatchDateEntity> Import(Stream iCalendarStream, System.Text.Encoding encoding, string defaultTimeZoneId, DateTimePeriod dateLimits)
    {
        var iCal = Ical.Net.Calendar.Load(new StreamReader(iCalendarStream, encoding));
        return Map(iCal, defaultTimeZoneId, dateLimits);
    }

    private static EntityCollection<ExcludeMatchDateEntity> Map(Ical.Net.Calendar iCal, string defaultTimeZoneId, DateTimePeriod dateLimits)
    {
        var excluded = new EntityCollection<ExcludeMatchDateEntity>();
            
        // small come before big date ranges
        foreach (var calendarEvent in iCal.Events.OrderBy(e => e.DtStart.Date).ThenBy(e => e.Duration.Days))
        {
            var exclDate = CreateEntity(calendarEvent, defaultTimeZoneId);
            if (dateLimits.Contains(exclDate.DateFrom) || dateLimits.Contains(exclDate.DateTo))
            {
                excluded.Add(CreateEntity(calendarEvent, defaultTimeZoneId));
            }
        }

        return excluded;
    }

    /// <summary>
    /// Creates an <see cref="ExcludeMatchDateEntity"/> from an <see cref="Ical.Net.CalendarComponents.CalendarEvent"/> if plausibility criteria are met.
    /// The <see cref="ExcludeMatchDateEntity.DateFrom"/> and <see cref="ExcludeMatchDateEntity.DateTo"/> are in UTC.
    /// </summary>
    /// <param name="calendarEvent"></param>
    /// <param name="defaultTimeZoneId">The time zone to use, if the it is not included in the start and end date/time of the event.</param>
    /// <returns>Returns the <see cref="ExcludeMatchDateEntity"/> created from the <see cref="Ical.Net.CalendarComponents.CalendarEvent"/>.</returns>
    private static ExcludeMatchDateEntity CreateEntity(Ical.Net.CalendarComponents.CalendarEvent calendarEvent, string defaultTimeZoneId)
    {
        var excludeMatchDate = new ExcludeMatchDateEntity();
            
        if (calendarEvent.Start == null)
        {
            throw new ArgumentException($"Could not create {nameof(ExcludeMatchDateEntity)} from {nameof(Ical.Net.CalendarComponents.CalendarEvent)} Start={calendarEvent.Start}, End={calendarEvent.End}, Name={calendarEvent.Description}", nameof(calendarEvent));
        }

        calendarEvent.Start.TzId ??= defaultTimeZoneId;
        var start = calendarEvent.Start.AsUtc;

        DateTime end;
        if (calendarEvent.End != null)
        {
            calendarEvent.End.TzId ??= defaultTimeZoneId;
            end = calendarEvent.End.AsUtc;
        }
        else
        {
            end = start.Date;
        }

        // Swap if necessary
        if (start > end) (start, end) = (end, start);

        excludeMatchDate.DateFrom = start;
        excludeMatchDate.DateTo = end;
        excludeMatchDate.Reason = calendarEvent.Summary;

        return excludeMatchDate;
    }
}