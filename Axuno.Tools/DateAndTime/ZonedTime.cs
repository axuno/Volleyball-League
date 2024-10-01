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

    /// <inheritdoc />
    public CultureInfo CultureInfo { get; internal set; } = CultureInfo.InvariantCulture;

    /// <inheritdoc />
    public string TimeZoneId { get; internal set; } = string.Empty;

    /// <inheritdoc />
    public DateTimeOffset DateTimeOffset { get; internal set; }

    /// <inheritdoc/>
    public string GenericName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the generic abbreviation for the timezone.
    /// </summary>
    public string GenericAbbreviation { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets the display name for the timezone, formatted like '(UTC-08:00) Pacific Time (USA &amp; Canada)'.
    /// </summary>
    public string DisplayName { get; internal set; } = string.Empty;

    /// <inheritdoc/>
    public string Name { get; internal set; } = string.Empty;

    /// <inheritdoc/>
    public string Abbreviation { get; internal set; } = string.Empty;

    /// <inheritdoc/>
    public bool IsDaylightSavingTime { get; internal set; }

    /// <inheritdoc/>
    public TimeSpan BaseUtcOffset { get; internal set; }
}
