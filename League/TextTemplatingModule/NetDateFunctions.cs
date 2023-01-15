using System;
using System.Globalization;
using System.Threading.Tasks;
using Scriban;
using Scriban.Runtime;
using Scriban.Syntax;
#nullable enable
#pragma warning disable IDE0060 // Remove unused parameter
namespace League.TextTemplatingModule;

public class NetDateTimeFunctions : ScriptObject, IScriptCustomFunction
{
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter? _timeZoneConverter;
        
    [ScriptMemberIgnore]
    public static readonly ScriptVariable DateVariable = new ScriptVariableGlobal("ndate");

    /// <summary>
    /// Initializes a new instance of the <see cref="NetDateTimeFunctions"/> class.
    /// </summary>
    public NetDateTimeFunctions(Axuno.Tools.DateAndTime.TimeZoneConverter? timeZoneConverter)
    {
        _timeZoneConverter = timeZoneConverter;
        CreateImportFunctions();
    }

    /// <summary>
    /// Converts a given <see cref="DateTime"/> into a formatted string.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="date">The <see cref="DateTime"/> to format.</param>
    /// <param name="pattern">The pattern to use. See https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings for details.</param>
    /// <param name="culture">The culture name to use date, time, currency formatting.</param>
    /// <returns>Returns the given <see cref="DateTime"/> converted into a formatted string.</returns>

    public static string Format(TemplateContext context, DateTime? date, string pattern, string? culture = null)
    {
        var cultureInfo = culture is not null ? CultureInfo.GetCultureInfo(culture) : CultureInfo.CurrentUICulture;
        return date is null ? string.Empty : ToString(date.Value, pattern, cultureInfo);
    }

    /// <summary>
    /// Returns the short date string for a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// Scriban: {{ ndate.to_short_date_string 'en' }}
    /// Result:  12/24/2020
    /// </remarks>
    public static string ToShortDateString(TemplateContext context, DateTime date, string? culture = null)
    {
        return date.ToString("d", culture is null ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(culture));
    }

    /// <summary>
    /// Returns the long date string for a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// Scriban: {{ ndate.to_long_date_string 'en' }}
    /// Result:  Thursday, December 24, 2020
    /// </remarks>
    public static string ToLongDateString(TemplateContext context, DateTime date, string? culture = null)
    {
        return date.ToString("D", culture is null ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(culture));
    }

    /// <summary>
    /// Returns the long time string for a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// Scriban: {{ ndate.to_long_time_string 'en' }}
    /// Result:  1:45 PM
    /// </remarks>
    public static string ToShortTimeString(TemplateContext context, DateTime date, string? culture = null)
    {
        return date.ToString("t", culture is null ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(culture));
    }

    /// <summary>
    /// Returns the long time string for a <see cref="DateTime"/>.
    /// </summary>
    /// <remarks>
    /// Scriban: {{ ndate.to_long_time_string 'en' }}
    /// Result:  1:45:30 PM
    /// </remarks>
    public static string ToLongTimeString(TemplateContext context, DateTime date, string? culture = null)
    {
        return date.ToString("T", culture is null ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(culture));
    }

    private static string ToString(DateTime date, string? pattern, IFormatProvider cultureInfo)
    {
        return string.IsNullOrWhiteSpace(pattern)
            ? date.ToString(cultureInfo)
            : date.ToString(pattern, cultureInfo);
    }

    /// <summary>
    /// Returns a <see cref="DateTime"/> object of the current time, including the hour, minutes, seconds and milliseconds.
    /// </summary>
    /// <remarks>
    /// Scriban:  {{ ndate.now.year }}
    /// Result:   2020
    /// </remarks>
    public static DateTime Now()
    {
        return DateTime.Now;
    }

    /// <summary>
    /// Adds the specified number of days to the input date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="days">The days.</param>
    /// <returns>A new date</returns>
    /// <remarks>
    /// Scriban:  {{ ndate.parse '2016/01/05' | date.add_days 1 }}
    /// Result:   06 Jan 2016
    /// </remarks>
    public static DateTime AddDays(DateTime date, double days)
    {
        return date.AddDays(days);
    }

    /// <summary>
    /// Adds the specified number of months to the input date.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="months">The months.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    /// <remarks>
    /// Scriban: {{ ndate.parse '2016/01/05' | date.add_months 1 }}
    /// Result:  05 Feb 2016
    /// </remarks>
    public static DateTime AddMonths(DateTime date, int months)
    {
        return date.AddMonths(months);
    }

    /// <summary>
    /// Adds the specified number of years to the input <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="years">The years.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    /// <remarks>
    /// Scriban: {{ ndate.parse '2016/01/05' | date.add_years 1 }}
    /// Result:  05 Jan 2017
    /// </remarks>
    public static DateTime AddYears(DateTime date, int years)
    {
        return date.AddYears(years);
    }

    /// <summary>
    /// Adds the specified number of hours to the input <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="hours">The hours.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    public static DateTime AddHours(DateTime date, double hours)
    {
        return date.AddHours(hours);
    }

