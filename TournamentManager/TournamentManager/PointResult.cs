namespace TournamentManager;

/// <summary>
/// Class which represents any points which can occur in sports match with 2 teams.
/// </summary>
public class PointResult : IOpponent<int?>, IComparable<IOpponent<int?>>, IComparer<IOpponent<int?>>
{
    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="result">The result as a string.</param>
    /// <param name="pointsSeparator">The separators which are used to separate home points and guest points</param>
    public PointResult(string result, char pointsSeparator = ':')
    {
        PointsSeparator = pointsSeparator;
        var r = result.Split(PointsSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (r.Length < 2)
        {
            Home = Guest = null;
        }
        else
        {
            Home = int.Parse(r[0], System.Globalization.CultureInfo.InvariantCulture);
            Guest = int.Parse(r[1], System.Globalization.CultureInfo.InvariantCulture);
        }
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    /// <param name="home">The home points of the result.</param>
    /// <param name="guest">The guest points of the result.</param>
    public PointResult(int? home, int? guest)
    {
        Home = home;
        Guest = guest;
    }

    /// <summary>
    /// Initializes a new instance
    /// </summary>
    public PointResult()
    {
    }

    /// <summary>
    /// Gets or sets the separator string which is used to separate home points and guest points.
    /// </summary>
    public char PointsSeparator { get; set; } = ':';

    /// <summary>
    /// Gets or sets the home points of the result.
    /// </summary>
    public int? Home { get; set; }

    /// <summary>
    /// Gets or sets the guest points of the result.
    /// </summary>
    public int? Guest { get; set; }

    /// <summary>
    /// Implements <see cref="IComparable{IResult}"/>. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(IOpponent<int?>? other)
    {
        return Compare(this, other);
    }

    /// <summary>
    /// Implements <see cref="IComparer{IOpponent}"/>. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(IOpponent<int?>? x, IOpponent<int?>? y)
    {
        // The "not null" is more than the "null" opponent

        if (x is null && y is not null) return -1;
        if (x is not null && y is null) return 1;
        if (x is null && y is null) return 0;

        if ((x!.Home ?? 0) < (y!.Home ?? 0)) return -1;
        if ((x.Home ?? 0) > (y.Home ?? 0)) return 1;

        // Home points are equal

        if ((x.Guest ?? 0) < (y.Guest ?? 0)) return 1;
        if ((x.Guest ?? 0) > (y.Guest ?? 0)) return -1;

        // Home and guest points are equal
        return 0;
    }

    /// <summary>
    /// Gets the string representation of the result using a format string with 2 placeholders, e.g. &quot;Home: {0} - Guest: {1} &quot;
    /// </summary>
    /// <param name="format">The format string with 2 placeholders, e.g. &quot;Home: {0} - Guest: {1} &quot;</param>
    /// <returns>Returns the string representation of the result using a format string.</returns>
    public string ToString(string format)
    {
        return string.Format(format, Home.HasValue ? Home : "-", Guest.HasValue ? Guest : "-");
    }

    /// <summary>
    /// Gets the string representation of the result using the default points separator.
    /// </summary>
    /// <returns>Returns the string representation of the result using the default points separator.</returns>
    public override string ToString()
    {
        return ToString($"{{0}}{PointsSeparator}{{1}}");
    }

    /// <summary>
    /// Adds the 2 results. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static PointResult operator +(PointResult a, IOpponent<int?> b)
    {
        return new((a.Home ?? 0) + (b.Home ?? 0), (a.Guest ?? 0) + (b.Guest ?? 0));
    }

    /// <summary>
    /// Subtracts results A from result B. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static PointResult operator -(PointResult a, IOpponent<int?> b)
    {
        if (a.Home < b.Home || a.Guest < b.Guest)
            throw new ArgumentException(@"Operation would lead to negative points value.", b.ToString());

        return new((a.Home ?? 0) - (b.Home ?? 0), (a.Guest ?? 0) - (b.Guest ?? 0));
    }

    /// <summary>
    /// Compares 2 results for equality. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static bool operator ==(PointResult a, IOpponent<int?> b)
    {
        return a.CompareTo(b) == 0;
    }

    /// <summary>
    /// Compares 2 results for inequality. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static bool operator !=(PointResult a, IOpponent<int?> b)
    {
        return a.CompareTo(b) != 0;
    }

    /// <summary>
    /// Compares 2 results for less than. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static bool operator <(PointResult a, IOpponent<int?> b)
    {
        return a.CompareTo(b) == -1;
    }

    /// <summary>
    /// Compares 2 results for more than. <see langword="null"/> results are treated as zero.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static bool operator >(PointResult a, IOpponent<int?> b)
    {
        return a.CompareTo(b) == 1;
    }

    #region ** Equality members **

    protected bool Equals(PointResult other)
    {
        return PointsSeparator == other.PointsSeparator && Home == other.Home && Guest == other.Guest;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType() == GetType() && Equals((PointResult) obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(ToString());
    }

    #endregion
}
