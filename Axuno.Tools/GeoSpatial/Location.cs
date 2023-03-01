using System;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Represents a Latitude/Longitude/Altitude coordinate.
/// </summary>
/// <example>
/// var latitude = Latitude.FromDegrees(-5, -10, -15.1234);
/// Console.WriteLine("{0:DMS1}", latitude); // 5° 10′ 15.1″ S
/// Console.WriteLine("{0:DM3}", latitude);  // 5° 10.252′ S
/// Console.WriteLine("{0:D}", latitude);    // 5.17° S
/// Console.WriteLine("{0:ISO}", latitude);  // -051015.1234
/// </example>
public sealed partial class Location : IEquatable<Location>, IFormattable, IXmlSerializable
{
    // Equatorial = 6378137, polar = 6356752.
    private const int EarthRadius = 6366710; // The mean radius of the earth in meters.

    /// <summary>Initializes a new instance of the Location class.</summary>
    /// <param name="latitude">The latitude of the coordinate.</param>
    /// <param name="longitude">The longitude of the coordinate.</param>
    /// <exception cref="ArgumentNullException">
    ///     latitude or longitude is null.
    /// </exception>
    public Location(Latitude latitude, Longitude longitude)
    {
        Latitude = latitude ?? throw new ArgumentNullException(nameof(latitude));
        Longitude = longitude ?? throw new ArgumentNullException(nameof(longitude));
    }

    /// <summary>Initializes a new instance of the Location class.</summary>
    /// <param name="latitude">The latitude of the coordinate.</param>
    /// <param name="longitude">The longitude of the coordinate.</param>
    /// <param name="altitude">
    ///     The altitude, specified in meters, of the coordinate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    ///     latitude/longitude is null.
    /// </exception>
    public Location(Latitude latitude, Longitude longitude, double altitude)
        : this(latitude, longitude)
    {
        Altitude = altitude;
    }

    /// <summary>
    ///     Gets the altitude of the coordinate, or null if the coordinate
    ///     does not contain altitude information.
    /// </summary>
    public double? Altitude { get; private set; }

    /// <summary>Gets the latitude of the coordinate.</summary>
    public Latitude Latitude { get; private set; }

    /// <summary>Gets the longitude of the coordinate.</summary>
    public Longitude Longitude { get; private set; }

    /// <summary>
    ///     Determines whether this instance and another specified Location object
    ///     have the same value.
    /// </summary>
    /// <param name="other">The Location to compare to this instance.</param>
    /// <returns>
    ///     true if the value of the value parameter is the same as this instance;
    ///     otherwise, false.
    /// </returns>
    public bool Equals(Location? other)
    {
        if (other is null) return false;

        return Altitude.Equals(other.Altitude) && Latitude.Equals(other.Latitude) &&
               Longitude.Equals(other.Longitude);
    }

    /// <summary>
    ///     Formats the value of the current instance using the specified format.
    /// </summary>
    /// <param name="format">
    ///     The format to use or null to use the default format (see
    ///     <see cref="Angle.ToString(string, IFormatProvider)" />).
    /// </param>
    /// <param name="formatProvider">
    ///     The provider to use to format the value or null to use the format
    ///     information from the current locale setting of the operating system.
    /// </param>
    /// <returns>
    ///     The value of the current instance in the specified format.
    /// </returns>
    /// <exception cref="ArgumentException">format is unknown.</exception>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (string.IsNullOrEmpty(format)) format = "DMS";
        formatProvider ??= CultureInfo.CurrentCulture;

        var builder = new StringBuilder();
        if (format == "ISO")
        {
            builder.Append(Latitude.ToString("ISO", null));
            builder.Append(Longitude.ToString("ISO", null));
            if (Altitude != null)
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0:+0.###;-0.###}", Altitude.Value);
            builder.Append('/');
        }
        else
        {
            var parsed = Angle.ParseFormatString(format);

            builder.Append(Latitude.ToString(format, formatProvider));
            builder.Append(' ');
            builder.Append(Longitude.ToString(format, formatProvider));
            if (Altitude != null)
            {
                builder.Append(' ');
                builder.Append(Angle.GetString(Altitude.Value, 1, parsed.Item2, formatProvider));
                builder.Append('m');
            }
        }

