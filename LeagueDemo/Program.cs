using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using NLog.Web;

namespace LeagueDemo;

public class Program
{
    public static void Main(string[] args)
    {
        // NLog: setup the logger first to catch all errors
        var currentDir = Directory.GetCurrentDirectory();
        var logger = NLogBuilder
            .ConfigureNLog($@"{currentDir}\{League.Startup.ConfigurationFolder}\NLog.Internal.config")
            .GetCurrentClassLogger();

        // Allows for <target name="file" xsi:type="File" fileName = "${var:logDirectory}logfile.log"... >
        NLog.LogManager.Configuration.Variables["logDirectory"] = currentDir + "\\";
            
        try
        {
            logger.Trace($"Configuration of {nameof(Microsoft.AspNetCore.WebHost)} starting.");
            logger.Info($"This app runs as {(Environment.Is64BitProcess ? "64-bit" : "32-bit")} process.\n\n");
                
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception e)
        {
            logger.Fatal(e, $"Application stopped after Exception. {e.Message}");
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            NLog.LogManager.Shutdown();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var absoluteConfigurationPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath,
                    League.Startup.ConfigurationFolder);
                config.SetBasePath(absoluteConfigurationPath)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true, reloadOnChange: true)
                    .AddJsonFile(@"credentials.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"credentials.{hostingContext.HostingEnvironment.EnvironmentName}.json",
                        optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .AddCommandLine(args);

                NLogBuilder.ConfigureNLog(Path.Combine(absoluteConfigurationPath,
                    $"NLog.{hostingContext.HostingEnvironment.EnvironmentName}.config"));
            })
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseStartup<LeagueDemo.Startup>();
                // Use static web assets from League (and other referenced projects or packages)
                webHostBuilder.UseStaticWebAssets();
            })
            .ConfigureLogging((hostingContext, logging) =>
            {
                logging.ClearProviders();
                // Note: This logging configuration overrides any call to SetMinimumLevel!
                logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
            })
            .UseNLog();
        // NLog: Setup NLog for dependency injection;
    }
}