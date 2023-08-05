using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using TournamentManager.DAL.EntityClasses;

namespace TournamentManager.Importers.ExcludedDates;

/// <summary>
/// Imports <see cref="ExcludeMatchDateEntity"/>s from an Excel (.xlsx) workbook.
/// </summary>
public class ExcelImporter
{
    private readonly Axuno.Tools.DateAndTime.TimeZoneConverter _timeZoneConverter;
    private readonly ILogger<ExcelImporter> _logger;

    /// <summary>
    /// CTOR.
    /// </summary>
    /// <param name="timeZoneConverter"></param>
    /// <param name="logger"></param>
    public ExcelImporter(Axuno.Tools.DateAndTime.TimeZoneConverter timeZoneConverter, ILogger<ExcelImporter> logger)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
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
    /// <param name="xlPathAndFileName">The path to the Excel file.</param>
    /// <param name="dateLimits">The limits for dates, which will be imported.</param>
    /// <returns>
    /// Returns an <see cref="IEnumerable{T}" />of <see cref="ExcludeMatchDateEntity"/>.
    /// <see cref="ExcludeMatchDateEntity.TournamentId"/>, <see cref="ExcludeMatchDateEntity.RoundId"/> and <see cref="ExcludeMatchDateEntity.TeamId"/> will not be set.
    /// </returns>
    public IEnumerable<ExcludeMatchDateEntity> Import(string xlPathAndFileName, DateTimePeriod dateLimits)
    {
        var xlFile = new FileInfo(xlPathAndFileName);
        _logger.LogDebug("Opening Excel file '{excelFile}'", xlPathAndFileName);
        using var package = new ExcelPackage(xlFile);
            
        var worksheet = package.Workbook.Worksheets.First();
        _logger.LogDebug("Using the first worksheet, '{worksheetName}'", worksheet.Name);
        _logger.LogDebug("Date limits are {dateStart} - {dateEnd}", dateLimits.Start, dateLimits.End);
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
                _logger.LogDebug("Import finished with worksheet row {rowNo}", row - 1);
                yield break;
            }
                    

            from = _timeZoneConverter.ToUtc(from.Date);
            to = _timeZoneConverter.ToUtc(to.Date);

            if (!dateLimits.Overlaps(new DateTimePeriod(from, to)))
            {
                _logger.LogDebug("UTC Dates {from} - {to} are out of limits", from, to);
                continue;
            }

            var reason = worksheet.Cells[row, 3].Value as string ?? string.Empty;
            yield return CreateEntity((from, to, reason));
            _logger.LogDebug("Imported UTC {from} - {to} ({reason})", from, to, reason);
        }
    }

    /// <summary>
    /// Creates an <see cref="ExcludeMatchDateEntity"/>.
    /// From and To dates are silently swapped if necessary.
    /// </summary>
    /// <param name="excluded"> <see cref="ValueTuple"/> with from date, to date and name of the exclusion.</param>
    /// <exception cref="ArgumentException">Throws for <see cref="DateTime.MinValue"/> date values and <see cref="string.IsNullOrWhiteSpace"/> name.</exception>
    private static ExcludeMatchDateEntity CreateEntity((DateTime From, DateTime To, string Name) excluded)
    {
        var excludeMatchDate = new ExcludeMatchDateEntity();

        if (excluded.From == DateTime.MinValue || excluded.To == DateTime.MinValue ||
            string.IsNullOrWhiteSpace(excluded.Name))
        {
            throw new ArgumentException($"Could not create {nameof(ExcludeMatchDateEntity)} with From={excluded.From.Date}, To={excluded.To.Date}, Name={excluded.Name}");
        }

        // Swap if necessary
        if (excluded.From > excluded.To) (excluded.From, excluded.To) = (excluded.To, excluded.From);
            
        excludeMatchDate.DateFrom = excluded.From;
        excludeMatchDate.DateTo = excluded.To;
        excludeMatchDate.Reason = excluded.Name;

        return excludeMatchDate;
    }
}