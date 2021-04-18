using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using League.Components;

namespace LeagueDemo
{
    public class Startup
    {
        internal League.Startup LeagueStartup;
        
        /// <summary>
        /// Gets the application configuration properties of this application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the information about the web hosting environment of this application.
        /// </summary>
        public IWebHostEnvironment WebHostEnvironment { get; }
        
        /// <summary>
        /// Gets the logger for class <see cref="Startup"/>.
        /// </summary>
        public ILogger<Startup> Logger { get; }
        
        /// <summary>
        /// Get a <see cref="LoggerFactory"/>.
        /// </summary>
        public ILoggerFactory LoggerFactory { get; }
        
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
            
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                builder.AddNLog();                
            });
            Logger = LoggerFactory.CreateLogger<Startup>();
            
            LeagueStartup = new League.Startup(Configuration, WebHostEnvironment);
        }
        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Initialize the League RCL
            LeagueStartup.ConfigureServices(services);
            
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently;
            });
            
            services.AddScoped<IMainNavigationNodeBuilder, LeagueDemo.ViewComponents.DemoMainNavigationNodeBuilder>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            LeagueStartup.Configure(app, env, loggerFactory);
            
            if (env.IsDevelopment())
            {
                app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
                
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }
            else
            {
                app.UseStatusCodePagesWithReExecute($"/{nameof(League.Controllers.Error)}/{{0}}");
                app.UseExceptionHandler($"/{nameof(League.Controllers.Error)}/500");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();           
            
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            // DO NOT add the default endpoints! => Url.Action(...) could produce Urls violating attribute routes
            // and e.g. showing wrong navigation links:
            // app.UseEndpoints(endpoints => { endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); });
        }
    }
}