        return builder.ToString();
    }

    /// <summary>This method is reserved and should not be used.</summary>
    /// <returns>This method always returns null.</returns>
    /// <remarks>
    ///     The IXmlSerializable interface documentation specifies that this
    ///     method should always return null.
    /// </remarks>
    XmlSchema? IXmlSerializable.GetSchema()
    {
        return null;
    }

    /// <summary>Generates an object from its XML representation.</summary>
    /// <param name="reader">
    ///     The XmlReader stream from which the object is deserialized.
    /// </param>
    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        // Important we check if it's an empty element because if it's not
        // we need to call ReadEndElement, which will skip the next element
        // if this element is empty, meaning data will be skipped.
        if (reader.IsEmptyElement)
        {
            reader.Skip();
        }
        else
        {
            if (TryParse(reader.ReadString(), LocationStyles.Iso, CultureInfo.InvariantCulture, out var parsed))
            {
                Altitude = parsed!.Altitude;
                Latitude = parsed.Latitude;
                Longitude = parsed.Longitude;
            }

            reader.ReadEndElement();
        }
    }

    /// <summary>Converts an object into its XML representation.</summary>
    /// <param name="writer">
    ///     The XmlWriter stream to which the object is serialized.
    /// </param>
    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        writer.WriteString(ToString("ISO", CultureInfo.InvariantCulture));
    }

    // These functions are based on the Aviation Formulary V1.45
    // by Ed Williams (http://williams.best.vwh.net/avform.htm)

    /// <summary>
    ///     Calculates the initial azimuth (or course; the angle measured
    ///     clockwise from true north) from this instance to the specified
    ///     value.
    /// </summary>
    /// <param name="point">The location of the other point.</param>
    /// <returns>
    ///     The initial course from this instance to the specified point.
    /// </returns>
    /// <example>
    ///     The azimuth from 0,0 to 1,0 is 0 degrees. From 0,0 to 0,1 is 90
    ///     degrees (due east).
    /// </example>
    public Angle Azimuth(Location point)
    {
        var lat1 = Latitude.Radians;
        var lon1 = Longitude.Radians;
        var lat2 = point.Latitude.Radians;
        var lon2 = point.Longitude.Radians;

        var x = Math.Cos(lat1) * Math.Sin(lat2) -
                Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(lon2 - lon1);
        var tan = Math.Atan2(Math.Sin(lon2 - lon1) * Math.Cos(lat2), x);

        return Angle.FromRadians(tan % (2 * Math.PI));
    }

    /// <summary>
    ///     Calculates the great circle distance, in meters, between this instance
    ///     and the specified value.
    /// </summary>
    /// <param name="point">The location of the other point.</param>
    /// <returns>The great circle distance, in meters.</returns>
    /// <remarks>The ante meridian is not considered.</remarks>
    /// <exception cref="ArgumentNullException">point is null.</exception>
    public double Distance(Location point)
    {
        if (point == null) throw new ArgumentNullException(nameof(point));

        var lat1 = Latitude.Radians;
        var lon1 = Longitude.Radians;
        var lat2 = point.Latitude.Radians;
        var lon2 = point.Longitude.Radians;

        var latitudeSquared = Math.Pow(Math.Sin((lat1 - lat2) / 2), 2);
        var longitudeSquared = Math.Pow(Math.Sin((lon1 - lon2) / 2), 2);
        var sqrt = Math.Sqrt(latitudeSquared + Math.Cos(lat1) * Math.Cos(lat2) * longitudeSquared);
        var distance = RadiansToMeters(2 * Math.Asin(sqrt));

        if (Altitude == null || point.Altitude == null) return distance;

        var altitudeDelta = point.Altitude - Altitude;
        return Math.Sqrt(Math.Pow(distance, 2) + Math.Pow(altitudeDelta.Value, 2));
    }

    /// <summary>
    ///     Determines whether this instance and a specified object, which must
    ///     also be a Location, have the same value.
    /// </summary>
    /// <param name="obj">The Location to compare to this instance.</param>
    /// <returns>
    ///     true if obj is a Location and its value is the same as this instance;
    ///     otherwise, false.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Location);
    }

    /// <summary>Returns the hash code for this instance.</summary>
    /// <returns>A 32-bit signed integer hash code.</returns>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = 17 * Latitude.GetHashCode();
            hash = hash * 23 + Longitude.GetHashCode();

            if (Altitude != null) hash = hash * 23 + Altitude.GetHashCode();
            return hash;
        }
    }

    /// <summary>
    ///     Calculates a point at the specified distance along the specified
    ///     radial from this instance.
    /// </summary>
    /// <param name="distance">The distance, in meters.</param>
    /// <param name="radial">
    ///     The course radial from this instance, measured clockwise from north.
    /// </param>
    /// <returns>A Location containing the calculated point.</returns>
    /// <exception cref="ArgumentNullException">radial is null.</exception>
    /// <remarks>The ante meridian is not considered.</remarks>
    public Location GetPoint(double distance, Angle? radial)
    {
        if (radial == null) throw new ArgumentNullException(nameof(radial));

        var lat = Latitude.Radians;
        var lon = Longitude.Radians;
        distance = MetersToRadians(distance);

        var latDist = Math.Cos(lat) * Math.Sin(distance);
        var radialLat = Math.Asin(Math.Sin(lat) * Math.Cos(distance) +
                                  latDist * Math.Cos(radial.Radians));

        var y = Math.Sin(radial.Radians) * latDist;
        var x = Math.Cos(distance) - Math.Sin(lat) * Math.Sin(radialLat);
        var atan = Math.Atan2(y, x);

        var radialLon = (lon + atan + Math.PI) % (2 * Math.PI) - Math.PI;

        return new Location(
            Latitude.FromRadians(radialLat),
            Longitude.FromRadians(radialLon));
    }

    private static double MetersToRadians(double meters)
    {
        return meters / EarthRadius;
    }

    /// <summary>
    ///     Determines whether two specified Locations have the same value.
    /// </summary>
    /// <param name="locationA">The first Location to compare, or null.</param>
    /// <param name="locationB">The second Location to compare, or null.</param>
    /// <returns>
    ///     true if the value of locationA is the same as the value of locationB;
    ///     otherwise, false.
    /// </returns>
    public static bool operator ==(Location? locationA, Location? locationB)
    {
        return locationA?.Equals(locationB) ?? locationB is null;
    }

    /// <summary>
    ///     Determines whether two specified Locations have different values.
    /// </summary>
    /// <param name="locationA">The first Location to compare, or null.</param>
    /// <param name="locationB">The second Location to compare, or null.</param>
    /// <returns>
    ///     true if the value of locationA is different from the value of
    ///     locationB; otherwise, false.
    /// </returns>
    public static bool operator !=(Location? locationA, Location? locationB)
    {
        return !(locationA == locationB);
    }

    /// <summary>Converts the string into a Location.</summary>
    /// <param name="value">
    ///     A string containing a co-ordinate to convert.
    /// </param>
    /// <param name="provider">
    ///     An object that supplies culture-specific formatting information about
    ///     the input string.
    /// </param>
    /// <returns>
    ///     A Location that is equivalent to the value specified in value.
    /// </returns>
    /// <exception cref="ArgumentNullException">value is null.</exception>
    /// <exception cref="FormatException">
    ///     value does not represent a valid co-ordinate.
    /// </exception>
    public static Location Parse(string value, IFormatProvider? provider)
    {
        return Parse(value, LocationStyles.None, provider);
    }

    /// <summary>Converts the string into a Location.</summary>
    /// <param name="value">
    ///     A string containing a co-ordinate to convert.
    /// </param>
    /// <param name="style">
    ///     A combination of allowable styles that value can be formatted to.
    /// </param>
    /// <param name="provider">
    ///     An object that supplies culture-specific formatting information about
    ///     the input string.
    /// </param>
    /// <returns>
    ///     A Location that is equivalent to the value specified in value.
    /// </returns>
    /// <exception cref="ArgumentNullException">value is null.</exception>
    /// <exception cref="FormatException">
    ///     value does not represent a valid co-ordinate.
    /// </exception>
    public static Location Parse(string value, LocationStyles style = LocationStyles.None,
        IFormatProvider? provider = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));

        if (TryParse(value, style, provider, out var output)) return output!;
        throw new FormatException();
    }

    private static double RadiansToMeters(double radians)
    {
        return radians * EarthRadius;
    }

    /// <summary>
    ///     Returns a string that represents the current Location in degrees,
    ///     minutes and seconds form.
    /// </summary>
    /// <returns>A string that represents the current instance.</returns>
    public override string ToString()
    {
        return ToString(null, null);
    }

    /// <summary>Converts the string into a Location.</summary>
    /// <param name="value">
    ///     A string containing a co-ordinate to convert.
    /// </param>
    /// <param name="location">
    ///     Contains the converted value, if the conversion succeeded, or null
    ///     if the conversion failed. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     true if the value was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParse(string value, out Location? location)
    {
        return TryParse(value, LocationStyles.None, null, out location);
    }

    /// <summary>Converts the string into a Location.</summary>
    /// <param name="value">
    ///     A string containing a co-ordinate to convert.
    /// </param>
    /// <param name="provider">
    ///     An object that supplies culture-specific formatting information about
    ///     the input string.
    /// </param>
    /// <param name="location">
    ///     Contains the converted value, if the conversion succeeded, or null
    ///     if the conversion failed. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     true if the value was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParse(string value, IFormatProvider provider, out Location? location)
    {
        return TryParse(value, LocationStyles.None, provider, out location);
    }

    /// <summary>Converts the string into a Location.</summary>
    /// <param name="value">
    ///     A string containing a co-ordinate to convert.
    /// </param>
    /// <param name="style">
    ///     A combination of allowable styles that value can be formatted to.
    /// </param>
    /// <param name="provider">
    ///     An object that supplies culture-specific formatting information about
    ///     the input string.
    /// </param>
    /// <param name="location">
    ///     Contains the converted value, if the conversion succeeded, or null
    ///     if the conversion failed. This parameter is passed uninitialized.
    /// </param>
    /// <returns>
    ///     true if the value was converted successfully; otherwise, false.
    /// </returns>
    public static bool TryParse(string value, LocationStyles style, IFormatProvider? provider, out Location? location)
    {
        location = null;
        if (style == LocationStyles.None)
            style = LocationStyles.Degrees | LocationStyles.DegreesMinutes | LocationStyles.DegreesMinutesSeconds;

        if ((style & LocationStyles.Iso) != 0)
        {
            location = Parser.ParseIso(value);
            if (location != null) return true;
        }

        if ((style & LocationStyles.DegreesMinutesSeconds) != 0)
        {
            location = Parser.ParseDegreesMinutesSeconds(value, provider);
            if (location != null) return true;
        }

        if ((style & LocationStyles.DegreesMinutes) != 0)
        {
            location = Parser.ParseDegreesMinutes(value, provider);
            if (location != null) return true;
        }

        if ((style & LocationStyles.Degrees) != 0)
        {
            location = Parser.ParseDegrees(value, provider);
            if (location != null) return true;
        }

        return location != null;
    }
}
