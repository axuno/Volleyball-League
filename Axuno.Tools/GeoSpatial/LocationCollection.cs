using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Axuno.Tools.GeoSpatial;

/// <summary>
///     Represents a collection of Locations and allows their serialization and
///     deserialization from XML.
/// </summary>
public sealed class LocationCollection : ICollection<Location>, IXmlSerializable
{
    private readonly List<Location> locations = new();

    /// <summary>
    /// Gets the number of Locations contained in this instance.
    /// </summary>
    public int Count => locations.Count;

    /// <summary>
    /// Gets a value indicating whether this instance is read-only.
    /// </summary>
    bool ICollection<Location>.IsReadOnly => false;

    /// <summary>Adds a Location to this instance.</summary>
    /// <param name="item">The Location to be added.</param>
    /// <exception cref="ArgumentNullException">item is null.</exception>
    public void Add(Location item)
    {
        if (item == null) throw new ArgumentNullException(nameof(item));
        locations.Add(item);
    }

    /// <summary>Removes all Locations from this instance.</summary>
    public void Clear()
    {
        locations.Clear();
    }

    /// <summary>
    ///     Determines whether a specified Location is contained in this instance.
    /// </summary>
    /// <param name="item">The Location to locate.</param>
    /// <returns>
    ///     true if item is found in this instance; otherwise, false. This
    ///     method also returns false if the specified value parameter is null.
    /// </returns>
    public bool Contains(Location item)
    {
        if (item == null) return false;
        return locations.Contains(item);
    }

    /// <summary>
    ///     Copies this instance to a compatible one-dimensional array, starting
    ///     at the specified index of the target array.
    /// </summary>
    /// <param name="array">The destination one-dimensional array.</param>
    /// <param name="arrayIndex">
    ///     The zero-based index in array at which copying begins.
    /// </param>
    /// <exception cref="ArgumentException">
    ///     The number of Locations contained in this instance is greater than the
    ///     available space from arrayIndex to the end of the destination array.
    /// </exception>
    /// <exception cref="ArgumentNullException">array is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     arrayIndex is less than 0.
    /// </exception>
    public void CopyTo(Location[] array, int arrayIndex)
    {
        locations.CopyTo(array, arrayIndex);
    }

    /// <summary>
    ///     Returns an enumerator that iterates through this instance.
    /// </summary>
    /// <returns>An enumerator for this instance.</returns>
    public IEnumerator<Location> GetEnumerator()
    {
        return locations.GetEnumerator();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through this instance.
    /// </summary>
    /// <returns>An enumerator for this instance.</returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Removes the first occurrence of a specific Location from this instance.
    /// </summary>
    /// <param name="item">The Location to remove.</param>
    /// <returns>
    ///     true if the specified value parameter is successfully removed;
    ///     otherwise, false. This method also returns false if the specified
    ///     value parameter was not found or is null.
    /// </returns>
    public bool Remove(Location item)
    {
        if (item == null) return false;
        return locations.Remove(item);
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
            foreach (var value in SplitString(reader.ReadString(), '/'))
            {
                if (Location.TryParse(value, LocationStyles.Iso, CultureInfo.InvariantCulture, out var location))
                    locations.Add(location!);
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
        var sb = new StringBuilder(16 * locations.Count); // 16 is the minimum that the ISO string will be
        foreach (var location in locations) sb.Append(location.ToString("ISO", CultureInfo.InvariantCulture));
        writer.WriteString(sb.ToString());
    }

    // String.Split eats the separator and also uses lots of memory on long strings
    private static IEnumerable<string> SplitString(string input, char separator)
    {
        if (string.IsNullOrEmpty(input)) yield break;

        var start = 0;
        var index = input.IndexOf(separator);
        while (index != -1)
        {
            // First increase the index so we include the separator in the
            // returned string and also to start searching from the position
            // after the previous find.
            index++;
            var length = index - start;
            yield return input.Substring(start, length);

            start = index;
            index = input.IndexOf(separator, index);
        }
    }
}
