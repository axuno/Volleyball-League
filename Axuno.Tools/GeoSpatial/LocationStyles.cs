using System;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
/// Determines the styles permitted in string arguments that are passed
/// to the Parse/TryParse methods of Location.
/// </summary>
[Flags]
public enum LocationStyles
{
    /// <summary>
    /// Default formatting options are used. Indicates that the Degrees,
    /// DegreesMinutes and DegreeMinuteSeconds styles are used.
    /// </summary>
    None = 0x00,

    /// <summary>Allows parsing of degree values only.</summary>
    Degrees = 0x01,

    /// <summary>
    /// Allows parsing of values that contain both degrees and minutes.
    /// </summary>
    DegreesMinutes = 0x02,

    /// <summary>
    /// Allows parsing of values that contain degrees, minutes and seconds.
    /// </summary>
    DegreesMinutesSeconds = 0x04,

    /// <summary>
    /// Allows parsing of values formmated to ISO 6709.
    /// </summary>
    Iso = 0x08,

    /// <summary>
    /// Indicates that the Degrees, DegreesMinutes, DegreeMinuteSeconds
    /// and Iso styles are used.
    /// </summary>
    Any = Degrees | DegreesMinutes | DegreesMinutesSeconds | Iso
}