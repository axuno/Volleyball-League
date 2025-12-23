using System.Globalization;
using TimeZoneNames;

namespace Axuno.Tools.DateAndTime;
/// <summary>
/// Converts between <see cref="DateTime"/> or <see cref="DateTimeOffset"/> and zone specific <see cref="ZonedTime"/>.
/// </summary>
public class TimeZoneConverter
{
    // IANA timezone ID
    private readonly string _ianaTimeZoneId;
    private readonly CultureInfo _cultureInfo;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="ianaTimeZoneId">
    /// The IANA timezone ID to use for converting.
    /// We initialize with IANA because of compatibility with the NodaTime TimeZoneConverter we had before.
    /// </param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for converting. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <exception cref="TimeZoneNotFoundException"></exception>
    public TimeZoneConverter(string ianaTimeZoneId, CultureInfo? cultureInfo = null)
    {
        _ianaTimeZoneId = ianaTimeZoneId;
        _ = TimeZoneInfo.FindSystemTimeZoneById(ianaTimeZoneId);
        _cultureInfo = cultureInfo ?? CultureInfo.CurrentUICulture;
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
    /// <param name="dateTimeOfAnyKind"></param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for time zone localization. If <see langword="null"/>, the default culture will be used.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance or null, if the <paramref name="dateTimeOfAnyKind"/> parameter is null.</returns>
    public IZonedTimeInfo? ToZonedTime(DateTime? dateTimeOfAnyKind, CultureInfo? cultureInfo = null)
    {
        return ToZonedTime(dateTimeOfAnyKind, _ianaTimeZoneId, cultureInfo ?? _cultureInfo);
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
    /// <param name="dateTimeOfAnyKind"></param>
    /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for time zone localization. If <see langword="null"/>, the default culture will be used.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance.</returns>
    public IZonedTimeInfo? ToZonedTime(DateTime dateTimeOfAnyKind, CultureInfo? cultureInfo = null)
    {
        return ToZonedTime(dateTimeOfAnyKind, _ianaTimeZoneId, cultureInfo ?? _cultureInfo);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="zoneDateTime">A <see cref="DateTime"/> in the timezone specified with the timezone ID given when creating this converter instance.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> or null, if the <paramref name="zoneDateTime"/> parameter is null.</returns>
    public DateTime? ToUtc(DateTime? zoneDateTime)
    {
        return ToUtc(zoneDateTime, _ianaTimeZoneId);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="zoneDateTime">A <see cref="DateTime"/> in the timezone specified with the timezone ID given when creating this converter instance.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
    public DateTime ToUtc(DateTime zoneDateTime)
    {
        return ToUtc(zoneDateTime, _ianaTimeZoneId);
    }

    public static DateTime? ToUtc(DateTime? zoneDateTime, string timeZoneId)
    {
        if (!zoneDateTime.HasValue) return null;

        // Convert IANA time zone to Windows time zone ID
        var windowsTimeZoneId = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Convert the local time to UTC
        // We have to change the DateTimeKind to Unspecified, because the TimeZoneInfo.ConvertTimeToUtc method does not work with Local time
        // This makes the method compatible with the NodaTime TimeZoneConverter we had before.
        zoneDateTime = DateTime.SpecifyKind(zoneDateTime.Value, DateTimeKind.Unspecified);
        var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(zoneDateTime.Value, windowsTimeZoneId);

        return utcDateTime;
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
    /// </summary>
    /// <param name="zoneDateTime">A <see cref="DateTime"/> in the timezone specified with parameter <paramref name="timeZoneId"/>.</param>
    /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
    /// <exception cref="TimeZoneNotFoundException">If <paramref name="timeZoneId"/> is unknown.</exception>
    public static DateTime ToUtc(DateTime zoneDateTime, string timeZoneId)
    {
        return (DateTime) ToUtc((DateTime?) zoneDateTime, timeZoneId)!;
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
    /// <param name="dateTimeOfAnyKind"></param>
    /// <param name="timeZoneId"></param>
    /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance or null, if the <paramref name="dateTimeOfAnyKind"/> parameter is null.</returns>
    /// <exception cref="TimeZoneNotFoundException">If <paramref name="timeZoneId"/> is unknown.</exception>
    public static IZonedTimeInfo? ToZonedTime(DateTime? dateTimeOfAnyKind, string timeZoneId, CultureInfo? cultureInfo = null)
    {
        if (!dateTimeOfAnyKind.HasValue) return null;

        var utcDateTime = dateTimeOfAnyKind.Value.Kind switch
        {
            DateTimeKind.Utc => dateTimeOfAnyKind.Value,
            DateTimeKind.Local => TimeZoneInfo.ConvertTimeToUtc(dateTimeOfAnyKind.Value),
            _ => DateTime.SpecifyKind(dateTimeOfAnyKind.Value, DateTimeKind.Utc)
        };

        return ToZonedTime(new DateTimeOffset(utcDateTime), timeZoneId, cultureInfo);
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
    /// <param name="dateTimeOfAnyKind"></param>
    /// <param name="timeZoneId"></param>
    /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance.</returns>
    /// <exception cref="TimeZoneNotFoundException">If <paramref name="timeZoneId"/> is unknown.</exception>
    public static IZonedTimeInfo? ToZonedTime(DateTime dateTimeOfAnyKind, string timeZoneId, CultureInfo? cultureInfo = null)
    {
        return ToZonedTime((DateTime?) dateTimeOfAnyKind, timeZoneId, cultureInfo);
    }

    /// <summary>
    /// Converts a <see cref="Nullable{DateTimeOffset}"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
    /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <returns>Returns the converted <see cref="Nullable{DateTimeOffset}"/> as a <see cref="ZonedTime"/> instance  or null, if the <paramref name="dateTimeOffset"/> parameter is null.</returns>
    /// <exception cref="TimeZoneNotFoundException">If IANA <paramref name="timeZoneId"/> is unknown.</exception>
    public static IZonedTimeInfo? ToZonedTime(DateTimeOffset? dateTimeOffset, string timeZoneId, CultureInfo? cultureInfo = null)
    {
        if (!dateTimeOffset.HasValue) return null;

        var zonedDateTime = new ZonedTime();

        cultureInfo ??= CultureInfo.CurrentUICulture;

        // Convert IANA time zone to Windows time zone ID
        var windowsTimeZoneId = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        // Convert UTC to the specified time zone
        var localDateTime = TimeZoneInfo.ConvertTime(dateTimeOffset.Value.UtcDateTime, windowsTimeZoneId);

        zonedDateTime.CultureInfo = cultureInfo;
        zonedDateTime.DateTimeOffset = new(localDateTime, windowsTimeZoneId.GetUtcOffset(localDateTime));
        zonedDateTime.IsDaylightSavingTime = windowsTimeZoneId.IsDaylightSavingTime(localDateTime);
        zonedDateTime.DisplayName = windowsTimeZoneId.DisplayName;
        zonedDateTime.BaseUtcOffset = windowsTimeZoneId.BaseUtcOffset;
        zonedDateTime.TimeZoneId = timeZoneId;

        var tzNames = TZNames.GetNamesForTimeZone(timeZoneId, cultureInfo.TwoLetterISOLanguageName);
        zonedDateTime.GenericName = tzNames.Generic ?? string.Empty;
        zonedDateTime.Name = (zonedDateTime.IsDaylightSavingTime ? tzNames.Daylight : tzNames.Standard) ?? string.Empty;

        var tzAbbr = TZNames.GetAbbreviationsForTimeZone(timeZoneId, cultureInfo.TwoLetterISOLanguageName);
        zonedDateTime.GenericAbbreviation = tzAbbr.Generic ?? string.Empty;
        zonedDateTime.Abbreviation = (zonedDateTime.IsDaylightSavingTime ? tzAbbr.Daylight : tzAbbr.Standard) ?? string.Empty;

        return zonedDateTime;
    }

    /// <summary>
    /// Converts a <see cref="Nullable{DateTimeOffset}"/> to a <see cref="ZonedTime"/> instance for the timezone.
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
    /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
    /// <returns>Returns the converted <see cref="Nullable{DateTimeOffset}"/> as a <see cref="ZonedTime"/> instance.</returns>
    /// <exception cref="TimeZoneNotFoundException">If IANA <paramref name="timeZoneId"/> is unknown.</exception>
    public static IZonedTimeInfo? ToZonedTime(DateTimeOffset dateTimeOffset, string timeZoneId, CultureInfo? cultureInfo = null)
    {
        return ToZonedTime((DateTimeOffset?) dateTimeOffset, timeZoneId, cultureInfo);
    }

    /// <summary>
    /// Checks whether the Windows <see cref="TimeZoneInfo"/> can be mapped to a IANA timezone ID.
    /// </summary>
    /// <param name="timeZoneInfo"></param>
    /// <returns>Returns <c>true</c> if the <see cref="TimeZoneInfo"/> can be mapped to a IANA timezone, otherwise <c>false</c>.</returns>
    public static bool CanMapToIanaTimeZone(TimeZoneInfo timeZoneInfo)
    {
        return TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZoneInfo.Id, out _);
    }

    /// <summary>
    /// Get a collection of available <see cref="TimeZoneInfo.Id"/>s.
    /// </summary>
    /// <returns>Returns a collection of available <see cref="TimeZoneInfo.Id"/>s.</returns>
    public static IReadOnlyCollection<string> GetSystemTimeZoneList()
    {
        return TimeZoneInfo.GetSystemTimeZones().Select(tz => tz.Id).ToList().AsReadOnly();
    }

    /// <summary>
    /// Get a collection of IANA Ids for <see cref="TimeZoneInfo"/>s that can be converted to IANA.
    /// </summary>
    /// <returns>A collection of IANA Ids for <see cref="TimeZoneInfo"/>s that can be converted to IANA.</returns>
    public static IReadOnlyCollection<string> GetIanaTimeZoneList()
    {
        var ianaTimeZones = new List<string>();

        foreach (var timeZone in TimeZoneInfo.GetSystemTimeZones())
        {
            if (TimeZoneInfo.TryConvertWindowsIdToIanaId(timeZone.Id, out var ianaTimeZoneId))
            {
                ianaTimeZones.Add(ianaTimeZoneId);
            }
        }

        return ianaTimeZones.AsReadOnly();
    }
}