    /// <summary>
    /// Adds the specified number of minutes to the input <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="minutes">The minutes.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    public static DateTime AddMinutes(DateTime date, double minutes)
    {
        return date.AddMinutes(minutes);
    }

    /// <summary>
    /// Adds the specified number of seconds to the input <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="seconds">The seconds.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    public static DateTime AddSeconds(DateTime date, double seconds)
    {
        return date.AddSeconds(seconds);
    }

    /// <summary>
    /// Adds the specified number of milliseconds to the input <see cref="DateTime"/>.
    /// </summary>
    /// <param name="date">The date.</param>
    /// <param name="millis">The milliseconds.</param>
    /// <returns>A new <see cref="DateTime"/></returns>
    public static DateTime AddMilliseconds(DateTime date, double millis)
    {
        return date.AddMilliseconds(millis);
    }

    /// <summary>
    /// Parses the specified input string to a date <see cref="DateTime"/>.
    /// </summary>
    /// <param name="context">The template context.</param>
    /// <param name="text">A text representing a date.</param>
    /// <returns>A <see cref="DateTime"/> object</returns>
    /// <remarks>
    /// Scriban: {{ ndate.parse '2016/01/05' }}
    /// Result:  05 Jan 2016
    /// </remarks>
    public static DateTime? Parse(TemplateContext context, string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }
        if (DateTime.TryParse(text, CultureInfo.CurrentUICulture, DateTimeStyles.None, out var result))
        {
            return result;
        }
        return result;
    }

    /// <summary>
    /// Clones the context of this object.
    /// </summary>
    /// <param name="deep"></param>
    /// <returns>Returns a cloned <see cref="IScriptObject"/>.</returns>
    public override IScriptObject Clone(bool deep)
    {
        var dateFunctions = (NetDateTimeFunctions)base.Clone(deep);
        // It important to call the CreateImportFunctions as it is instance specific (using ToString for the object)
        dateFunctions.CreateImportFunctions();
        return dateFunctions;
    }

    /// <summary>
    /// Call the custom function object.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="callerContext"></param>
    /// <param name="arguments"></param>
    /// <param name="blockStatement"></param>
    /// <returns>Returns the result of the custom function.</returns>
    public virtual object? Invoke(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        // If we access 'ndate' without any parameter, it calls the 'parse' function by default 
        // otherwise it is the 'date' object itself
        return arguments.Count switch
        {
            0 => this,
            1 => Parse(context, context.ObjectToString(arguments[0])),
            _ => throw new ScriptRuntimeException(callerContext.Span,
                $"Invalid number of parameters `{arguments.Count}` for `{DateVariable.Name}` object/function.")
        };
    }

    /// <summary>
    /// Call the custom function object asynchronously.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="callerContext"></param>
    /// <param name="arguments"></param>
    /// <param name="blockStatement"></param>
    /// <returns>Returns the result of the custom function.</returns>
    ValueTask<object?> IScriptCustomFunction.InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments,
        ScriptBlockStatement blockStatement)
    {
        return InvokeAsync(context, callerContext, arguments, blockStatement);
    }

    public int RequiredParameterCount => 0;

    public int ParameterCount => 0;

    public ScriptVarParamKind VarParamKind => ScriptVarParamKind.Direct;

    public Type ReturnType => typeof(object);

    public ScriptParameterInfo GetParameterInfo(int index)
    {
        return new ScriptParameterInfo(typeof(object), "parameter" + index);
    }

    private ValueTask<object?> InvokeAsync(TemplateContext context, ScriptNode callerContext, ScriptArray arguments, ScriptBlockStatement blockStatement)
    {
        return new ValueTask<object?>(Invoke(context, callerContext, arguments, blockStatement));
    }

    private void CreateImportFunctions()
    {
        /*  Note:
         *  For optional arguments we cannot work with Func<T, ...>
         *  Instead we need a separate method, where optional arguments have a default value.
         */

        // Convert a given DateTime to a DateTime in the defined time zone
        this.Import("to_zoned_time", new Func<TemplateContext, DateTime?, DateTime?>(ToZonedTime));
        // Convert a DateTime of the defined time zone to UTC DateTime
        this.Import("to_utc", new Func<TemplateContext, DateTime?, DateTime?>(ToUtc));
        this.Import("tz_abbr", new Func<DateTime?, string?, string?>(GetTimeZoneAbbreviation));

    }

    private DateTime? ToZonedTime(TemplateContext ctx, DateTime? date)
    {
        return _timeZoneConverter?.ToZonedTime(date)?.DateTimeOffset.DateTime;
    }

    private DateTime? ToUtc(TemplateContext ctx, DateTime? date)
    {
        return _timeZoneConverter?.ToUtc(date);
    }

    private string? GetTimeZoneAbbreviation(DateTime? date, string? culture = null)
    {
        // use a separate method to allow for optional arguments
        return _timeZoneConverter?.ToZonedTime(date, string.IsNullOrWhiteSpace(culture) ? CultureInfo.CurrentUICulture : CultureInfo.GetCultureInfo(culture))?.Abbreviation;
    }
}
#pragma warning restore IDE0060 // Remove unused parameter
