namespace TournamentManager.Importers.ExcludeDates;
public interface IExcludeDateImporter
{
    /// <summary>
    /// Gets all dates to import within the specified <paramref name="fromToTimePeriod"/>.
    /// </summary>
    /// <param name="fromToTimePeriod"></param>
    /// <returns>All dates to import within the specified <paramref name="fromToTimePeriod"/>.</returns>
    IEnumerable<ExcludeDateRecord> Import(DateTimePeriod fromToTimePeriod);
}
