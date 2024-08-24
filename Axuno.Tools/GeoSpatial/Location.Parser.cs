using System.Globalization;
using System.Text.RegularExpressions;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Represents a Latitude/Longitude/Altitude coordinate.
/// </summary>
public partial class Location
{
    // Default timeout value for the regular expressions
    private static int _regExTimeout = 1000;

    /// <summary>Parsing latitude and longitude information.</summary>
    /// <example>
    /// User input:
    /// 12° 34′ 56″ S This uses the ISO 31-1 symbols. 
    /// -12° 34′ 56″  This uses a negative sign instead of 'S' 
    /// -12°34′56″S   This uses a negative sign and an 'S' and omits the whitespace. We'll assume the coordinate is in the southern hemisphere. 
    /// -12 34" 56'   This omits the degree symbol and uses quotation marks. This is probably the easiest to type as it doesn't use any special symbols not found on a normal keyboard. 
    /// -12 34’ 56”   Same as above but uses smart quotes (think copying from Microsoft Word). 
    /// +12 34 56 S   We can assume this is DMS format as there are three groups of numbers. 
    /// S 12d34m56s   Some programs allow D for degree, M for minute and S for second, with the North/South suffix at the beginning. 
    /// S 12* 34' 56" This is often seen in legal descriptions. 
    /// </example>
    private static class Parser
    {
        private const string DegreePattern = @"
^\s*                 # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?     # Optional suffix
(?<latDeg>.+?)       # Match anything and we'll try to parse it later
[D\*\u00B0]?\s*      # Degree symbol ([D|*|°] optional) followed by optional whitespace
(?<latSuf>[NS])?\s+  # Suffix could also be here. Need some whitespace to separate

(?<lonSuf>[EW])?     # Now try the longitude
(?<lonDeg>.+?)       # Degrees
[D\*\u00B0]?\s*      # Degree symbol + whitespace
(?<lonSuf>[EW])?     # Optional suffix
\s*$                 # Match the end of the string (ignoring whitespace)";

