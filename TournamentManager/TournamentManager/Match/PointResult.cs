using System;
using System.Collections.Generic;

namespace TournamentManager.Match;

/// <summary>
/// Class which represents any points which can occur in sports match with 2 teams.
/// </summary>
public class PointResult : IOpponent<int>, IComparable<IOpponent<int>>, IComparer<IOpponent<int>>
{
    /// <summary>
    /// Initializes a new instance of the class TournamentManager.Match.ResultBase
    /// </summary>
    /// <param name="result">The result as a string.</param>
    /// <param name="pointsSeparator">The separator which is used to separate home points and guest points</param>
    public PointResult(string result, string pointsSeparator = ":")
    {
        PointsSeparator = pointsSeparator;
        var r = result.Split(new[] { PointsSeparator }, StringSplitOptions.None);
        try
        {
            Home = int.Parse(r[0], System.Globalization.CultureInfo.CurrentCulture);
            Guest = int.Parse(r[1], System.Globalization.CultureInfo.CurrentCulture);
        }
        catch
        {
            throw new Exception();
        }
    }

    /// <summary>
    /// Initializes a new instance of the class TournamentManager.Match.ResultBase
    /// </summary>
    /// <param name="home">The home points of the result.</param>
    /// <param name="guest">The guest points of the result.</param>
    public PointResult(int home, int guest)
    {
        Home = home;
        Guest = guest;
    }

    static PointResult()
    {
    }

    /// <summary>
    /// Gets or sets the separator string which is used to separate home points and guest points.
    /// </summary>
    public string PointsSeparator { get; set; } = ":";

    /// <summary>
    /// Gets or sets the home points of the result.
    /// </summary>
    public int Home { get; set; }

    /// <summary>
    /// Gets or sets the guest points of the result.
    /// </summary>
    public int Guest { get; set; }

    /// <summary>
    /// Implements <see cref="IComparable{IResult}"/>
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public int CompareTo(IOpponent<int>? other)
    {
        return Compare(this, other);
    }

    /// <summary>
    /// Implements <see cref="IComparer{IOpponent}"/>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public int Compare(IOpponent<int>? x, IOpponent<int>? y)
    {
        if (x == null)
            throw new ArgumentNullException(nameof(x));
        if (y == null)
            throw new ArgumentNullException(nameof(y));

        if (x.Home < y.Home)
            return -1;
        if (x.Home > y.Home)
            return 1;

        // Home points are equal

        if (x.Guest < y.Guest)
            return 1;
        if (x.Guest > y.Guest)
            return -1;

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
        return string.Format(format, Home, Guest);
    }

    /// <summary>
    /// Gets the string representation of the result using the default points separator.
    /// </summary>
    /// <returns>Returns the string representation of the result using the default points separator.</returns>
    public override string ToString()
    {
        return string.Concat(Home, PointsSeparator, Guest);
    }

    /// <summary>
    /// Adds the 2 results.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static PointResult operator +(PointResult a, IOpponent<int> b)
    {
        return new PointResult(a.Home + b.Home, a.Guest + b.Guest);
    }

    /// <summary>
    /// Subtracts results A from result B.
    /// </summary>
    /// <param name="a">The result of team A.</param>
    /// <param name="b">The result of team B.</param>
    /// <returns></returns>
    public static PointResult operator -(PointResult a, IOpponent<int> b)
    {
        if (a.Home < b.Home || a.Guest < b.Guest)
            throw new ArgumentException("Operation would lead to negative points value.", b.ToString());

        return new PointResult(a.Home - b.Home, a.Guest - b.Guest);
    }
}