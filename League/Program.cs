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

        private static string _absoluteConfigurationPath = string.Empty;
        
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
                    _absoluteConfigurationPath = Path.Combine(hostingContext.HostingEnvironment.ContentRootPath, ConfigurationFolder);
                    config.SetBasePath(_absoluteConfigurationPath)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                        .AddJsonFile(@"credentials.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"credentials.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: false, reloadOnChange: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                    
                    if (hostingContext.HostingEnvironment.IsDevelopment())
                    {
                        var secretsFolder = GetSecretsFolder();    
                        config.AddJsonFile(Path.Combine(secretsFolder, @"credentials.json"), false);
                        config.AddJsonFile(Path.Combine(secretsFolder, $"credentials.{hostingContext.HostingEnvironment.EnvironmentName}.json"), false);
                    }

                    NLogBuilder.ConfigureNLog(Path.Combine(_absoluteConfigurationPath, $"NLog.{hostingContext.HostingEnvironment.EnvironmentName}.config"));
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
        
        
        /// <summary>
        /// Gets the name of the folder containing credentials and other data of the live website.
        /// </summary>
        /// <returns>The name of the folder containing credentials and other data of the live website</returns>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public static string GetSecretsFolder()
        {
            var folder = Path.Combine(_absoluteConfigurationPath, @"..\..\..\Secrets");
            if (!Directory.Exists(folder)) throw new DirectoryNotFoundException("Secrets folder not found");
            return folder;
        }
    }
   
}
