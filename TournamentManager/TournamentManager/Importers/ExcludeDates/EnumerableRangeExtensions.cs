namespace TournamentManager.Importers.ExcludeDates;

/// <summary>
/// The <see cref="EnumerableRangeExtensions"/> class provides extension methods for <see cref="IEnumerable{T}"/> collections.
/// It is used to create a collection of <see cref="ValueTuple"/>s with consecutive value ranges.
/// </summary>
public static class EnumerableRangeExtensions
{
    /// <summary>
    /// Creates a collection of <see cref="ValueTuple"/>s with consecutive <see langword="int"/> ranges.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>A collection of <see cref="ValueTuple"/>s with consecutive <see langword="int"/> ranges.</returns>
    public static IEnumerable<(int First, int Last)> ConsecutiveRanges(this IEnumerable<int> source)
    {
        return source.Select(i => (long)i).ConsecutiveRanges().Select(range => ((int)range.First, (int)range.Last));
    }

    /// <summary>
    /// Creates a collection of <see cref="ValueTuple"/>s with consecutive <see langword="long"/> ranges.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>A collection of <see cref="ValueTuple"/>s with consecutive <see langword="long"/> ranges.</returns>
    public static IEnumerable<(long First, long Last)> ConsecutiveRanges(this IEnumerable<long> source)
    {
        // Order the source numbers and get an enumerator
        using var enumerator = source.OrderBy(number => number).GetEnumerator();

        // Move to the first element
        if (!enumerator.MoveNext())
        {
            yield break; // If the source is empty, exit the method
        }

        // Initialize the first and last numbers of the current range
        var first = enumerator.Current;
        var last = first;

        // Iterate through the remaining elements
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;

            // Check if the current number is consecutive to the last number
            if (current - last != 1)
            {
                yield return (first, last);
                first = current;
            }

            last = current; // Extend the current range
        }

        yield return (first, last);
    }

    /// <summary>
    /// Creates a collection of <see cref="ValueTuple"/>s with consecutive <see langword="DateOnly"/> ranges.
    /// </summary>
    /// <param name="source"></param>
    /// <returns>A collection of <see cref="ValueTuple"/>s with consecutive <see langword="long"/> ranges.</returns>
    public static IEnumerable<(DateOnly First, DateOnly Last)> ConsecutiveRanges(this IEnumerable<DateOnly> source)
    {
        // Order the source dates and get an enumerator
        using var enumerator = source.OrderBy(date => date).GetEnumerator();

        // Move to the first element
        if (!enumerator.MoveNext())
        {
            yield break;
        }

        // Initialize the first and last dates of the current range
        var first = enumerator.Current;
        var last = first;

        // Iterate through the remaining elements
        while (enumerator.MoveNext())
        {
            var current = enumerator.Current;

            // Check if the current date is consecutive to the last date
            if (current.DayNumber - last.DayNumber != 1)
            {
                yield return (first, last);
                first = current;
            }

            last = current; // Extend the current range
        }

        yield return (first, last);
    }

    /// <summary>
    /// Creates a collection of <see cref="ValueTuple"/>s with consecutive <see cref="Axuno.Tools.GermanHoliday"/> date ranges having the same holiday name.
    /// Only the date part of the <see cref="DateTime"/> is considered.
    /// </summary>
    /// <param name="source">An <see cref="IEnumerable{T}"/> collection of <see cref="Axuno.Tools.GermanHoliday"/>.</param>
    /// <returns>Returns an <see cref="IEnumerable{T}"/> of <see cref="ValueTuple"/>s with consecutive <see cref="Axuno.Tools.GermanHoliday"/> date ranges having the same holiday name.</returns>
    public static IEnumerable<(DateTime From, DateTime To, string Name)> ConsecutiveRanges(this IEnumerable<Axuno.Tools.GermanHoliday> source)
    {
        // ensure the required order the holiday list
        using var e = source.OrderBy(holiday => holiday.Date)
            .ThenBy(holiday => holiday.Name).GetEnumerator();

        DateTime? first = null, last = null;
        var name = string.Empty;

        while (e.MoveNext())
        {
            var date = e.Current;

            if (first is null)
            {
                first = date.Date;
                last = date.Date;
                name = date.Name;
            }
            else if (date.Date > last!.Value.Date.AddDays(1) || date.Name != name)
            {
                yield return (first.Value.Date, last.Value.Date, name);
                first = date.Date;
                last = date.Date;
                name = date.Name;
            }
            else
            {
                last = date.Date;
            }
        }

        if (first is not null)
        {
            yield return (first.Value, last!.Value, name);
        }
    }
}
