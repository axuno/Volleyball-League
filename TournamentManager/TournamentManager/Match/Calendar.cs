using System.Text;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using TournamentManager.DAL.TypedViewClasses;

namespace TournamentManager.Match;

public class Calendar
{
    private readonly Ical.Net.Calendar _calendar = new();

    public Calendar()
    {
        // Don't use this: Organizer = new Organizer() { CommonName = "TournamentManager", Value = new Uri("mailto:noreply@tournamentmanager")};

        FirstAlarm = new TimeSpan(-7, 0, 0, 0, 0);
        SecondAlarm = new TimeSpan(-1, 0, 0, 0, 0);
    }

    /// <summary>
    /// The name of the calendar. Will be used by calendar clients. Default: Tournament Calendar 
    /// </summary>
    public string Name { get; set; } = "Tournament Calendar";

    /// <summary>
    /// The summary of the event
    /// </summary>
    public string Summary { get; set; } = "Volleyball Match";

    /// <summary>
    /// Format string for the UID property. Insert {0} as placeholder for the match id.
    /// </summary>
    public string UidFormat { get; set; } = "TournamentManager-{0}";

    /// <summary>
    /// Format string used for Google Maps in the description part. Insert {0} for Latitude, {1} for Longitude.
    /// </summary>
    public string DescriptionGoogleMapsFormat { get; set; } = "Venue in Google Maps: http://maps.google.com?q={0},{1}";

    /// <summary>
    /// Format string used for opponents in the description part. Insert {0} for home team, {1} for guest team.
    /// </summary>
    public string DescriptionOpponentsFormat { get; set; } = "Match '{0}' : '{1}'";

    /// <summary>
    /// The footer at the end of the description text.
    /// </summary>
    public string DescriptionFooter { get; set; } = string.Empty;

    /// <summary>
    /// The <see cref="TimeSpan"/> before the event, when an alarm is displayed on the calendar client.
    /// </summary>
    public TimeSpan FirstAlarm { get; set; }

    /// <summary>
    /// The <see cref="TimeSpan"/> before the event, when an alarm is displayed on the calendar client.
    /// </summary>
    public TimeSpan SecondAlarm { get; set; }

    /// <summary>
    /// Adds <see cref="FirstAlarm"/> and <see cref="SecondAlarm"/> to the event, if <c>true</c>.
    /// </summary>
    public bool WithAlarms { get; set; }

