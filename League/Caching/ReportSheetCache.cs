//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Caching;

#pragma warning disable CA3003  // reason: False positive due to CancellationToken in GetOrCreatePdf

/// <summary>
/// Represents a cache for report sheets.
/// </summary>
public class ReportSheetCache
{
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _pathToBrowser;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<ReportSheetCache> _logger;

    /// <summary>
    /// Folder name for the report sheet cache
    /// relative to <see cref="IWebHostEnvironment.WebRootPath"/>.
    /// </summary>
    public const string ReportSheetCacheFolder = "report-sheets";

    /// <summary>
    /// Template for chart images. {0}: tenant key, {1}: match ID, {2}: language
    /// </summary>
    public const string ReportSheetFilenameTemplate = "Sheet_{0}_{1}_{2}.pdf";

    /// <summary>
    /// Initializes a new instance of the <see cref="ReportSheetCache"/> class.
    /// </summary>
    /// <param name="tenantContext"></param>
    /// <param name="configuration"></param>
    /// <param name="webHostEnvironment"></param>
    /// <param name="loggerFactory"></param>
    public ReportSheetCache(ITenantContext tenantContext, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ILoggerFactory loggerFactory)
    {
        _tenantContext = tenantContext;
        _webHostEnvironment = webHostEnvironment;
        _pathToBrowser = Path.Combine(webHostEnvironment.ContentRootPath, configuration["Browser:ExecutablePath"] ?? string.Empty);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ReportSheetCache>();
        UsePuppeteer = false;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use Puppeteer for generating the report sheet,
    /// instead of Browser command line.
    /// </summary>
    public bool UsePuppeteer { get; set; }

    private void EnsureCacheFolder()
    {
        var cacheFolder = Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCacheFolder);
        if (Directory.Exists(cacheFolder)) return;

        Directory.CreateDirectory(cacheFolder);
        _logger.LogDebug("Cache folder '{CacheFolder}' created", cacheFolder);
    }

    /// <summary>
    /// Gets or creates a PDF file for a match report sheet.
    /// </summary>
    /// <param name="data"></param>
    /// <param name="html"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Stream"/> of the PDF file.</returns>
    public async Task<Stream> GetOrCreatePdf(MatchReportSheetRow data, string html, CancellationToken cancellationToken)
    {
        EnsureCacheFolder();

        var cacheFile = GetPathToCacheFile(data.Id);

        if (!File.Exists(cacheFile) || IsOutdated(cacheFile, data.ModifiedOn))
        {
            _logger.LogDebug("Create new match report for tenant '{Tenant}', match '{MatchId}'", _tenantContext.Identifier, data.Id);

            using var converter = new HtmlToPdfConverter(_pathToBrowser, CreateTempPathFolder(), _loggerFactory)
                { UsePuppeteer = UsePuppeteer };

            var pdfData = await converter.GeneratePdfData(html, cancellationToken);

            if (pdfData != null)
            {
                await File.WriteAllBytesAsync(cacheFile, pdfData, cancellationToken);
                return File.OpenRead(cacheFile);
            }

            _logger.LogWarning("No PDF data created for match ID '{MatchId}'", data.Id);
            return Stream.Null;
        }

        _logger.LogDebug("Read match report from cache for tenant '{Tenant}', match '{MatchId}'", _tenantContext.Identifier, data.Id);
        var stream = File.OpenRead(cacheFile);
        return stream;
    }

    private static bool IsOutdated(string cacheFile, DateTime dataModifiedOn)
    {
        var fi = new FileInfo(cacheFile);
        return !fi.Exists || fi.LastWriteTimeUtc < dataModifiedOn; // Database dates are in UTC
    }

    private string GetPathToCacheFile(long matchId)
    {
        var fileName = string.Format(ReportSheetFilenameTemplate, _tenantContext.Identifier, matchId,
            Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        return Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCacheFolder, fileName);
    }

    private static string CreateTempPathFolder()
    {
        // Create folder in TempPath
        var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }
}
#pragma warning restore CA3003

