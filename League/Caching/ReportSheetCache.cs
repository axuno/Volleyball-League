//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using OxyPlot;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Caching;
public class ReportSheetCache
{
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly string _pathToChromium;
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

    public ReportSheetCache(ITenantContext tenantContext, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ILogger<ReportSheetCache> logger)
    {
        _tenantContext = tenantContext;
        _webHostEnvironment = webHostEnvironment;
        _pathToChromium = Path.Combine(webHostEnvironment.ContentRootPath, configuration["Chromium:ExecutablePath"] ?? string.Empty);
        _logger = logger;
    }

    private void EnsureCacheFolder()
    {
        var cacheFolder = Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCacheFolder);
        if (!Directory.Exists(cacheFolder))
        {
            _logger.LogDebug("Cache folder '{CacheFolder}' created", cacheFolder);
            Directory.CreateDirectory(cacheFolder);
        }
    }

    public async Task<Stream> GetOrCreatePdf(MatchReportSheetRow data, string html, CancellationToken cancellationToken)
    {
        EnsureCacheFolder();
        var model =
            (await _tenantContext.DbContext.AppDb.MatchRepository.GetMatchReportSheetAsync(
                _tenantContext.TournamentContext.MatchPlanTournamentId, data.Id, cancellationToken));

        if (model is null) return Stream.Null;

        var matchId = model.Id;

        var cacheFile = GetPathToCacheFile(matchId);

        if (!File.Exists(cacheFile) || IsOutdated(cacheFile, data.ModifiedOn))
        {
            _logger.LogDebug("Create new match report for tenant '{Tenant}', match '{MatchId}'", _tenantContext.Identifier, matchId);
            cacheFile = await GetReportSheetChromium(matchId, html, cancellationToken);
            // GetReportSheetPuppeteer() still throws on production server
            if (cacheFile == null) return Stream.Null;
        }

        _logger.LogDebug("Read match report from cache for tenant '{Tenant}', match '{MatchId}'", _tenantContext.Identifier, matchId);
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
            Product = PuppeteerSharp.Product.Chrome,
            // Alternative: --use-cmd-decoder=validating 
            Args = new[]
                { "--no-sandbox", "--disable-gpu", "--disable-extensions", "--use-cmd-decoder=passthrough" },
            ExecutablePath = _pathToChromium,
            Timeout = 5000
        };
        // Use Puppeteer as a wrapper for the browser, which can generate PDF from HTML
        // Start command line arguments set by Puppeteer:
        // --allow-pre-commit-input --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-features=Translate,BackForwardCache,AcceptCHFrame,MediaRouter,OptimizationHints --disable-hang-monitor --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-sync --enable-automation --enable-blink-features=IdleDetection --enable-features=NetworkServiceInProcess2 --export-tagged-pdf --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --headless
        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options).ConfigureAwait(false);
        await using var page = await browser.NewPageAsync().ConfigureAwait(false);

        await page.SetContentAsync(html); // Bootstrap 5 is loaded from CDN
        await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.

        // Todo: This part works on the development machine, but throws on the production web server
        /*
2023-03-21 22:23:44.4533||FATAL|League.Controllers.Match|ReportSheet failed for match ID '3188' PuppeteerSharp.MessageException: Protocol error (IO.read): Read failed
   at PuppeteerSharp.CDPSession.SendAsync(String method, Object args, Boolean waitForCallback) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\CDPSession.cs:line 94
   at PuppeteerSharp.CDPSession.SendAsync[T](String method, Object args) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\CDPSession.cs:line 55
   at PuppeteerSharp.Helpers.ProtocolStreamReader.ReadProtocolStreamByteAsync(CDPSession client, String handle, String path) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\Helpers\ProtocolStreamReader.cs:line 63
   at PuppeteerSharp.Page.PdfInternalAsync(String file, PdfOptions options) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\Page.cs:line 1175
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetOrCreatePdf(MatchReportSheetRow data, String html, CancellationToken cancellationToken)
   at League.Controllers.Match.ReportSheet(Int64 id, CancellationToken cancellationToken)    at PuppeteerSharp.CDPSession.SendAsync(String method, Object args, Boolean waitForCallback) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\CDPSession.cs:line 94
   at PuppeteerSharp.CDPSession.SendAsync[T](String method, Object args) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\CDPSession.cs:line 55
   at PuppeteerSharp.Helpers.ProtocolStreamReader.ReadProtocolStreamByteAsync(CDPSession client, String handle, String path) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\Helpers\ProtocolStreamReader.cs:line 63
   at PuppeteerSharp.Page.PdfInternalAsync(String file, PdfOptions options) in C:\projects\puppeteer-sharp\lib\PuppeteerSharp\Page.cs:line 1175
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetReportSheetPuppeteer(Int64 matchId, String html, CancellationToken cancellationToken)
   at League.Caching.ReportSheetCache.GetOrCreatePdf(MatchReportSheetRow data, String html, CancellationToken cancellationToken)
   at League.Controllers.Match.ReportSheet(Int64 id, CancellationToken cancellationToken)
   |url: https://volleyball-liga.de/augsburg/match/reportsheet/3188|action: ReportSheet
         */
        var fullPath = GetPathToCacheFile(matchId);

        var bytes = await page.PdfDataAsync(new PuppeteerSharp.PdfOptions
            { Scale = 1.0M, Format = PuppeteerSharp.Media.PaperFormat.A4 }).ConfigureAwait(false);

        await File.WriteAllBytesAsync(fullPath, bytes, cancellationToken);

        return fullPath;
    }

    private async Task<string> CreateReportSheetPdfChromium(string tempFolder, string htmlUri)
    {
        // Temporary file for the PDF stream form Chromium
        var pdfFile = Path.Combine(tempFolder, Path.GetRandomFileName() + ".pdf");

        // Run Chromium
        // Command line switches overview: https://kapeli.com/cheat_sheets/Chromium_Command_Line_Switches.docset/Contents/Resources/Documents/index
        var startInfo = new System.Diagnostics.ProcessStartInfo(_pathToChromium,
                $"--allow-pre-commit-input --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-features=Translate,BackForwardCache,AcceptCHFrame,MediaRouter,OptimizationHints --disable-hang-monitor --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-sync --enable-automation --enable-blink-features=IdleDetection --enable-features=NetworkServiceInProcess2 --export-tagged-pdf --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --headless --hide-scrollbars --mute-audio --no-sandbox --disable-gpu --use-cmd-decoder=passthrough --no-margins --user-data-dir={tempFolder} --print-to-pdf={pdfFile} {htmlUri}")
            { CreateNoWindow = true, UseShellExecute = false };
        var proc = System.Diagnostics.Process.Start(startInfo);

        if (proc is null)
        {
            //_logger.LogCritical("Process '{PathToChromium}' could not be started.", _pathToChromium);
            throw new InvalidOperationException($"Process '{_pathToChromium}' could not be started.");
        }

        const int timeout = 8000;
        var timePassed = 0;
        while (!proc.HasExited)
        {
            timePassed += 100;
            await Task.Delay(100, default);
            if (timePassed < timeout) continue;

            proc.Kill(true);
            throw new OperationCanceledException($"Chromium timed out after {timeout}ms.");
        }

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
