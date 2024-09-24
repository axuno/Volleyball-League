//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Caching;

#pragma warning disable S2083   // reason: False positive due to CancellationToken in GetOrCreatePdf
#pragma warning disable CA3003  // reason: False positive due to CancellationToken in GetOrCreatePdf

/// <summary>
/// Represents a cache for report sheets.
/// </summary>
public class ReportSheetCache
{
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _pathToChromium;
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
        _pathToChromium = Path.Combine(webHostEnvironment.ContentRootPath, configuration["Chromium:ExecutablePath"] ?? string.Empty);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ReportSheetCache>();
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use Puppeteer for generating the report sheet,
    /// instead of Chromium command line.
    /// </summary>
    public bool UsePuppeteer { get; set; } = false;

    private void EnsureCacheFolder()
    {
        var cacheFolder = Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCacheFolder);
        if (!Directory.Exists(cacheFolder))
        {
            Directory.CreateDirectory(cacheFolder);
            _logger.LogDebug("Cache folder '{CacheFolder}' created", cacheFolder);
        }
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

            cacheFile = UsePuppeteer
                ? await GetReportSheetPuppeteer(data.Id, html, cancellationToken)
                : await GetReportSheetChromium(data.Id, html, cancellationToken);

            if (cacheFile == null) return Stream.Null;
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

    private async Task<string?> GetReportSheetChromium(long matchId, string html, CancellationToken cancellationToken)
    {
        // Create folder in TempPath
        var tempFolder = CreateTempPathFolder();
            
        // Temporary file with HTML content - extension must be ".html"!
        var htmlUri = await CreateHtmlFile(html, tempFolder, cancellationToken);

        var pdfFile = await CreateReportSheetPdfChromium(tempFolder, htmlUri);

        var cacheFile = MovePdfToCache(pdfFile, matchId);

        DeleteTempPathFolder(tempFolder);

        return cacheFile;
    }

    private string? MovePdfToCache(string pdfFile, long matchId)
    {
        if (!File.Exists(pdfFile)) return null;

        var fullPath = GetPathToCacheFile(matchId);
        try
        {
            // may throw UnauthorizedAccessException on production server
            File.Move(pdfFile, fullPath, true);
        }
        catch
        {
            File.Copy(pdfFile, fullPath, true);    
        }

        return fullPath;
    }

    private string GetPathToCacheFile(long matchId)
    {
        var fileName = string.Format(ReportSheetFilenameTemplate, _tenantContext.Identifier, matchId,
            Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName);

        return Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCacheFolder, fileName);
    }

    private async Task<string?> GetReportSheetPuppeteer(long matchId, string html, CancellationToken cancellationToken)
    {
        var options = new PuppeteerSharp.LaunchOptions
        {
            Headless = true,
            Browser = PuppeteerSharp.SupportedBrowser.Chromium,
            // Alternative: --use-cmd-decoder=validating 
            Args = new[]  // Chromium requires using a sandboxed browser for PDF generation, unless sandbox is disabled
                { "--no-sandbox", "--disable-gpu", "--disable-extensions", "--use-cmd-decoder=passthrough" },
            ExecutablePath = _pathToChromium,
            Timeout = 5000,
            ProtocolTimeout = 10000 // default is 180,000
        };
        // Use Puppeteer as a wrapper for the browser, which can generate PDF from HTML
        // Start command line arguments set by Puppeteer v20:
        // --allow-pre-commit-input --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-field-trial-config --disable-hang-monitor --disable-infobars --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-search-engine-choice-screen --disable-sync --enable-automation --enable-blink-features=IdleDetection --export-tagged-pdf --generate-pdf-document-outline --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --disable-features=Translate,AcceptCHFrame,MediaRouter,OptimizationHints,ProcessPerSiteUpToMainFrameThreshold --enable-features= --headless=new --hide-scrollbars --mute-audio about:blank --no-sandbox --disable-gpu --disable-extensions --use-cmd-decoder=passthrough --remote-debugging-port=0 --user-data-dir="C:\Users\xyz\AppData\Local\Temp\yk1fjkgt.phb"
        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options, _loggerFactory).ConfigureAwait(false);
        await using var page = await browser.NewPageAsync().ConfigureAwait(false);

        await page.SetContentAsync(html); // Bootstrap 5 is loaded from CDN
        await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.

        var fullPath = GetPathToCacheFile(matchId);
        try
        {
            // page.PdfDataAsync times out after 180,000ms (3 minutes)
            var bytes = await page.PdfDataAsync(new PuppeteerSharp.PdfOptions
                { Scale = 1.0M, Format = PuppeteerSharp.Media.PaperFormat.A4 }).ConfigureAwait(false);

            await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error creating PDF file with Puppeteer for match ID '{MatchId}'", matchId);
            await File.WriteAllBytesAsync(fullPath, Array.Empty<byte>(), cancellationToken);
        }

        return fullPath;
    }

    private async Task<string> CreateReportSheetPdfChromium(string tempFolder, string htmlUri)
    {
        // Temporary file for the PDF stream form Chromium
        var pdfFile = Path.Combine(tempFolder, Path.GetRandomFileName() + ".pdf");

        // Run Chromium
        // Command line switches overview: https://kapeli.com/cheat_sheets/Chromium_Command_Line_Switches.docset/Contents/Resources/Documents/index
        // or better https://peter.sh/experiments/chromium-command-line-switches/
        var startInfo = new System.Diagnostics.ProcessStartInfo(_pathToChromium,
                $"--allow-pre-commit-input --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-features=Translate,BackForwardCache,AcceptCHFrame,MediaRouter,OptimizationHints --disable-hang-monitor --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-sync --enable-automation --enable-blink-features=IdleDetection --enable-features=NetworkServiceInProcess2 --export-tagged-pdf --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --headless --hide-scrollbars --mute-audio --no-sandbox --disable-gpu --use-cmd-decoder=passthrough --no-margins --user-data-dir={tempFolder} --no-pdf-header-footer --print-to-pdf={pdfFile} {htmlUri}")
            { CreateNoWindow = true, UseShellExecute = false };
        var proc = System.Diagnostics.Process.Start(startInfo);

        if (proc is null)
        {
            _logger.LogError("Process '{PathToChromium}' could not be started.", _pathToChromium);
        }

        const int timeout = 8000;
        var timePassed = 0;
        while (proc is { HasExited: false })
        {
            timePassed += 100;
            await Task.Delay(100, default);
            if (timePassed < timeout) continue;

            proc.Kill(true);
            throw new OperationCanceledException($"Chromium timed out after {timeout}ms.");
        }

        // non-existing file is handled in MovePdfToCache
        return pdfFile;
    }

    private static async Task<string> CreateHtmlFile(string html, string tempFolder, CancellationToken cancellationToken)
    {
        var htmlFile = Path.Combine(tempFolder, Path.GetRandomFileName() + ".html");
        await File.WriteAllTextAsync(htmlFile, html, cancellationToken);
        return new Uri(htmlFile).AbsoluteUri;
    }

    private static string CreateTempPathFolder()
    {
        // Create folder in TempPath
        var tempFolder = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }

    private static void DeleteTempPathFolder(string path)
    {
        // Delete folder in TempPath
        if (!Directory.Exists(path)) return;
        try
        {
            Directory.Delete(path, true);
        }
        catch
        {
            // Best effort when trying to remove
        }
    }
}
#pragma warning restore CA3003
#pragma warning restore S2083
