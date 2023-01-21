using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using League.Components;
using League.Web.ViewComponents;

namespace League.Web;

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
        

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    public void ConfigureServices(IServiceCollection services)
    {
        // Initialize the League RCL
        LeagueStartup.ConfigureServices(services);
            
        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently;
        });
            
        // Add custom navigation menu items to the League default navigation system
        services.AddScoped<IMainNavigationNodeBuilder, DemoMainNavigationNodeBuilder>();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
    {
        #region * Setup error handling *

        // Error handling must be one of the very first things to configure
        if (env.IsProduction())
        {
            // The StatusCodePagesMiddleware should be one of the earliest 
            // middleware in the pipeline, as it can only modify the response 
            // of middleware that comes after it in the pipeline
            app.UseStatusCodePagesWithReExecute($"/{nameof(League.Controllers.Error)}/{{0}}");
            app.UseExceptionHandler($"/{nameof(League.Controllers.Error)}/500");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }
        else
        {
            app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
            app.UseDeveloperExceptionPage();
            app.UseStatusCodePages();
        }

        #endregion

        LeagueStartup.Configure(app, env, loggerFactory);

        app.UseHttpsRedirection();           
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        // DO NOT add the default endpoints! => Url.Action(...) could produce Urls violating attribute routes
        // and e.g. showing wrong navigation links:
        // app.UseEndpoints(endpoints => { endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); });
    }
}
