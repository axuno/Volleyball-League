using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;
using NLog.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using TournamentManager.MultiTenancy;
using Axuno.BackgroundTask;

namespace League.WebApp;

public class Program
{
    public static async Task Main(string[] args)
    {
        // NLog: setup the logger first to catch all errors
        var currentDir = Directory.GetCurrentDirectory();
        var logger = NLogBuilder
            .ConfigureNLog($@"{currentDir}\{LeagueStartup.ConfigurationFolder}\NLog.Internal.config")
            .GetCurrentClassLogger();

        // Allows for <target name="file" xsi:type="File" fileName = "${var:logDirectory}logfile.log"... >
        NLog.LogManager.Configuration.Variables["logDirectory"] = currentDir + "\\";
            
        try
        {
            logger.Trace($"Configuration of {nameof(Microsoft.AspNetCore.WebHost)} starting.");
            logger.Info($"This app runs as {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process.\n\n");

            var builder = SetupBuilder(args);

            var loggingConfig = builder.Configuration.GetSection("Logging");
            builder.Logging.ClearProviders();
            // Enable NLog as logging provider for Microsoft.Extension.Logging
            builder.Logging.AddNLog(loggingConfig);
            NLogBuilder.ConfigureNLog(Path.Combine(builder.Environment.ContentRootPath, LeagueStartup.ConfigurationFolder,
                $"NLog.{builder.Environment.EnvironmentName}.config"));

            builder.WebHost.ConfigureServices((context, services) =>
            {
                LeagueStartup.ConfigureServices(services, context.HostingEnvironment, context.Configuration);
                WebAppStartup.ConfigureServices(services);
            });

            var app = builder.Build();

            builder.WebHost.ConfigureAppConfiguration(_ =>
            {
                LeagueStartup.Configure(app, app.Services.GetRequiredService<ILoggerFactory>());
                LeagueStartup.InitializeRankingAndCharts(app.Services.GetRequiredService<TenantStore>(), app.Services.GetRequiredService<IBackgroundQueue>(), app.Services);
                WebAppStartup.Configure(app, app.Services.GetRequiredService<ILoggerFactory>());
            });
            
            await app.RunAsync();
        }
        catch (Exception e)
        {
            logger.Fatal(e, "Application stopped after Exception.");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }
    }

    public static WebApplicationBuilder SetupBuilder(string[] args)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ApplicationName = typeof(Program).Assembly.GetName().Name, // don't use Assembly.Fullname
            ContentRootPath = Directory.GetCurrentDirectory(),
            WebRootPath = "wwwroot"
        });
        
        var absoluteConfigurationPath = Path.Combine(builder.Environment.ContentRootPath,
            LeagueStartup.ConfigurationFolder);

        builder.Configuration.SetBasePath(absoluteConfigurationPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
                optional: true, reloadOnChange: true)
            .AddJsonFile(@"credentials.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"credentials.{builder.Environment.EnvironmentName}.json",
                optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);

        // Use static web assets from League (and other referenced projects or packages)
        builder.WebHost.UseStaticWebAssets();

        return builder;
    }
}