    /// <summary>
    /// Creates a calendar event for the matches.
    /// </summary>
    /// <param name="matches">The matches to use for the calendar events.</param>
    /// <param name="tzId">The timezone Id.</param>
    /// <returns>Returns a <see cref="Ical.Net.Calendar"/> instance.</returns>
    public Calendar CreateEvents(List<CalendarRow> matches, string tzId)
    {
        // Don't set Calendar.Method to "PUBLISH"
        if (matches.Count > 1)
        {
            _calendar.AddProperty("NAME", Name);
            _calendar.AddProperty("X-WR-CALNAME", Name);
        }
            
        foreach (var match in matches)
        {
            if (match.PlannedStart == null || match.PlannedEnd == null)
                throw new InvalidOperationException("PlannedStart and PlannedEnd must not be null for calendar events.");

            var evt = _calendar.Create<CalendarEvent>();
                
            evt.Summary = Summary;
            evt.Location = $"{match.VenueName}, {match.VenueStreet}, {match.VenuePostalCode} {match.VenueCity}";
            evt.Description = !string.IsNullOrWhiteSpace(DescriptionOpponentsFormat)
                ? string.Format(DescriptionOpponentsFormat, match.HomeTeamNameForRound, match.GuestTeamNameForRound)
                : string.Empty;
            if (match is { VenueLongitude: not null, VenueLatitude: not null } && !string.IsNullOrWhiteSpace(DescriptionGoogleMapsFormat))
                evt.Description += "\n" + string.Format(DescriptionGoogleMapsFormat,
                    match.VenueLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    match.VenueLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            evt.Description += "\n" + DescriptionFooter;
            evt.Sequence = (int)match.ChangeSerial;
            evt.Start = new CalDateTime(value: match.PlannedStart.Value, tzId);
            evt.End = new CalDateTime(value: match.PlannedEnd.Value, tzId);
            // evt.Created = new CalDateTime(match.ModifiedOn);
            evt.LastModified = new CalDateTime(match.ModifiedOn, "UTC");
            evt.DtStamp = new CalDateTime(DateTime.UtcNow);

            evt.Uid = string.Format(UidFormat, match.Id);
            evt.Status = EventStatus.Confirmed;
            evt.Class = "PRIVATE";
            evt.Transparency = "OPAQUE";
            evt.AddProperty("X-MICROSOFT-CDO-BUSYSTATUS", "BUSY");
               
            if (WithAlarms)
            {
                // first alarm
                evt.Alarms.Add(new Alarm
                    {
                        // Note: "Duration" property does NOT mean the length of the alarm ringing
                        // but the time span before the event!
                        Trigger = new Trigger { Duration = Duration.FromTimeSpanExact(FirstAlarm) },
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );

                // second alarm
                evt.Alarms.Add(new Alarm
                    {
                        Trigger = new Trigger { Duration = Duration.FromTimeSpanExact(SecondAlarm) },
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );
            }
            // Adding 'evt' to Events would cause double entries
        }
        return this;
    }

    /// <summary>
    /// Creates a public calendar with events for the matches.
    /// </summary>
    /// <param name="matches">
    /// The matches to use for the calendar events.
    /// Date/time values must be in UTC.
    /// </param>
    /// <returns>Returns a <see cref="Ical.Net.Calendar"/> instance.</returns>
    public Calendar CreatePublicCalendar(List<CalendarRow> matches)
    {
        _calendar.Method = "PUBLISH";

        _calendar.AddProperty("NAME", Name);
        _calendar.AddProperty("X-WR-CALNAME", Name);
        _calendar.AddProperty("REFRESH-INTERVAL;VALUE=DURATION", "P24H");

        foreach (var match in matches)
        {
            if (match.PlannedStart == null || match.PlannedEnd == null)
                throw new InvalidOperationException("PlannedStart and PlannedEnd must not be null for calendar events.");

            var evt = _calendar.Create<CalendarEvent>();

            evt.Summary = Summary;
            evt.Location = $"{match.VenueName}, {match.VenueStreet}, {match.VenuePostalCode} {match.VenueCity}";
            evt.Description = !string.IsNullOrWhiteSpace(DescriptionOpponentsFormat)
                ? string.Format(DescriptionOpponentsFormat, match.HomeTeamNameForRound, match.GuestTeamNameForRound)
                : string.Empty;
            if (match is { VenueLongitude: not null, VenueLatitude: not null } && !string.IsNullOrWhiteSpace(DescriptionGoogleMapsFormat))
                evt.Description += "\n" + string.Format(DescriptionGoogleMapsFormat,
                    match.VenueLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    match.VenueLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            evt.Description += "\n" + DescriptionFooter;
            evt.Sequence = (int)match.ChangeSerial;
            evt.Start = new CalDateTime(value: match.PlannedStart.Value, "UTC");
            evt.End = new CalDateTime(value: match.PlannedEnd.Value, "UTC");
            // evt.Created = new CalDateTime(match.ModifiedOn);
            evt.LastModified = new CalDateTime(match.ModifiedOn, "UTC");
            evt.DtStamp = new CalDateTime(DateTime.UtcNow);

            evt.Uid = string.Format(UidFormat, match.Id);
            evt.Class = "PUBLIC";

            if (WithAlarms)
            {
                // first alarm
                evt.Alarms.Add(new Alarm()
                    {
                    // Note: "Duration" property does NOT mean the length of the alarm ringing
                    // but the time span before the event!
                    Trigger = new Trigger { Duration = Duration.FromTimeSpanExact(FirstAlarm) },
                    Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );

                // second alarm
                evt.Alarms.Add(new Alarm()
                    {
                    Trigger = new Trigger { Duration = Duration.FromTimeSpanExact(SecondAlarm) },
                    Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );
            }
            // Adding 'evt' to Events would cause double entries
        }
        return this;
    }

    public override string ToString()
    {
        return new CalendarSerializer().SerializeToString(_calendar)!;
    }

    /// <summary>
    /// Serializes the calendar to a file.
    /// </summary>
    /// <param name="filename">The name of the file to write the calendar to.</param>
    /// <param name="encoding">
    /// RFC5545 sect. 3.4.1: iCal default charset is UTF8.
    /// The encoding to use for the calendar text. If <c>null</c>, UTF-8 without BOM is used.
    /// Important: no Byte Order Mark (BOM) for Android, Google, Apple
    /// </param>
    public void Serialize(string filename, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(false);
        var ms = new MemoryStream();
        new CalendarSerializer().Serialize(_calendar, ms, encoding);
        ms.Seek(0, SeekOrigin.Begin);
        var fs = File.Create(filename);
        ms.CopyTo(fs);
        fs.Close();
    }

    /// <summary>
    /// Serializes the calendar to a <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The <see cref="Stream"/> to write the calendar to. The stream must be writable.</param>
    /// <param name="encoding">
    /// RFC5545 sect. 3.4.1: iCal default charset is UTF8.
    /// The encoding to use for the calendar text. If <c>null</c>, UTF-8 without BOM is used.
    /// Important: no Byte Order Mark (BOM) for Android, Google, Apple
    /// </param>
    public void Serialize(Stream stream, Encoding? encoding = null)
    {
        encoding ??= new UTF8Encoding(false);
        new CalendarSerializer().Serialize(_calendar, stream, encoding);
    }
}
