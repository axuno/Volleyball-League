using System;
using System.Globalization;

namespace Axuno.Tools.DateAndTime;

/// <summary>
/// Keeps time and localized information about this time in a certain timezone.
/// </summary>
public class ZonedTime : IZonedTimeInfo
{
    internal ZonedTime()
    {
    }

    /// <summary>
    /// Gets the <see cref="CultureInfo"/> used for localization.
    /// </summary>
    public CultureInfo CultureInfo { get; internal set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Gets the IANA timezone ID related to the <see cref="DateTimeOffset"/>.
    /// </summary>
    public string TimeZoneId { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the <see cref="DateTimeOffset"/> which is set based on the timezone offset to UTC.
    /// </summary>
    public DateTimeOffset DateTimeOffset { get; internal set; }

    /// <summary>
    /// Gets the generic name for the time zone.
    /// </summary>
    public string GenericName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the generic abbreviation for the timezone.
    /// </summary>
    public string GenericAbbreviation { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the display name for the timezone, formatted like '(UTC-08:00) Pacific Time (USA &amp; Canada)'.
    /// </summary>
    public string DisplayName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the name of the timezone related to the <see cref="DateTimeOffset"/>.
    /// </summary>
    public string Name { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the timezone abbreviation related to the <see cref="DateTimeOffset"/>.
    /// </summary>
    public string Abbreviation { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets whether the timezone related to the <see cref="DateTimeOffset"/> is daylight saving time.
    /// </summary>
    public bool IsDaylightSavingTime { get; internal set; }

    /// <summary>
    /// Gets the base UTC offset of the timezone related to the <see cref="DateTimeOffset"/>.
    /// </summary>
    public TimeSpan BaseUtcOffset { get; internal set; }
}
