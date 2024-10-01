using System.Globalization;

namespace Axuno.Tools.DateAndTime;

/// <summary>
/// Interface.
/// </summary>
public interface IZonedTimeInfo
{
    /// <summary>
    /// Gets the <see cref="System.Globalization.CultureInfo"/> used for localization.
    /// </summary>
    CultureInfo CultureInfo { get; }

    /// <summary>
    /// Gets the IANA timezone ID related to the <see cref="System.DateTimeOffset"/>.
    /// </summary>
    string TimeZoneId { get; }

    /// <summary>
    /// Gets the <see cref="System.DateTimeOffset"/> which is set based on the timezone offset to UTC.
    /// </summary>
    DateTimeOffset DateTimeOffset { get; }

    /// <summary>
    /// Gets the generic name for the time zone.
    /// </summary>
    string GenericName { get; }

    /// <summary>
    /// Gets the generic abbreviation for the timezone.
    /// </summary>
    string GenericAbbreviation { get; }

    /// <summary>
    /// Gets the display name for the timezone, formatted like '(UTC-08:00) Pacific Time (USA &amp; Canada)'.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets the name of the timezone related to the <see cref="System.DateTimeOffset"/>.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the timezone abbreviation related to the <see cref="System.DateTimeOffset"/>.
    /// </summary>
    string Abbreviation { get; }

    /// <summary>
    /// Gets whether the timezone related to the <see cref="System.DateTimeOffset"/> is daylight saving time.
    /// </summary>
    bool IsDaylightSavingTime { get; }

    /// <summary>
    /// Gets the time difference between the current time zone's standard time and Coordinated Universal Time (UTC).
    /// </summary>
    TimeSpan BaseUtcOffset { get; }
}
