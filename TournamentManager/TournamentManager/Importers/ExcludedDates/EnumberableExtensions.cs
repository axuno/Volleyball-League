namespace TournamentManager.Importers.ExcludedDates;

public static class EnumerableExtensions
{
    public static IEnumerable<(int First, int Last)> ConsecutiveRanges(this IEnumerable<int> source)
    {
        using var e = source.GetEnumerator();
        for (var more = e.MoveNext(); more;)
        {
            int first = e.Current, last = first, next;
            while ((more = e.MoveNext()) && (next = e.Current) > last && next - last == 1)
                last = next;
            yield return (first, last);
        }
    }

    public static IEnumerable<(DateTime First, DateTime Last)> ConsecutiveRanges(this IEnumerable<DateTime> source)
    {
        using var e = source.OrderBy(s => s).GetEnumerator();
        for (var more = e.MoveNext(); more;)
        {
            DateTime first = e.Current, last = first, next;
            while ((more = e.MoveNext()) && (next = e.Current) > last && (next - last).Days == 1)
                last = next;
            yield return (first, last);
        }
    }

    /// <summary>
    /// Creates a collection of <see cref="ValueTuple"/>s with consecutive <see cref="Axuno.Tools.GermanHoliday"/> date ranges having the same holiday name.
    /// </summary>
    /// <param name="source">An <see cref="IEnumerable{T}"/> collection of <see cref="Axuno.Tools.GermanHoliday"/>.</param>
    /// <returns>Returns an <see cref="IEnumerable{T}"/> of <see cref="ValueTuple"/>s with consecutive <see cref="Axuno.Tools.GermanHoliday"/> date ranges having the same holiday name.</returns>
    public static IEnumerable<(DateTime From, DateTime To, string Name)> ConsecutiveRanges(this IEnumerable<Axuno.Tools.GermanHoliday> source)
    {
        // ensure the required order the holiday list
        using var e = source.OrderBy(holiday => holiday.Date)
            .ThenBy(holiday => holiday.Name).GetEnumerator();

        for (var more = e.MoveNext(); more;)
        {
            if(e.Current is null) continue;

            var days = 1;
            Axuno.Tools.GermanHoliday first = e.Current, last = first, next;
            while ((more = e.MoveNext() && e.Current is not null) && 
                   (next = e.Current!).Date > last.Date && 
                   (ReferenceEquals(first, next) || (next.Date - first.Date).Days == days++ && next.Name == first.Name))
                last = next;
            yield return (first.Date, last.Date, first.Name);
        }
    }
}