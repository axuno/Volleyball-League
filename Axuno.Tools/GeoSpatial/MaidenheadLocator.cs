using System;
using System.Text.RegularExpressions;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
///     Class providing static methods for converting Maidenhead locators to latitude and longitude and vice versa
/// </summary>
/// <remarks>
///     Longitude:
///     1st character:  A B C D E F G H I J K L M N O P Q R              letter
///     |     |     |     |     |     |
///     -180  -120  -60   0     +60   +120               degrees
///     3rd character:  0 1 2 3 4 5 6 7 8 9                              digit
///     |         |       |
///     0         +10     +18                            degrees
///     5th character:  A B C D E F G H I J K L M N O P Q R S T U V W X  letter
///     |       |       |       |       |       |
///     0       +20     +40     +60     +80     +100     minutes
///     Latitude:
///     2nd character:  A B C D E F G H I J K L M N O P Q R              letter
///     |     |     |     |     |     |
///     -90   -60   -30   0     +30   +60                degrees
///     4th character:  0 1 2 3 4 5 6 7 8 9                              Ziffer
///     |         |       |
///     0         +5      +9                             degrees
///     6th character:  A B C D E F G H I J K L M N O P Q R S T U V W X  letter
///     |       |       |       |       |       |
///     0       +10     +20     +30     +40     +50      minutes
/// </remarks>
public class MaidenheadLocator
{
    /// <summary>
    ///     Checks whether a string is a valid Maidenhead locator
    /// </summary>
    /// <param name="locator">A Maidenhead locator string</param>
    /// <returns>Return true, if the string is a valid Maidenhead locator</returns>
    public static bool IsLocator(string locator)
    {
        return Regex.IsMatch(locator, "^[A-R]{2}[0-9]{2}[A-X]{2}$");
    }

    /// <summary>
    ///     Convert latitude and longitude in degrees to a locator
    /// </summary>
    /// <param name="latitude">Latitude to convert</param>
    /// <param name="longitude">Longitude to convert</param>
    /// <returns>Locator string</returns>
    public static string LatLongToLocator(double latitude, double longitude)
    {
        var locator = "";

        latitude += 90;
        longitude += 180;
        var v = (int) (longitude / 20);
        longitude -= v * 20;
        locator += (char) ('A' + v);
        v = (int) (latitude / 10);
        latitude -= v * 10;
        locator += (char) ('A' + v);
        locator += ((int) (longitude / 2)).ToString();
        locator += ((int) latitude).ToString();
        longitude -= (int) (longitude / 2) * 2;
        latitude -= (int) latitude;
        locator += (char) ('A' + longitude * 12);
        locator += (char) ('A' + latitude * 24);
        return locator;
    }

    /// <summary>
    /// Convert latitude and longitude expressed as Location to a locator
    /// </summary>
    /// <param name="location">Location to convert</param>
    /// <returns>Locator string</returns>
    public static string LocationToLocator(Location location)
    {
        return LatLongToLocator(location.Latitude.TotalDegrees, location.Longitude.TotalDegrees);
    }

    /// <summary>
    ///     Convert a locator to latitude and longitude expressed as Location
    /// </summary>
    /// <param name="locator">Locator string to convert</param>
    /// <returns>Location of the locator</returns>
    public static Location LocatorToLocation(string locator)
    {
        locator = locator.Trim().ToUpper();
        if (!IsLocator(locator))
            throw new FormatException("Invalid locator format");

        var location = new Location(
            new Latitude(Angle.FromDegrees((locator[1] - 'A') * 10 + (locator[3] - '0') +
                (locator[5] - 'A' + 0.5) / 24 - 90)),
            new Longitude(Angle.FromDegrees((locator[0] - 'A') * 20 + (locator[2] - '0') * 2 +
                (locator[4] - 'A' + 0.5) / 12 - 180)));

        return location;
    }
}