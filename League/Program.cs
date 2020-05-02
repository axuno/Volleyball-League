using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;

namespace League
{
    public class Program
    {
        /// <summary>
        /// The name of the configuration folder, which is relative to HostingEnvironment.ContentRootPath.
        /// Constant is also used in components where IWebHostEnvironment is injected
        /// </summary>
        public const string ConfigurationFolder = "Configuration";

        public static void Main(string[] args)
        {
            // NLog: setup the logger first to catch all errors
            var currentDir = Directory.GetCurrentDirectory();
            var logger = NLogBuilder
                .ConfigureNLog($@"{currentDir}\{ConfigurationFolder}\NLog.Internal.config")
                .GetCurrentClassLogger();

            // Allows for <target name="file" xsi:type="File" fileName = "${var:logDirectory}logfile.log"... >
            NLog.LogManager.Configuration.Variables["logDirectory"] = currentDir + "\\";

            try
            {
                logger.Trace($"Configuration of {nameof(WebHost)} starting.");
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

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var configPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, ConfigurationFolder);
                    config.SetBasePath(configPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile(@"credentials.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"credentials.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);

                    var secretsFolder = Path.Combine(configPath,  @"..\..\..\Secrets");
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        if (!Directory.Exists(secretsFolder)) throw new DirectoryNotFoundException("Secrets folder not found");
                        config.AddJsonFile(Path.Combine(secretsFolder, @"credentials.json"), false);
                        config.AddJsonFile(Path.Combine(secretsFolder, $"credentials.{hostingContext.HostingEnvironment.EnvironmentName}.json"), false);
                    }

                    NLogBuilder.ConfigureNLog(Path.Combine(configPath, $"NLog.{hostingContext.HostingEnvironment.EnvironmentName}.config"));
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.ClearProviders();
                    // Note: This logging configuration overrides any call to SetMinimumLevel!
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                })
                .UseNLog();  // NLog: Setup NLog for dependency injection;
    }
}
