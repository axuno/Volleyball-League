using System.Globalization;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Represents a longitude ("x" axis) co-ordinate.
/// </summary>
public sealed class Longitude : Angle
{
    /// <summary>Initializes a new instance of the Longitude class.</summary>
    /// <param name="angle">The angle of the longitude.</param>
    /// <exception cref="ArgumentNullException">angle is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// angle is greater than 180 degrees or less than -180 degrees.
    /// </exception>
    public Longitude(Angle angle)
        : this((angle ?? new Angle(0)).Radians) // Prevent null reference access, we'll validate later
    {
        if (angle == null) throw new ArgumentNullException(nameof(angle));
        ValidateRange("angle", angle.Radians, -Math.PI, Math.PI);
    }

    private Longitude(double radians)
        : base(Math.Abs(radians - Math.PI) < .0000001 ? -Math.PI : radians)
    {
        // The above test is a special case. According to the ISO 6709, the
        // 180th meridian (180 degrees == Pi radians) is always -180 degrees.
        // Instead of throwing an exception we'll just change the value.
    }

    /// <summary>
    /// Gets a value indicating whether this instance represents am east or
    /// west longitude.
    /// </summary>
    public char Direction => Radians < 0 ? 'W' : 'E';

    /// <summary>Creates a new Longitude from an angle in degrees.</summary>
    /// <param name="degrees">The angle of the longitude in degrees.</param>
    /// <returns>A new Longitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     degrees is greater than 180 or less than -180.
    /// </exception>
    public static new Longitude FromDegrees(double degrees)
    {
        ValidateRange("degrees", degrees, -180, 180);
        return new Longitude(Angle.FromDegrees(degrees).Radians);
    }

    /// <summary>
    ///     Creates a new Longitude from an angle in degrees and minutes.
    /// </summary>
    /// <param name="degrees">The amount of degrees.</param>
    /// <param name="minutes">The amount of minutes.</param>
    /// <returns>A new Longitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The specified angle (degrees + minutes) is greater than 180 or less
    ///     than -180.
    /// </exception>
    public static new Longitude FromDegrees(double degrees, double minutes)
    {
        var angle = Angle.FromDegrees(degrees, minutes);
        ValidateRange("angle", angle.TotalDegrees, -180, 180);

        return new Longitude(angle.Radians);
    }

    /// <summary>
    ///     Creates a new Longitude from an angle in degrees, minutes and seconds.
    /// </summary>
    /// <param name="degrees">The amount of degrees.</param>
    /// <param name="minutes">The amount of minutes.</param>
    /// <param name="seconds">The amount of seconds.</param>
    /// <returns>A new Longitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     The specified angle (degrees + minutes + seconds) is greater than
    ///     180 or less than -180.
    /// </exception>
    public static new Longitude FromDegrees(double degrees, double minutes, double seconds)
    {
        var angle = Angle.FromDegrees(degrees, minutes, seconds);
        ValidateRange("angle", angle.TotalDegrees, -180, 180);

        return new Longitude(angle.Radians);
    }

    /// <summary>Creates a new Longitude from an amount in radians.</summary>
    /// <param name="radians">The angle of the longitude in radians.</param>
    /// <returns>A new Longitude representing the specified value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     radians is greater than PI or less than -PI.
    /// </exception>
    public static new Longitude FromRadians(double radians)
    {
        ValidateRange(nameof(radians), radians, -Math.PI, Math.PI);
        return new Longitude(radians);
    }

    /// <summary>
    ///     Formats the value of the current instance using the specified format.
    /// </summary>
    /// <param name="format">
    ///     The format to use (see remarks) or null to use the default format.
    /// </param>
    /// <param name="formatProvider">
    ///     The provider to use to format the value or null to use the format
    ///     information from the current locale setting of the operating system.
    /// </param>
    /// <returns>
    ///     The value of the current instance in the specified format.
    /// </returns>
    /// <exception cref="ArgumentException">format is unknown.</exception>
    /// <remarks>
    ///     Valid format strings are those for
    ///     <see cref="Angle.ToString(string, IFormatProvider)" /> plus "ISO"
    ///     (without any precision specifier), which returns the angle in
    ///     ISO 6709 compatible format.
    /// </remarks>
    public override string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (format == "ISO")
            return string.Format(
                CultureInfo.InvariantCulture, // ISO defines the punctuation
                "{0:+000;-000}{1:00}{2:00.####}",
                TotalDegrees,
                Math.Abs(Minutes),
                Math.Abs(Seconds));

        var formatted = base.ToString(format, formatProvider);
        if (Radians < 0)
        {
            // We're going to remove the negative sign, but find out what a
            // negative sign is in the current format provider
            var numberFormat = GetNumberFormatInfo(formatProvider);
            var negativeSign = numberFormat.NegativeSign;
            formatted = formatted[negativeSign.Length..];
        }

        return formatted + " " + Direction;
    }
}
