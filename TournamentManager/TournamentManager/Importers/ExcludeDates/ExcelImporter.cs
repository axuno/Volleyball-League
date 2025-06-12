using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace TournamentManager.Importers.ExcludeDates;

/// <summary>
/// Imports <see cref="ExcludeDateRecord"/>s from an Excel (.xlsx) workbook.
/// </summary>
public class ExcelImporter : IExcludeDateImporter
{
    private readonly string _xlPathAndFileName;
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<ExcelImporter> _logger;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="xlPathAndFileName"></param>
    /// <param name="timeZoneConverter"></param>
    /// <param name="logger"></param>
    public ExcelImporter(string xlPathAndFileName, Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, ILogger<ExcelImporter> logger)
    {
        ExcelPackage.License.SetNonCommercialOrganization("GitHub Repository axuno Volleyball-League");
        _xlPathAndFileName = xlPathAndFileName;
        _timeZoneConverter = timeZoneConverter;
        _logger = logger;
    }

    /// <summary>
    /// Imports excluded match dates from an Excel workbook. The first worksheet of the workbook will be used.
    /// 3 expected columns: <see cref="DateTime"/> DateFrom, <see cref="DateTime"/> DateTo, <see cref="string"/> Reason.
    /// </summary>
    /// <remarks>
    /// Excel date values are considered as local time and will be converted to UTC.
    /// A maximum of 1,000 rows will be imported.
    /// </remarks>
    /// <param name="fromToTimePeriod">The limits for dates, which will be imported.</param>
    /// <returns>
    /// Returns an <see cref="IEnumerable{T}" />of <see cref="ExcludeDateRecord"/>.
    /// </returns>
    public IEnumerable<ExcludeDateRecord> Import(DateTimePeriod fromToTimePeriod)
    {
        var xlFile = new FileInfo(_xlPathAndFileName);
        _logger.LogDebug("Opening Excel file '{ExcelFile}'", _xlPathAndFileName);
        using var package = new ExcelPackage(xlFile);

        var worksheet = package.Workbook.Worksheets.First();
        _logger.LogDebug("Using the first worksheet, '{WorksheetName}'", worksheet.Name);
        _logger.LogDebug("Date limits are {DateStart} - {DateEnd}", fromToTimePeriod.Start, fromToTimePeriod.End);
        var row = 0;

        while (true)
        {
            row++;
            if (row == 1 && worksheet.Cells[row, 1].Value is not DateTime)
            {
                _logger.LogDebug("First cell is not a date, assume existing headline row");
                continue; // may contain a headline row
            }

            if (!(worksheet.Cells[row, 1].Value is DateTime from && worksheet.Cells[row, 2].Value is DateTime to) ||
                row > 1000)
            {
                _logger.LogDebug("Import finished with worksheet row {RowNo}", row - 1);
                yield break;
            }

            // Swap if needed
            if (from > to) (from, to) = (to, from);

            // No time part means, the whole day should be excluded
            const int millisecondsOneSecond = 1000;
            if (from.TimeOfDay.TotalMilliseconds < millisecondsOneSecond && to.TimeOfDay.TotalMilliseconds < millisecondsOneSecond)
                // Add the time span until the end of the day
                to = to.AddDays(1).AddSeconds(-1);

            from = _timeZoneConverter.ToUtc(from);
            to = _timeZoneConverter.ToUtc(to);

            if (!fromToTimePeriod.Overlaps(new DateTimePeriod(from, to)))
            {
                _logger.LogDebug("UTC Dates {From} - {To} are out of limits", from, to);
                continue;
            }

            var reason = worksheet.Cells[row, 3].Value as string ?? string.Empty;
            yield return new ExcludeDateRecord(new DateTimePeriod(from, to), reason);
            _logger.LogDebug("Imported UTC {From} - {To} ({Reason})", from, to, reason);
        }
    }
}
