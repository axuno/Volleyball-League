using Axuno.BackgroundTask;
using NLog;
using NLog.Web;
using TournamentManager.MultiTenancy;

namespace League.WebApp;

public class Program
{
    /// <summary>
    /// Program cannot be made static.
    /// </summary>
    protected Program(){}

    public static async Task Main(string[] args)
    {
        // NLog: set up the logger first to catch all errors
        var currentDir = Directory.GetCurrentDirectory();
        var logConfigFile = Path.Combine(currentDir, LeagueStartup.ConfigurationFolder, "NLog.Internal.config");
        var logger = LogManager.Setup()
            .LoadConfigurationFromFile(logConfigFile)
            .GetCurrentClassLogger();
        // Allows using <target name="file" xsi:type="File" fileName = "${var:logDirectory}logfile.log"... >
        LogManager.Configuration?.Variables["logDirectory"] = currentDir + Path.DirectorySeparatorChar;
            
        try
        {
            logger.Trace($"Configuration of {nameof(WebApplicationBuilder)} starting.");
            logger.Info($"This app runs as {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process.");

            var builder = SetupBuilder(args);

            #region ** Setup logging **
            
            // Enable NLog as logging provider for Microsoft.Extension.Logging
            logConfigFile = Path.Combine(currentDir, LeagueStartup.ConfigurationFolder, $"NLog.{builder.Environment.EnvironmentName}.config");
            await using var logFactory = LogManager.Setup()
                .LoadConfigurationFromFile(logConfigFile)
                .LogFactory;
                
            var nLogOptions = new NLogAspNetCoreOptions { AutoShutdown = true, IncludeScopes = true };

            builder.Logging.ClearProviders();
            builder.Logging.AddNLogWeb(logFactory, nLogOptions);

            // NLog: Setup NLog Dependency injection using the configuration from above
            builder.Host.UseNLog();

            #endregion

            LeagueStartup.ConfigureServices(builder);
            WebAppStartup.ConfigureServices(builder);

            var app = builder.Build();

            LeagueStartup.Configure(app);
            LeagueStartup.InitializeRankingAndCharts(app.Services.GetRequiredService<TenantStore>(), app.Services.GetRequiredService<IBackgroundQueue>(), app.Services);
            WebAppStartup.Configure(app);

            await app.RunAsync();
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Application stopped after Exception.");
        }
    }

    public static WebApplicationBuilder SetupBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name, // don't use Assembly.Fullname
            WebRootPath = "wwwroot" // Note: WebRootPath is relative to ContentRootPath.
            // Note: ContentRootPath is detected by the framework.
            // If set explicitly as WebApplicationOptions, WebApplicationFactory in unit tests does not override it.
        });

        // Use static web assets from League (and other referenced projects or packages)
        // Note: When the app is published, the static asset files get copied up into the wwwroot folder.
        builder.WebHost.UseStaticWebAssets();

        var absoluteConfigurationPath = Path.Combine(builder.Environment.ContentRootPath,
            LeagueStartup.ConfigurationFolder);

        builder.Configuration.SetBasePath(absoluteConfigurationPath)
            .AddJsonFile("AppSettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"AppSettings.{builder.Environment.EnvironmentName}.json",
                optional: true, reloadOnChange: true)
            .AddJsonFile(@"Credentials.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"Credentials.{builder.Environment.EnvironmentName}.json",
                optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
        
        return builder;
    }
}