        private const string DegreeMinutePattern = @"
^\s*                 # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?     # Optional suffix
(?<latDeg>.+?)       # Match anything
[D\*\u00B0\s]        # Degree symbol or whitespace
(?<latMin>.+?)       # Now look for minutes
[M'\u2032\u2019]?\s* # Minute symbol [single quote, prime, smart quote, M] + whitespace
(?<latSuf>[NS])?\s+  # Optional suffix + whitespace

(?<lonSuf>[EW])?      # Now try the longitude
(?<lonDeg>.+?)        # Degrees
[D\*\u00B0?\s]        # Degree symbol or whitespace
(?<lonMin>.+?)        # Minutes
[M'\u2032\u2019]?\s*  # Minute symbol
(?<lonSuf>[EW])?      # Optional suffix
\s*$                  # Match the end of the string (ignoring whitespace)";

        private const string DegreeMinuteSecondPattern = @"
^\s*                  # Ignore any whitespace at the start of the string
(?<latSuf>[NS])?      # Optional suffix
(?<latDeg>.+?)        # Match anything
[D\*\u00B0\s]         # Degree symbol/whitespace
(?<latMin>.+?)        # Now look for minutes
[M'\u2032\u2019\s]    # Minute symbol/whitespace
(?<latSec>.+?)        # Look for seconds
[""\u2033\u201D]?\s*  # Second symbol [double quote (c# escaped), double prime or smart doube quote] + whitespace
(?<latSuf>[NS])?\s+   # Optional suffix + whitespace

(?<lonSuf>[EW])?      # Now try the longitude
(?<lonDeg>.+?)        # Degrees
[D\*\u00B0\s]         # Degree symbol/whitespace
(?<lonMin>.+?)        # Minutes
[M'\u2032\u2019\s]    # Minute symbol/whitespace
(?<lonSec>.+?)        # Seconds
[""\u2033\u201D]?\s*  # Second symbol
(?<lonSuf>[EW])?      # Optional suffix
\s*$                  # Match the end of the string (ignoring whitespace)";

        private const string IsoPattern = @"
^\s*                                        # Match the start of the string, ignoring any whitespace
(?<latitude> [+-][0-9]{2,6}(?: \. [0-9]+)?) # The decimal digits and punctuation are strictly defined
(?<longitude>[+-][0-9]{3,7}(?: \. [0-9]+)?) # in the standard. The decimal part is optional.
(?<altitude> [+-][0-9]+(?: \. [0-9]+)?)?    # The altitude component is optional
/                                           # The string must be terminated by '/'";

        private const RegexOptions Options =
            RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase;

        private static readonly Regex DegreeRegex = new(DegreePattern, Options, TimeSpan.FromMilliseconds(_regExTimeout));

        private static readonly Regex DegreeMinuteRegex = new(DegreeMinutePattern, Options, TimeSpan.FromMilliseconds(_regExTimeout));

        private static readonly Regex DegreeMinuteSecondRegex = new(DegreeMinuteSecondPattern, Options, TimeSpan.FromMilliseconds(_regExTimeout));

        private static readonly Regex IsoRegex =
            new(IsoPattern,
                RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace, TimeSpan.FromMilliseconds(_regExTimeout));

        private static Location? CreateLocation(Angle? latitude, Angle? longitude, double? altitude)
        {
            // Validate the angles to make sure they were correctly parsed
            // and that they are within range (prevents throwing exceptions
            // from the constructors).
            if (latitude == null ||
                longitude == null ||
                Math.Abs(latitude.TotalDegrees) > 90.0 ||
                Math.Abs(longitude.TotalDegrees) > 180.0)
                return null;

            if (altitude != null)
                return new Location(new Latitude(latitude), new Longitude(longitude), altitude.Value);
            return new Location(new Latitude(latitude), new Longitude(longitude));
        }

        private static double MakeSameSign(double source, double value)
        {
            value = Math.Abs(value);
            if (Math.Sign(source) == -1) return -value;
            return value;
        }

        private static Location? ParseLocation(string input, IFormatProvider? provider, Regex regex)
        {
            var match = regex.Match(input.Replace(", ", " "));
            if (!match.Success) return null;

            if (!(TryGetValue(match, "latSuf", out var latSuf) &&
                  TryGetValue(match, "latDeg", out var latDeg)))
                return null;

            TryGetValue(match, "latMin", out var latMin);
            TryGetValue(match, "latSec", out var latSec);

            var latitude = ParseAngle(
                provider, latSuf!, latDeg!, latMin, latSec);

            if (!(TryGetValue(match, "lonSuf", out var lonSuf) &&
                  TryGetValue(match, "lonDeg", out var lonDeg)))
                return null;

            TryGetValue(match, "lonMin", out var lonMin);
            TryGetValue(match, "lonSec", out var lonSec);

            var longitude = ParseAngle(
                provider, lonSuf!, lonDeg!, lonMin, lonSec);

            return CreateLocation(latitude, longitude, null);
        }

        private static Angle? ParseAngle(IFormatProvider? provider, string suffix, string degrees,
            string? minutes = null, string? seconds = null)
        {
            double minuteValue = 0;
            double secondValue = 0;

            // First try parsing the values (minutes and seconds are optional)
            if (!double.TryParse(degrees, NumberStyles.Float, provider, out var degreeValue) ||
                minutes != null && !double.TryParse(minutes, NumberStyles.Float, provider, out minuteValue) ||
                seconds != null && !double.TryParse(seconds, NumberStyles.Float, provider, out secondValue))
                return null;

            // We've parsed all the information! Now make everything the correct
            // sign, starting with degrees.
            if (!string.IsNullOrEmpty(suffix))
            {
                // Make the angle a known sign and invert it if we need to
                degreeValue = Math.Abs(degreeValue);
                switch (suffix)
                {
                    case "s":
                    case "S":
                    case "w":
                    case "W":
                        degreeValue = -degreeValue;
                        break;
                }
            }

            // Now make minutes + seconds have the same sign
            minuteValue = MakeSameSign(degreeValue, minuteValue);
            secondValue = MakeSameSign(degreeValue, secondValue);

            // Return then angle
            return Angle.FromDegrees(degreeValue, minuteValue, secondValue);
        }

        /// <summary>
        ///     Parses the input string for a value containg a pair of degree
        ///     values.
        /// </summary>
        /// <param name="value">The input to parse.</param>
        /// <param name="provider">
        ///     The culture-specific formatting information to use when parsing.
        /// </param>
        /// <returns>
        ///     A Location representing the string on success; otherwise, null.
        /// </returns>
        internal static Location? ParseDegrees(string value, IFormatProvider? provider)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return ParseLocation(value, provider, DegreeRegex);
        }

        /// <summary>
        ///     Parses the input string for a value containg a pair of degree
        ///     minute values.
        /// </summary>
        /// <param name="value">The input to parse.</param>
        /// <param name="provider">
        ///     The culture-specific formatting information to use when parsing.
        /// </param>
        /// <returns>
        ///     A Location representing the string on success; otherwise, null.
        /// </returns>
        internal static Location? ParseDegreesMinutes(string value, IFormatProvider? provider)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return ParseLocation(value, provider, DegreeMinuteRegex);
        }

        /// <summary>
        ///     Parses the input string for a value containing a pair of degree
        ///     minute second values.
        /// </summary>
        /// <param name="value">The input to parse.</param>
        /// <param name="provider">
        ///     The culture-specific formatting information to use when parsing.
        /// </param>
        /// <returns>
        ///     A Location representing the string on success; otherwise, null.
        /// </returns>
        internal static Location? ParseDegreesMinutesSeconds(string value, IFormatProvider? provider)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            return ParseLocation(value, provider, DegreeMinuteSecondRegex);
        }

        /// <summary>
        ///     Parses the specified input string for an ISO 6709 formatted
        ///     coordinate from a string.
        /// </summary>
        /// <param name="value">The input to parse.</param>
        /// <returns>
        ///     A Location representing the string on success; otherwise, null.
        /// </returns>
        internal static Location? ParseIso(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var match = IsoRegex.Match(value);
                if (match.Success)
                {
                    var latitude = ParseIsoAngle(match.Groups[1].Value, 2);
                    var longitude = ParseIsoAngle(match.Groups[2].Value, 3);

                    double? altitude = null;
                    var group = match.Groups[3];
                    if (group.Success) altitude = double.Parse(group.Value, CultureInfo.InvariantCulture);

                    return CreateLocation(latitude, longitude, altitude);
                }
            }

            return null;
        }

