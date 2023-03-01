using System;
using System.Globalization;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Represents a latitude ("y" axis) co-ordinate.
/// </summary>
public sealed class Latitude : Angle
{
    /// <summary>Initializes a new instance of the Latitude class.</summary>
    /// <param name="angle">The angle of the latitude.</param>
    /// <exception cref="ArgumentNullException">angle is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// angle is greater than 90 degrees or less than -90 degrees.
    /// </exception>
    public Latitude(Angle? angle)
        : base((angle ?? new Angle(0)).Radians) // Prevent null reference access
    {
        ValidateRange("angle", Radians, -Math.PI / 2.0, Math.PI / 2.0);
    }

    private Latitude(double radians)
        : base(radians)
    {
    }

    /// <summary>
    /// Gets a value indicating whether this instance represents a north or
    /// south latitude.
    /// </summary>
    public char Direction => Radians < 0 ? 'S' : 'N';

    /// <summary>Creates a new Latitude from an angle in degrees.</summary>
    /// <param name="degrees">The angle of the latitude in degrees.</param>
    /// <returns>A new Latitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// degrees is greater than 90 or less than -90.
    /// </exception>
    public static new Latitude FromDegrees(double degrees)
    {
        ValidateRange("degrees", degrees, -90, 90);
        return new Latitude(Angle.FromDegrees(degrees).Radians);
    }

    /// <summary>
    /// Creates a new Latitude from an angle in degrees and minutes.
    /// </summary>
    /// <param name="degrees">The amount of degrees.</param>
    /// <param name="minutes">The amount of minutes.</param>
    /// <returns>A new Latitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The specified angle (degrees + minutes) is greater than 90 or less
    /// than -90.
    /// </exception>
    public static new Latitude FromDegrees(double degrees, double minutes)
    {
        var angle = Angle.FromDegrees(degrees, minutes);
        ValidateRange("angle", angle.TotalDegrees, -90, 90);

        return new Latitude(angle.Radians);
    }

    /// <summary>
    /// Creates a new Latitude from an angle in degrees, minutes and seconds.
    /// </summary>
    /// <param name="degrees">The amount of degrees.</param>
    /// <param name="minutes">The amount of minutes.</param>
    /// <param name="seconds">The amount of seconds.</param>
    /// <returns>A new Latitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The specified angle (degrees + minutes + seconds) is greater than
    /// 90 or less than -90.
    /// </exception>
    public static new Latitude FromDegrees(double degrees, double minutes, double seconds)
    {
        var angle = Angle.FromDegrees(degrees, minutes, seconds);
        ValidateRange("angle", angle.TotalDegrees, -90, 90);

        return new Latitude(angle.Radians);
    }

    /// <summary>Creates a new Latitude from an amount in radians.</summary>
    /// <param name="radians">The angle of the latitude in radians.</param>
    /// <returns>A new Latitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// radians is greater than PI/2 or less than -PI/2.
    /// </exception>
    public static new Latitude FromRadians(double radians)
    {
        ValidateRange("radians", radians, -Math.PI / 2.0, Math.PI / 2.0);
        return new Latitude(radians);
    }

    /// <summary>
    /// Formats the value of the current instance using the specified format.
    /// </summary>
    /// <param name="format">
    /// The format to use (see remarks) or null to use the default format.
    /// </param>
    /// <param name="formatProvider">
    /// The provider to use to format the value or null to use the format
    /// information from the current locale setting of the operating system.
    /// </param>
    /// <returns>
    /// The value of the current instance in the specified format.
    /// </returns>
    /// <exception cref="ArgumentException">format is unknown.</exception>
    /// <remarks>
    /// Valid format strings are those for
    /// <see cref="Angle.ToString(string, IFormatProvider)"/> plus "ISO"
    /// (without any precision specifier), which returns the angle in
    /// ISO 6709 compatible format.
    /// </remarks>
    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "ISO")
        {
            return string.Format(
                CultureInfo.InvariantCulture, // ISO defines the punctuation
                "{0:+00;-00}{1:00}{2:00.####}",
                TotalDegrees,
                Math.Abs(Minutes),
                Math.Abs(Seconds));
        }

        var formatted = base.ToString(format, formatProvider);
        if (Radians < 0)
        {
            // We're going to remove the negative sign, but find out what a
            // negative sign is in the current format provider
            var numberFormat = Angle.GetNumberFormatInfo(formatProvider);
            var negativeSign = numberFormat.NegativeSign;
            formatted = formatted[negativeSign.Length..];
        }
        return formatted + " " + Direction;
    }
}
