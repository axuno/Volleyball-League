using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.Importers.ExcludeDates;

/// <summary>
/// Records used by <see cref="IExcludeDateImporter"/>s to store excluded match dates.
/// </summary>
/// <param name="Period"></param>
/// <param name="Reason"></param>
public readonly record struct ExcludeDateRecord(DateTimePeriod Period, string Reason)
{
    /// <summary>
    /// Creates an <see cref="ExcludeMatchDateEntity"/>.
    /// </summary>
    /// <exception cref="ArgumentException">Throws for <see cref="DateTime.MinValue"/> date values and <see cref="string.IsNullOrWhiteSpace"/> name.</exception>
    public ExcludeMatchDateEntity ToExcludeMatchDateEntity()
    {
        var excludeMatchDate = new ExcludeMatchDateEntity();

        if (Period.Start == DateTime.MinValue || Period.End == DateTime.MinValue ||
            string.IsNullOrWhiteSpace(Reason))
            throw new ArgumentException($"Could not create {nameof(ExcludeDateRecord)} with From={Period.Start?.Date}, To={Period.End?.Date}, Name={Reason}");

        excludeMatchDate.DateFrom = (DateTime) Period.Start!;
        excludeMatchDate.DateTo = (DateTime) Period.End!;
        excludeMatchDate.Reason = Reason;

        return excludeMatchDate;
    }

    public override string ToString()
    {
        return $"{Period.Start:yyyy-MM-dd HH:mm:ss} - {Period.End:yyyy-MM-dd HH:mm:ss}: {Reason}";
    }
}
