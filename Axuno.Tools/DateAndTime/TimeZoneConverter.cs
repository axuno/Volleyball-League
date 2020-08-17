using System;
using System.Collections.Generic;
using System.Globalization;
using NodaTime;
using NodaTime.TimeZones;
using TzConverter = TimeZoneConverter;

namespace Axuno.Tools.DateAndTime
{
    /// <summary>
    /// Converts from <see cref="DateTime"/> or <see cref="DateTimeOffset"/> and zone specific <see cref="ZonedTime"/>.
    /// </summary>
    /// <remarks>
    /// Credits to Joe Audette's blog at
    /// https://www.joeaudette.com/blog/2016/06/23/cross-platform-timezone-handling-for-aspnet-core
    /// </remarks>
    public class TimeZoneConverter
    {
        private readonly IDateTimeZoneProvider _dateTimeZoneProvider;
        private readonly string _timeZoneId;
        private readonly CultureInfo _cultureInfo;
        private readonly ZoneLocalMappingResolver _resolver;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="dateTimeZoneProvider"></param>
        /// <param name="timeZoneInfo">The Windows <see cref="TimeZoneInfo"/> to use for converting.</param>
        /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for converting.</param>
        /// <param name="resolver">The <see cref="ZoneLocalMappingResolver"/> to use for converting.</param>
        public TimeZoneConverter(IDateTimeZoneProvider dateTimeZoneProvider, TimeZoneInfo timeZoneInfo,
            CultureInfo cultureInfo = null, ZoneLocalMappingResolver resolver = null) : this(dateTimeZoneProvider,
            TzConverter.TZConvert.WindowsToIana(timeZoneInfo.Id), cultureInfo, resolver)
        {}

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="dateTimeZoneProvider"></param>
        /// <param name="ianaTimeZoneId">The IANA timezone ID to use for converting.</param>
        /// <param name="cultureInfo">The <see cref="CultureInfo"/> to use for converting.</param>
        /// <param name="resolver">The <see cref="ZoneLocalMappingResolver"/> to use for converting.</param>
        public TimeZoneConverter(IDateTimeZoneProvider dateTimeZoneProvider, string ianaTimeZoneId,
            CultureInfo cultureInfo = null, ZoneLocalMappingResolver resolver = null)
        {
            _dateTimeZoneProvider = dateTimeZoneProvider;
            _timeZoneId = ianaTimeZoneId;
            _cultureInfo = cultureInfo;
            _resolver = resolver;
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
        /// <param name="dateTimeOfAnyKind"></param>
        /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance or null, if the <see cref="dateTimeOfAnyKind"/> parameter is null.</returns>
        public ZonedTime ToZonedTime(DateTime? dateTimeOfAnyKind)
        {
            return ToZonedTime(dateTimeOfAnyKind, _timeZoneId, _cultureInfo, _dateTimeZoneProvider);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
        /// <param name="dateTimeOfAnyKind"></param>
        /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance.</returns>
        public ZonedTime ToZonedTime(DateTime dateTimeOfAnyKind)
        {
            return ToZonedTime(dateTimeOfAnyKind, _timeZoneId, _cultureInfo, _dateTimeZoneProvider);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime">A <see cref="DateTime"/> in the the timezone specified with the timezone ID given when creating this converter instance.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> or null, if the <see cref="zoneDateTime"/> parameter is null.</returns>
        public DateTime? ToUtc(DateTime? zoneDateTime)
        {
            return ToUtc(zoneDateTime, _timeZoneId, _dateTimeZoneProvider, _resolver);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime">A <see cref="DateTime"/> in the the timezone specified with the timezone ID given when creating this converter instance.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
        public DateTime ToUtc(DateTime zoneDateTime)
        {
            return ToUtc(zoneDateTime, _timeZoneId, _dateTimeZoneProvider, _resolver);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime">A <see cref="DateTime"/> in the timezone specified with parameter <see cref="timeZoneId"/>.</param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> or null, if the <see cref="zoneDateTime"/> parameter is null.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public DateTime? ToUtc(DateTime? zoneDateTime, string timeZoneId)
        {
            return ToUtc(zoneDateTime, timeZoneId, _dateTimeZoneProvider, _resolver);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime">A <see cref="DateTime"/> in the timezone specified with parameter <see cref="timeZoneId"/>.</param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public DateTime ToUtc(DateTime zoneDateTime, string timeZoneId)
        {
            return ToUtc(zoneDateTime, timeZoneId, _dateTimeZoneProvider, _resolver);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
        /// <param name="dateTimeOfAnyKind"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
        /// <param name="timeZoneProvider">The <see cref="IDateTimeZoneProvider"/> to use. For performance use a <see cref="DateTimeZoneCache"/>.
        /// <example>
        /// IDateTimeZoneProvider dtzp = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default)
        /// </example>
        /// </param>
        /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance or null, if the <see cref="dateTimeOfAnyKind"/> parameter is null.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public static ZonedTime ToZonedTime(DateTime? dateTimeOfAnyKind, string timeZoneId,
            CultureInfo cultureInfo = null, IDateTimeZoneProvider timeZoneProvider = null)
        {
            if (!dateTimeOfAnyKind.HasValue) return null;

            DateTime utcDateTime;
            switch (dateTimeOfAnyKind.Value.Kind)
            {
                case DateTimeKind.Utc:
                    utcDateTime = dateTimeOfAnyKind.Value;
                    break;
                case DateTimeKind.Local:
                    utcDateTime = dateTimeOfAnyKind.Value.ToUniversalTime();
                    break;
                default: //DateTimeKind.Unspecified
                    utcDateTime = DateTime.SpecifyKind(dateTimeOfAnyKind.Value, DateTimeKind.Utc);
                    break;
            }

            return ToZonedTime(new DateTimeOffset(utcDateTime), timeZoneId, cultureInfo, timeZoneProvider);
        }

        /// <summary>
        /// Converts a <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <remarks><see cref="DateTimeKind.Unspecified"/> will be treated like <see cref="DateTimeKind.Utc"/></remarks>
        /// <param name="dateTimeOfAnyKind"></param>
        /// <param name="timeZoneId"></param>
        /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
        /// <param name="timeZoneProvider">The <see cref="IDateTimeZoneProvider"/> to use. For performance use a <see cref="DateTimeZoneCache"/>.
        /// <example>
        /// IDateTimeZoneProvider dtzp = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default)
        /// </example>
        /// </param>
        /// <returns>Returns the converted <see cref="DateTime"/> as a <see cref="ZonedTime"/> instance.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public static ZonedTime ToZonedTime(DateTime dateTimeOfAnyKind, string timeZoneId,
            CultureInfo cultureInfo = null, IDateTimeZoneProvider timeZoneProvider = null)
        {
            return ToZonedTime((DateTime?) dateTimeOfAnyKind, timeZoneId, cultureInfo, timeZoneProvider);
        }

        /// <summary>
        /// Converts a <see cref="Nullable{DateTimeOffset}"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
        /// <param name="timeZoneProvider">The <see cref="IDateTimeZoneProvider"/> to use. For performance use a <see cref="DateTimeZoneCache"/>.
        /// <example>
        /// IDateTimeZoneProvider dtzp = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default)
        /// </example>
        /// </param>
        /// <returns>Returns the converted <see cref="Nullable{DateTimeOffset}"/> as a <see cref="ZonedTime"/> instance  or null, if the <see cref="dateTimeOffset"/> parameter is null.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If IANA <see cref="timeZoneId"/> is unknown.</exception>
        public static ZonedTime ToZonedTime(DateTimeOffset? dateTimeOffset, string timeZoneId,
            CultureInfo cultureInfo = null, IDateTimeZoneProvider timeZoneProvider = null)
        {
            if (!dateTimeOffset.HasValue) return null;

            var zonedDateTime = new ZonedTime();

            timeZoneProvider ??= DateTimeZoneProviders.Tzdb;
            cultureInfo ??= CultureInfo.CurrentUICulture;

            // throws if timeZoneId is unknown
            var timeZone = timeZoneProvider[timeZoneId];

            var instantInZone = Instant.FromDateTimeUtc(dateTimeOffset.Value.UtcDateTime).InZone(timeZone);
            zonedDateTime.CultureInfo = cultureInfo;
            zonedDateTime.DateTimeOffset = instantInZone.ToDateTimeOffset();
            zonedDateTime.IsDaylightSavingTime = instantInZone.IsDaylightSavingTime();

            var timeZoneInfo = TzConverter.TZConvert.GetTimeZoneInfo(timeZoneId);
            zonedDateTime.DisplayName = timeZoneInfo.DisplayName;
            zonedDateTime.BaseUtcOffset = timeZoneInfo.BaseUtcOffset;
            zonedDateTime.TimeZoneId = timeZoneId;

            var tzNames =
                TimeZoneNames.TZNames.GetNamesForTimeZone(timeZone.Id, cultureInfo.TwoLetterISOLanguageName);
            zonedDateTime.GenericName = tzNames.Generic;
            zonedDateTime.Name = zonedDateTime.IsDaylightSavingTime ? tzNames.Daylight : tzNames.Standard;

            var tzAbbr =
                TimeZoneNames.TZNames.GetAbbreviationsForTimeZone(timeZone.Id,
                    cultureInfo.TwoLetterISOLanguageName);
            zonedDateTime.GenericAbbreviation = tzAbbr.Generic;
            zonedDateTime.Abbreviation = zonedDateTime.IsDaylightSavingTime ? tzAbbr.Daylight : tzAbbr.Standard;

            return zonedDateTime;
        }

        /// <summary>
        /// Converts a <see cref="Nullable{DateTimeOffset}"/> to a <see cref="ZonedTime"/> instance for the timezone.
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <param name="cultureInfo">The see <see cref="CultureInfo"/> to use for localizing timezone strings. Default is <see cref="CultureInfo.CurrentUICulture"/>.</param>
        /// <param name="timeZoneProvider">The <see cref="IDateTimeZoneProvider"/> to use. For performance use a <see cref="DateTimeZoneCache"/>.
        /// <example>
        /// IDateTimeZoneProvider dtzp = new DateTimeZoneCache(TzdbDateTimeZoneSource.Default)
        /// </example>
        /// </param>
        /// <returns>Returns the converted <see cref="Nullable{DateTimeOffset}"/> as a <see cref="ZonedTime"/> instance.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If IANA <see cref="timeZoneId"/> is unknown.</exception>
        public static ZonedTime ToZonedTime(DateTimeOffset dateTimeOffset, string timeZoneId,
            CultureInfo cultureInfo = null, IDateTimeZoneProvider timeZoneProvider = null)
        {
            return ToZonedTime((DateTimeOffset?) dateTimeOffset, timeZoneId, cultureInfo, timeZoneProvider);
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime"></param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <param name="timeZoneProvider"></param>
        /// <param name="resolver">The <see cref="ZoneLocalMappingResolver"/> to use. Default is <see cref="Resolvers.LenientResolver"/>´, which never throws an exception due to ambiguity or skipped time.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/> or null, if the <see cref="zoneDateTime"/> parameter is null.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public static DateTime? ToUtc(DateTime? zoneDateTime, string timeZoneId,
            IDateTimeZoneProvider timeZoneProvider, ZoneLocalMappingResolver resolver = null)
        {
            if (!zoneDateTime.HasValue) return null;

            timeZoneProvider ??= DateTimeZoneProviders.Tzdb;
            // never throws an exception due to ambiguity or skipped time:
            resolver ??= Resolvers.LenientResolver;
            // throws if timeZoneId is unknown
            var timeZone = timeZoneProvider[timeZoneId];

            var local = LocalDateTime.FromDateTime(zoneDateTime.Value);
            var zonedTime = timeZone.ResolveLocal(local, resolver);
            return zonedTime.ToDateTimeUtc();
        }

        /// <summary>
        /// Converts the <see cref="DateTime"/> of any <see cref="DateTimeKind"/> to a <see cref="DateTime"/> of <see cref="DateTimeKind.Utc"/>.
        /// </summary>
        /// <param name="zoneDateTime"></param>
        /// <param name="timeZoneId">The ID of the IANA timezone database, https://en.wikipedia.org/wiki/List_of_tz_database_time_zones.</param>
        /// <param name="timeZoneProvider"></param>
        /// <param name="resolver">The <see cref="ZoneLocalMappingResolver"/> to use. Default is <see cref="Resolvers.LenientResolver"/>´, which never throws an exception due to ambiguity or skipped time.</param>
        /// <returns>Returns the converted <see cref="DateTime"/> with <see cref="DateTimeKind.Utc"/>.</returns>
        /// <exception cref="DateTimeZoneNotFoundException">If <see cref="timeZoneId"/> is unknown.</exception>
        public static DateTime ToUtc(DateTime zoneDateTime, string timeZoneId,
            IDateTimeZoneProvider timeZoneProvider, ZoneLocalMappingResolver resolver = null)
        {
            return ToUtc((DateTime?)zoneDateTime, timeZoneId, timeZoneProvider, resolver).Value;
        }
        
        /// <summary>
        /// Checks whether the Windows <see cref="TimeZoneInfo"/> can be mapped to a IANA timezone ID.
        /// </summary>
        /// <param name="timeZoneInfo"></param>
        /// <returns>Returns <c>true</c> if the <see cref="TimeZoneInfo"/> can be mapped to a IANA timezone, otherwise <c>false</c>.</returns>
        public static bool CanMapToIanaTimeZone(TimeZoneInfo timeZoneInfo)
        {
            return TzConverter.TZConvert.TryWindowsToIana(timeZoneInfo.Id, out var ianaTimeZoneName);
        }

        /// <summary>
        /// Get a collection of available <see cref="IDateTimeZoneProvider.Ids"/>.
        /// </summary>
        /// <param name="timeZoneProvider">The <see cref="IDateTimeZoneProvider"/> to use. Default is <see cref="DateTimeZoneProviders.Tzdb"/>.</param>
        /// <returns>Returns a collection of available <see cref="IDateTimeZoneProvider.Ids"/>.</returns>
        public static IReadOnlyCollection<string> GetTimeZoneList(IDateTimeZoneProvider timeZoneProvider = null)
        {
            timeZoneProvider ??= DateTimeZoneProviders.Tzdb;
            return timeZoneProvider.Ids;
        }
    }
}
