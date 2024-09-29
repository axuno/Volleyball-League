//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using Microsoft.Extensions.Logging;

namespace TournamentManager.HtmlToPdfConverter;

#pragma warning disable CA3003  // reason: False positive due to CancellationToken in GetPdfDataBrowser

/// <summary>
/// The class to create PDF files from HTML content.
/// For converting HTML to PDF, it uses either a Browser command line or <see cref="PuppeteerSharp"/>.
/// </summary>
public class HtmlToPdfConverter : IDisposable
{
    private readonly string _pathToBrowser;
    private readonly string _tempFolder;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<HtmlToPdfConverter> _logger;
    private bool _isDisposing;

    /// <summary>
    /// Initializes a new instance of the <see cref="HtmlToPdfConverter"/> class.
    /// </summary>
    /// <param name="pathToBrowser">The path to the Browser executable.</param>
    /// <param name="tempPath">The folder where temporary files will be stored.</param>
    /// <param name="loggerFactory"></param>
    public HtmlToPdfConverter(string pathToBrowser, string tempPath, ILoggerFactory loggerFactory)
    {
        _pathToBrowser = pathToBrowser;
        EnsureTempFolder(tempPath);
        _tempFolder = CreateTempPathFolder(tempPath);
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<HtmlToPdfConverter>();
        UsePuppeteer = false;
        BrowserKind = BrowserKind.Chromium;
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use Puppeteer for generating the report sheet,
    /// instead of Browser command line.
    /// </summary>
    public bool UsePuppeteer { get; set; }

    /// <summary>
    /// The kind of browser to use for generating the PDF.
    /// </summary>
    public BrowserKind BrowserKind { get; set; }

    private void EnsureTempFolder(string tempFolder)
    {
        if (Directory.Exists(tempFolder)) return;

        Directory.CreateDirectory(tempFolder);
        _logger.LogDebug("Temporary path '{TempFolder}' created", tempFolder);
    }

    /// <summary>
    /// Creates a PDF file from the specified HTML content.
    /// </summary>
    /// <param name="html"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Stream"/> of the PDF file.</returns>
    public async Task<byte[]?> GeneratePdfData(string html, CancellationToken cancellationToken)
    {
        var pdfData = UsePuppeteer
            ? await GetPdfDataPuppeteer(html, false)
            : await GetPdfDataBrowser(html, cancellationToken);

        return pdfData;
    }

    /// <summary>
    /// Creates a PDF file from the specified HTML file.
    /// </summary>
    /// <param name="htmlFile"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>A <see cref="Stream"/> of the PDF file.</returns>
    public async Task<byte[]?> GeneratePdfData(FileInfo htmlFile, CancellationToken cancellationToken)
    {
        var pdfData = UsePuppeteer
            ? await GetPdfDataPuppeteer(htmlFile, cancellationToken)
            : await GetPdfDataBrowser(htmlFile, cancellationToken);

        return pdfData;
    }

    private async Task<byte[]?> GetPdfDataBrowser(string html, CancellationToken cancellationToken)
    {
        var tmpHtmlPath = await CreateHtmlFile(html, cancellationToken);
        return await GetPdfDataBrowser(new FileInfo(tmpHtmlPath), cancellationToken);
    }

    private async Task<byte[]?> GetPdfDataBrowser(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        try
        {
            var tmpPdfFile = await CreatePdfDataBrowser(fileInfo.FullName, cancellationToken);

            if (tmpPdfFile != null && File.Exists(tmpPdfFile))
                return await File.ReadAllBytesAsync(tmpPdfFile, cancellationToken);

            _logger.LogError("Error creating PDF file with Browser");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PDF file with Browser");
            return null;
        }
    }

    private async Task<byte[]?> GetPdfDataPuppeteer(FileInfo fileInfo, CancellationToken cancellationToken)
    {
        return await GetPdfDataPuppeteer(fileInfo.FullName, true);
    }

    private async Task<byte[]?> GetPdfDataPuppeteer(string fileOrHtmlContent, bool isFile)
    {
        var options = new PuppeteerSharp.LaunchOptions
        {
            Headless = true,
            Browser = (PuppeteerSharp.SupportedBrowser) BrowserKind,
            // Alternative: --use-cmd-decoder=validating 
            Args = new[]  // Chromium-based browsers require using a sandboxed browser for PDF generation, unless sandbox is disabled
                { "--no-sandbox", "--disable-gpu", "--allow-file-access-from-files", "--disable-extensions", "--use-cmd-decoder=passthrough" },
            ExecutablePath = _pathToBrowser,
            UserDataDir = _tempFolder,
            Timeout = 5000,
            ProtocolTimeout = 10000 // default is 180,000 - used for page.PdfDataAsync
        };

        // Use Puppeteer as a wrapper for the browser, which can generate PDF from HTML
        // Start command line arguments set by Puppeteer v20:
        // --allow-pre-commit-input --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-field-trial-config --disable-hang-monitor --disable-infobars --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-search-engine-choice-screen --disable-sync --enable-automation --enable-blink-features=IdleDetection --export-tagged-pdf --generate-pdf-document-outline --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --disable-features=Translate,AcceptCHFrame,MediaRouter,OptimizationHints,ProcessPerSiteUpToMainFrameThreshold --enable-features= --headless=new --hide-scrollbars --mute-audio about:blank --no-sandbox --disable-gpu --disable-extensions --use-cmd-decoder=passthrough --remote-debugging-port=0 --user-data-dir="C:\Users\xyz\AppData\Local\Temp\yk1fjkgt.phb"
        await using var browser = await PuppeteerSharp.Puppeteer.LaunchAsync(options, _loggerFactory).ConfigureAwait(false);
        await using var page = await browser.NewPageAsync().ConfigureAwait(false);

        if (isFile)
            await page.GoToAsync(new Uri(fileOrHtmlContent).AbsoluteUri);
        else
            await page.SetContentAsync(fileOrHtmlContent);

        await page.EvaluateExpressionHandleAsync("document.fonts.ready"); // Wait for fonts to be loaded. Omitting this might result in no text rendered in pdf.

        try
        {
            return await page.PdfDataAsync(new PuppeteerSharp.PdfOptions
            { Scale = 1.0M, Format = PuppeteerSharp.Media.PaperFormat.A4 }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PDF file with Puppeteer");
            return null;
        }
    }

    private async Task<string?> CreatePdfDataBrowser(string htmlFile, CancellationToken cancellationToken)
    {
        // Temporary file for the PDF stream from the Browser
        // Note: non-existing file is handled in MovePdfToCache
        var pdfFile = Path.Combine(_tempFolder, Path.GetRandomFileName() + ".pdf");

        // Note: --timeout ={timeout.TotalMilliseconds} as Browser argument does not work
        var timeout = TimeSpan.FromMilliseconds(5000);

        // Run the Browser
        // Command line switches overview: https://kapeli.com/cheat_sheets/Chromium_Command_Line_Switches.docset/Contents/Resources/Documents/index
        // or better https://peter.sh/experiments/chromium-command-line-switches/
        var startInfo = new System.Diagnostics.ProcessStartInfo(_pathToBrowser,
                $"--allow-pre-commit-input --allow-file-access-from-files --disable-background-networking --disable-background-timer-throttling --disable-backgrounding-occluded-windows --disable-breakpad --disable-client-side-phishing-detection --disable-component-extensions-with-background-pages --disable-component-update --disable-default-apps --disable-dev-shm-usage --disable-extensions --disable-features=Translate,BackForwardCache,AcceptCHFrame,MediaRouter,OptimizationHints --disable-hang-monitor --disable-ipc-flooding-protection --disable-popup-blocking --disable-prompt-on-repost --disable-renderer-backgrounding --disable-sync --enable-automation --enable-blink-features=IdleDetection --enable-features=NetworkServiceInProcess2 --export-tagged-pdf --force-color-profile=srgb --metrics-recording-only --no-first-run --password-store=basic --use-mock-keychain --headless --hide-scrollbars --mute-audio --no-sandbox --disable-gpu --use-cmd-decoder=passthrough --no-margins --no-pdf-header-footer --print-to-pdf={pdfFile} {htmlFile}")
        { CreateNoWindow = true, UseShellExecute = false };
        using var proc = System.Diagnostics.Process.Start(startInfo);

        if (proc == null)
        {
            _logger.LogError("Process '{PathToBrowser}' could not be started.", _pathToBrowser);
            return pdfFile;
        }

        var processTask = proc.WaitForExitAsync(cancellationToken);

        await Task.WhenAny(processTask, Task.Delay(timeout, cancellationToken));

        if (processTask.IsCompleted) return pdfFile;

        proc.Kill(true);
        return null;
    }

    private async Task<string> CreateHtmlFile(string html, CancellationToken cancellationToken)
    {
        var htmlFile = Path.Combine(_tempFolder, Path.GetRandomFileName() + ".html"); // extension must be "html"
        await File.WriteAllTextAsync(htmlFile, html, cancellationToken);
        return htmlFile;
    }

    private static string CreateTempPathFolder(string tempPath)
    {
        // Create child folder in TempPath
        var tempFolder = Path.Combine(tempPath, Path.GetRandomFileName());
        if (!Directory.Exists(tempFolder)) Directory.CreateDirectory(tempFolder);
        return tempFolder;
    }

    private void DeleteTempPathFolder()
    {
        // Delete folder in TempPath
        if (!Directory.Exists(_tempFolder)) return;
        Directory.Delete(_tempFolder, true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposing || !disposing) return;
        _isDisposing = true;

        try
        {
            DeleteTempPathFolder();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing {HtmlToPdfConverter}", nameof(HtmlToPdfConverter));
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}

#pragma warning restore CA3003  // reason: False positive due to CancellationToken in GetPdfDataBrowser
