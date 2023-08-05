//
// Copyright Volleyball League Project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Globalization;
using League.Caching;
using League.Tests.TestComponents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using TournamentManager.DAL.TypedViewClasses;
using TournamentManager.MultiTenancy;

namespace League.Tests.Caching;

[TestFixture]
public class ReportSheetCacheTests
{
    private readonly ReportSheetCache _cache;
    private readonly ITenantContext _tenantContext;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ReportSheetCacheTests()
    {
        _webHostEnvironment = new HostingEnvironment {
            WebRootPath = Path.GetTempPath(), ContentRootPath
                // Because we use the Chromium installation in the demo web app
                = DirectoryLocator.GetTargetProjectPath(typeof(League.WebApp.WebAppStartup))
        };

        _tenantContext = new TenantContext
        {
            Identifier = "testorg"
        };

        var chromiumPath = new List<KeyValuePair<string, string?>>
            { new("Chromium:ExecutablePath", "Chromium-Win\\chrome.exe") };

        IServiceProvider services = UnitTestHelpers.GetReportSheetCacheServiceProvider(_tenantContext, _webHostEnvironment, chromiumPath);
        _cache = services.GetRequiredService<ReportSheetCache>();
    }

    [TestCase("en")]
    [TestCase("de")]
    public async Task CreateNewPdfInOutputPath(string c)
    {
        DeleteOutputFolder();
        var culture = CultureInfo.GetCultureInfo(c);
        using var switcher = new CultureSwitcher(culture, culture);

        var data = new MatchReportSheetRow
            { Id = 1234, ModifiedOn = new DateTime(2023, 03, 22, 12, 0, 0).ToUniversalTime() };

        var stream = await _cache.GetOrCreatePdf(data, "<html><body>Some text</body></html>", CancellationToken.None);
        var fileName = Path.GetFileName(((FileStream) stream).Name);
        await stream.DisposeAsync();

        Assert.That(fileName,
            Is.EqualTo(string.Format(ReportSheetCache.ReportSheetFilenameTemplate, _tenantContext.Identifier, data.Id,
                culture.TwoLetterISOLanguageName)));
    }

    [TestCase("en")]
    [TestCase("de")]
    public async Task ShouldReturnExistingPdfFromCache(string c)
    {
        DeleteOutputFolder();
        var culture = CultureInfo.GetCultureInfo(c);
        using var switcher = new CultureSwitcher(culture, culture);

        var data = new MatchReportSheetRow
            { Id = 1234, ModifiedOn = new DateTime(2023, 03, 22, 12, 0, 0).ToUniversalTime() };

        // (1) This should create the file in the cache
        var stream1 = await _cache.GetOrCreatePdf(data, "<html><body>Some text</body></html>", CancellationToken.None);
        var fileInfo1 = new FileInfo(Path.GetFileName(((FileStream) stream1).Name));
        await stream1.DisposeAsync();

        // (1) This should return the file from the cache
        var stream2 = await _cache.GetOrCreatePdf(data, "<html><body>Some text</body></html>", CancellationToken.None);
        var fileInfo2 = new FileInfo(Path.GetFileName(((FileStream) stream1).Name));
        await stream2.DisposeAsync();

        // Assert the file was not created again
        Assert.That(fileInfo1.CreationTimeUtc.Ticks, Is.EqualTo(fileInfo2.CreationTimeUtc.Ticks));
    }

    private void DeleteOutputFolder()
    {
        var outputFolder = Path.Combine(_webHostEnvironment.WebRootPath, ReportSheetCache.ReportSheetCacheFolder);

        // Delete folder in TempPath
        if (!Directory.Exists(outputFolder)) return;
        Directory.Delete(outputFolder, true);
    }
}
