using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MailMergeLib.AspNet;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;

namespace League.BackgroundTasks
{
    /// <summary>
    /// Create a new <see cref="IWebHost"/> instance, which can be injected into background tasks,
    /// e.g. when using a <see cref="IHostedService"/>. Useful for rendering <see cref="RazorView"/>s
    /// outside the app's <see cref="Microsoft.AspNetCore.Http.HttpContext"/>.
    /// </summary>
    public class BackgroundWebHost : IDisposable
    {
        private static IWebHost _instance;
        private static IServiceCollection _serviceDescriptors;

        /// <summary>
        /// CTOR.
        /// </summary>
        /// <param name="serviceDescriptors">A <see cref="IServiceCollection"/>, like is built in the ConfigureServices method of a web app.
        /// If the parameter is null, the <see cref="IWebHost"/> will be built with a minimum set of services needed for processing
        /// <see cref="RazorView"/>s.
        /// </param>
        public BackgroundWebHost(IServiceCollection serviceDescriptors = null)
        {
            _serviceDescriptors = serviceDescriptors;
        }

        /// <summary>
        /// Gets the static instance of a new <see cref="IWebHost"/>.
        /// The <see cref="IWebHost"/> will be lazy-initialized when accessing the <see cref="Instance"/> property for the first time.
        /// </summary>
        public IWebHost Instance
        {
            get
            {
                if (_instance != null) return _instance;

                _instance = _serviceDescriptors != null
                    ? CreateWithExternalServices(_serviceDescriptors)
                    : CreateWithMinimalServices();
                return _instance;
            }
        }

        /// <summary>
        /// Creates a <see cref="IWebHost"/> using the <see cref="IServiceCollection"/> supplied in the constructor.
        /// </summary>
        /// <param name="serviceDescriptors"></param>
        /// <returns></returns>
        private static IWebHost CreateWithExternalServices(IServiceCollection serviceDescriptors)
        {
            return new WebHostBuilder().ConfigureServices(services =>
            {
                foreach (var serviceDescriptor in serviceDescriptors)
                {
                    services.Add(serviceDescriptor);
                }
                // make sure the the RazorViewToStringRenderer has transient lifetime
                var razorViewToStringRender = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(RazorViewToStringRenderer));
                if (razorViewToStringRender != null) services.Remove(razorViewToStringRender);
                services.AddTransient<RazorViewToStringRenderer>();
            }).UseStartup<Startup>().Build();
        }

        /// <summary>
        /// Creates a <see cref="IWebHost"/> built with a minimum set of services needed for processing
        /// <see cref="RazorView"/>s.
        /// </summary>
        /// <returns></returns>
        private static IWebHost CreateWithMinimalServices()
        {
            // Source code for AspNetCore 3.x inspired by Corstian Boerman: 
            // http://corstianboerman.com/2019-12-25/using-the-razorviewtostringrenderer-with-asp-net-core-3.html

            var appDirectory = Directory.GetCurrentDirectory();
            return new WebHostBuilder().ConfigureServices(services =>
            {
                // Basic requirement for RazorViewToStringRenderer

                services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
                var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
                services.AddSingleton<DiagnosticListener>(diagnosticSource);
                services.AddSingleton<DiagnosticSource>(diagnosticSource);
                services.AddLogging();
                services.AddRazorPages().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix);
                services.Configure<Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation.MvcRazorRuntimeCompilationOptions>(options =>
                {
                    options.FileProviders.Add(new PhysicalFileProvider(appDirectory));
                });
                services.AddTransient<RazorViewToStringRenderer>();

            }).UseStartup<Startup>().Build();
        }

        private class Startup
        {
            public void Configure()
            {
            }
        }

        #region *** Uncommented code, usable only for AspNetCore 2.x
        /*
        protected static Task<string> RenderViewAsync(string viewName, object model)
        {
            var scopeFactory = InitializeServices(Directory.GetCurrentDirectory());
            using (var serviceScope = scopeFactory.CreateScope())
            {
                var helper = serviceScope.ServiceProvider.GetRequiredService<RazorViewToStringRenderer>();

                return helper.RenderViewToStringAsync(viewName, model);
            }
        }


        protected static IServiceScopeFactory InitializeServices(string customApplicationBasePath = null)
        {
            // Initialize the necessary services
            var services = new ServiceCollection();
            ConfigureDefaultServices(services, customApplicationBasePath);

            // Add a custom service that is used in the view.
            //services.AddSingleton<EmailReportGenerator>();

            var serviceProvider = services.BuildServiceProvider();
            return serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        private static void ConfigureDefaultServices(IServiceCollection services, string customApplicationBasePath)
        {
            string applicationName;
            IFileProvider fileProvider;
            if (!string.IsNullOrEmpty(customApplicationBasePath))
            {
                applicationName = Path.GetFileName(customApplicationBasePath);
                fileProvider = new PhysicalFileProvider(customApplicationBasePath);
            }
            else
            {
                applicationName = Assembly.GetEntryAssembly()?.GetName().Name;
                fileProvider = new PhysicalFileProvider(Directory.GetCurrentDirectory());
            }

            services.AddSingleton<IHostEnvironment>(new HostingEnvironment
            {
                ApplicationName = applicationName,
                ContentRootFileProvider = fileProvider,
            });


            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<DiagnosticSource>(diagnosticSource);
            services.AddSingleton<DiagnosticListener>(diagnosticSource);
            services.AddLogging();
            services.AddMvc();
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new Views.LeagueViewLocationExpander()); // R# will not resolve
                options.ViewLocationFormats.Add("/Views/Emails/{0}.cshtml"); // R# will resolve
            });
            services.AddTransient<RazorViewToStringRenderer>();
        }
        */
        #endregion

        public void Dispose()
        {
            _serviceDescriptors?.Clear();
            _instance?.Dispose();
        }
    }
}
