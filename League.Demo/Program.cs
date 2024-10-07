using TournamentManager.MultiTenancy;
using Axuno.BackgroundTask;
using NLog;
using NLog.Web;

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
        // Allows for <target name="file" xsi:type="File" fileName = "${var:logDirectory}logfile.log"... >
        LogManager.Configuration.Variables["logDirectory"] = currentDir + Path.DirectorySeparatorChar;
            
        try
        {
            logger.Trace($"Configuration of {nameof(Microsoft.AspNetCore.WebHost)} starting.");
            logger.Info($"This app runs as {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process.\n\n");

            var builder = SetupBuilder(args);

            #region ** Setup logging **

            builder.Logging.ClearProviders();
            // Enable NLog as logging provider for Microsoft.Extension.Logging
            logConfigFile = Path.Combine(currentDir, LeagueStartup.ConfigurationFolder, $"NLog.{builder.Environment.EnvironmentName}.config");
            var nLogConfiguration = LogManager.Setup()
                .LoadConfigurationFromFile(logConfigFile)
                .LogFactory.Configuration;
            var nLogOptions = new NLogAspNetCoreOptions { AutoShutdown = true, IncludeScopes = true };

            builder.Logging.AddNLog(nLogConfiguration, nLogOptions);
            builder.Host.UseNLog();

            // Create LoggerFactory for use in configuration
            var loggerFactory = LoggerFactory.Create(b => b.AddNLog(nLogConfiguration, nLogOptions));

            #endregion

            LeagueStartup.ConfigureServices(builder, loggerFactory);
            WebAppStartup.ConfigureServices(builder, loggerFactory);

            var app = builder.Build();

            LeagueStartup.Configure(app, loggerFactory);
            LeagueStartup.InitializeRankingAndCharts(app.Services.GetRequiredService<TenantStore>(), app.Services.GetRequiredService<IBackgroundQueue>(), app.Services);
            WebAppStartup.Configure(app, loggerFactory);

            await app.RunAsync();
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Application stopped after Exception.");
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            LogManager.Shutdown();
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