        private static Angle? ParseIsoAngle(string value, int degreeDigits)
        {
            var decimalPoint = value.IndexOf('.');
            if (decimalPoint == -1) decimalPoint = value.Length;

            Angle angle;

            // The only variable is the number of degree digits - there will
            // always be the sign, two minute digits and two seconds digits
            switch (decimalPoint - degreeDigits)
            {
                case 1: // sign only : value represents degrees
                    angle = Angle.FromDegrees(
                        double.Parse(value[1..], CultureInfo.InvariantCulture));
                    break;
                case 3: // sign + MM : value is degrees and minutes
                    angle = Angle.FromDegrees(
                        int.Parse(value.AsSpan(1, degreeDigits), CultureInfo.InvariantCulture),
                        double.Parse(value[(degreeDigits + 1)..], CultureInfo.InvariantCulture));
                    break;
                case 5: // sign + MM + SS : value is degrees, minutes and seconds
                    angle = Angle.FromDegrees(
                        int.Parse(value.AsSpan(1, degreeDigits), CultureInfo.InvariantCulture),
                        int.Parse(value.AsSpan(degreeDigits + 1, 2), CultureInfo.InvariantCulture),
                        double.Parse(value[(degreeDigits + 3)..], CultureInfo.InvariantCulture));
                    break;
                default:
                    return null; // Invalid format
            }

            if (value[0] == '-') // Check the sign
                return Angle.Negate(angle);
            return angle;
        }

        private static bool TryGetValue(Match match, string groupName, out string? result)
        {
            var group = match.Groups[groupName];

            // Need to check that only a single capture occurred, as the suffixes are used more than once
            if (group is { Success: true, Captures.Count: 1 })
            {
                result = group.Value;
                return true;
            }

            result = null;
            return false;
        }
    }
}
