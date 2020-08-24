using System;
using System.IO;
using TournamentManager.DAL.EntityClasses;
using TournamentManager.DAL.HelperClasses;

namespace TournamentManager.Plan
{
    public class ExcludedDates
    {
        /// <summary>
        /// Imports the <see langword="string"/> representation of an iCalendar.
        /// <see cref="ExcludeMatchDateEntity.TournamentId"/>, <see cref="ExcludeMatchDateEntity.RoundId"/> and <see cref="ExcludeMatchDateEntity.TeamId"/> will not be set.
        /// </summary>
        /// <param name="iCalendarString"></param>
        /// <param name="defaultTimeZoneId">The time zone to use, if it is not included in the start and end date/time of the event.</param>
        /// <returns></returns>
        public EntityCollection<ExcludeMatchDateEntity> Import(string iCalendarString, string defaultTimeZoneId)
        {
            var iCal = Ical.Net.Calendar.Load(iCalendarString);
            return Map(iCal, defaultTimeZoneId);
        }

        /// <summary>
        /// Imports the <seealso cref="Stream"/> representation of an iCalendar.
        /// <see cref="ExcludeMatchDateEntity.TournamentId"/>, <see cref="ExcludeMatchDateEntity.RoundId"/> and <see cref="ExcludeMatchDateEntity.TeamId"/> will not be set.
        /// </summary>
        /// <param name="iCalendarStream">The <seealso cref="Stream"/>.</param>
        /// <param name="encoding">The <seealso cref="System.Text.Encoding"/> to use.</param>
        /// <param name="defaultTimeZoneId">The time zone to use, if it is not included in the start and end date/time of the event.</param>
        /// <returns></returns>
        public EntityCollection<ExcludeMatchDateEntity> Import(Stream iCalendarStream, System.Text.Encoding encoding, string defaultTimeZoneId)
        {
            var iCal = Ical.Net.Calendar.Load(new StreamReader(iCalendarStream, encoding));
            return Map(iCal, defaultTimeZoneId);
        }

        private EntityCollection<ExcludeMatchDateEntity> Map(Ical.Net.Calendar iCal, string defaultTimeZoneId)
        {
            var excluded = new EntityCollection<ExcludeMatchDateEntity>();
            
            foreach (var calendarEvent in iCal.Events)
            {
                if (TryCreateEntity(calendarEvent, defaultTimeZoneId, out var excludeMatchDate))
                {
                    excluded.Add(excludeMatchDate);
                }
            }

            return excluded;
        }

        /// <summary>
        /// Creates an <see cref="ExcludeMatchDateEntity"/> from an <see cref="Ical.Net.CalendarComponents.CalendarEvent"/> if plausibility criteria are met.
        /// The <see cref="ExcludeMatchDateEntity.DateFrom"/> and <see cref="ExcludeMatchDateEntity.DateTo"/> are in UTC. For whole day events,
        /// <seealso cref="ExcludeMatchDateEntity.DateFrom"/> and <see cref="ExcludeMatchDateEntity.DateTo"/> represent the UTC start and end of the day.
        /// </summary>
        /// <param name="calendarEvent"></param>
        /// <param name="defaultTimeZoneId">The time zone to use, if the it is not included in the start and end date/time of the event.</param>
        /// <param name="excludeMatchDate">The <see cref="ExcludeMatchDateEntity"/> set, if it can be filled successfully.</param>
        /// <returns>Returns <see langword="true"/>, if the <seealso cref="Ical.Net.CalendarComponents.CalendarEvent"/> could be processed successfully, else <see langword="false"/>.</returns>
        private static bool TryCreateEntity(Ical.Net.CalendarComponents.CalendarEvent calendarEvent, string defaultTimeZoneId, out ExcludeMatchDateEntity excludeMatchDate)
        {
            excludeMatchDate = new ExcludeMatchDateEntity();

            if (calendarEvent.Start == null) return false;

            if (calendarEvent.Start.HasTime && calendarEvent.End != null && calendarEvent.End.HasTime)
            {
                if (calendarEvent.Start.Equals(calendarEvent.End)) return false;
            }

            calendarEvent.Start.TzId ??= defaultTimeZoneId;
            var start = calendarEvent.Start.AsUtc;

            DateTime end;
            if (calendarEvent.End != null)
            {
                calendarEvent.End.TzId ??= defaultTimeZoneId;
                end = !calendarEvent.End.HasTime ? calendarEvent.End.AsUtc.AddDays(1).AddSeconds(-1) : calendarEvent.End.AsUtc;
            }
            else
            {
                end = start.Date.AddDays(1).AddSeconds(-1);
            }

            // Swap if necessary
            if (start > end) (start, end) = (end, start);

            excludeMatchDate.DateFrom = start;
            excludeMatchDate.DateTo = end;
            excludeMatchDate.Reason = calendarEvent.Summary;

            return true;
        }
    }
}