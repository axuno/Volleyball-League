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
        _calendar.ProductId = "TournamentManager/Ical.Net";

        //Organizer = new Organizer() { CommonName = "TournamentManager", Value = new Uri("mailto:noreply@tournamentmanager")};

        // Does not work well:
        //_calendar.AddTimeZone(new VTimeZone("Europe/Berlin")); // TzId string NodaTime is aware of

        FirstAlarm = new TimeSpan(-7, 0, 0, 0, 0);
        SecondAlarm = new TimeSpan(-1, 0, 0, 0, 0);
    }

    protected string ProductId
    {
        set
        {
            _calendar.ProductId = value;
        }

        get
        {
            return _calendar.ProductId;
        }
    }

    /*
    public Organizer Organizer
    { get; set; }
    */

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
    /// <returns>Returns a <see cref="Ical.Net.Calendar"/> instance.</returns>
    public Calendar CreateEvents(List<CalendarRow> matches)
    {
        //_calendar.Method = "PUBLISH";     // Matches.RealStart.HasValue ? "CANCEL" : "REQUEST";
        if (matches.Count > 1)
        {
            _calendar.AddProperty("NAME", Name);
            _calendar.AddProperty("X-WR-CALNAME", Name);
        }
            
        foreach (var match in matches)
        {
            var evt = _calendar.Create<CalendarEvent>();
                
            //evt.Organizer = Organizer;
            evt.Summary = Summary;
            evt.Location = $"{match.VenueName}, {match.VenueStreet}, {match.VenuePostalCode} {match.VenueCity}";
            evt.Description = !string.IsNullOrWhiteSpace(DescriptionOpponentsFormat)
                ? string.Format(DescriptionOpponentsFormat, match.HomeTeamNameForRound, match.GuestTeamNameForRound)
                : string.Empty;
            if (match.VenueLongitude.HasValue && match.VenueLatitude.HasValue && !string.IsNullOrWhiteSpace(DescriptionGoogleMapsFormat))
                evt.Description += "\n" + string.Format(DescriptionGoogleMapsFormat,
                    match.VenueLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    match.VenueLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            evt.Description += "\n" + DescriptionFooter;
            evt.Sequence = (int)match.ChangeSerial;
            evt.Start = new CalDateTime(value: match.PlannedStart ?? throw new InvalidOperationException($"{nameof(match.PlannedStart)} must not be null"));
            evt.End = new CalDateTime(value: match.PlannedEnd ?? throw new InvalidOperationException($"{nameof(match.PlannedEnd)} must not be null"));
            // evt.Created = new CalDateTime(match.ModifiedOn);
            evt.LastModified = new CalDateTime(match.ModifiedOn);
            evt.DtStamp = new CalDateTime(DateTime.Now);

            evt.IsAllDay = false;
            evt.Uid = string.Format(UidFormat, match.Id);
            evt.Status = EventStatus.Confirmed;
            evt.Class = "PRIVATE";
            evt.Transparency = "OPAQUE";
            evt.AddProperty("X-MICROSOFT-CDO-BUSYSTATUS", "BUSY");
               
            if (WithAlarms)
            {
                // first alarm
                evt.Alarms.Add(new Alarm()
                    {
                        // Note: "Duration" property does NOT mean the length of the alarm ringing
                        // but the TimeSpan before the event!
                        Trigger = new Trigger(FirstAlarm),
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );

                // second alarm
                evt.Alarms.Add(new Alarm()
                    {
                        Trigger = new Trigger(SecondAlarm),
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );
            }
            // _iCal.Events.Add(evt);  // this would double entries!!
        }
        return this;
    }

    /// <summary>
    /// Creates a public calendar with events for the matches.
    /// </summary>
    /// <param name="matches">The matches to use for the calendar events.</param>
    /// <returns>Returns a <see cref="Ical.Net.Calendar"/> instance.</returns>
    public Calendar CreatePublicCalendar(List<CalendarRow> matches)
    {
        _calendar.Method = "PUBLISH";

        _calendar.AddProperty("NAME", Name);
        _calendar.AddProperty("X-WR-CALNAME", Name);
        _calendar.AddProperty("REFRESH-INTERVAL;VALUE=DURATION", "P24H");

        foreach (var match in matches)
        {
            var evt = _calendar.Create<CalendarEvent>();

            evt.Summary = Summary;
            evt.Location = $"{match.VenueName}, {match.VenueStreet}, {match.VenuePostalCode} {match.VenueCity}";
            evt.Description = !string.IsNullOrWhiteSpace(DescriptionOpponentsFormat)
                ? string.Format(DescriptionOpponentsFormat, match.HomeTeamNameForRound, match.GuestTeamNameForRound)
                : string.Empty;
            if (match.VenueLongitude.HasValue && match.VenueLatitude.HasValue && !string.IsNullOrWhiteSpace(DescriptionGoogleMapsFormat))
                evt.Description += "\n" + string.Format(DescriptionGoogleMapsFormat,
                    match.VenueLatitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    match.VenueLongitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            evt.Description += "\n" + DescriptionFooter;
            evt.Sequence = (int)match.ChangeSerial;
            evt.Start = new CalDateTime(value: match.PlannedStart ?? throw new InvalidOperationException($"{nameof(match.PlannedStart)} must not be null"));
            evt.End = new CalDateTime(value: match.PlannedEnd ?? throw new InvalidOperationException($"{nameof(match.PlannedEnd)} must not be null"));
            // evt.Created = new CalDateTime(match.ModifiedOn);
            evt.LastModified = new CalDateTime(match.ModifiedOn);
            evt.DtStamp = new CalDateTime(DateTime.Now);

            evt.IsAllDay = false;
            evt.Uid = string.Format(UidFormat, match.Id);
            evt.Class = "PUBLIC";

            if (WithAlarms)
            {
                // first alarm
                evt.Alarms.Add(new Alarm()
                    {
                        // Note: "Duration" property does NOT mean the length of the alarm ringing
                        // but the TimeSpan before the event!
                        Trigger = new Trigger(FirstAlarm),
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );

                // second alarm
                evt.Alarms.Add(new Alarm()
                    {
                        Trigger = new Trigger(SecondAlarm),
                        Action = AlarmAction.Display,
                        Summary = evt.Summary
                    }
                );
            }
            // _iCal.Events.Add(evt);  // this would add double entries
        }
        return this;
    }

    public override string ToString()
    {
        var serializer = new CalendarSerializer(new SerializationContext());
        return serializer.SerializeToString(_calendar);
    }

    public void Serialize(string filename, Encoding encoding)
    {
        var serializer = new CalendarSerializer(new SerializationContext());
        var ms = new MemoryStream();
        serializer.Serialize(_calendar, ms, encoding);
        ms.Seek(0, SeekOrigin.Begin);
        var fs = File.Create(filename);
        ms.CopyTo(fs);
        fs.Close();
    }

    public void Serialize(Stream stream, Encoding encoding)
    {
        var serializer = new CalendarSerializer(new SerializationContext());
        serializer.Serialize(_calendar, stream, encoding);
    }
}