using League.Components;
using League.WebApp.ViewComponents;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.FileProviders;

namespace League.WebApp;

/// <summary>
/// The demo startup class to set up and configure the league.
/// </summary>
public static class WebAppStartup
{
    /// <summary>
    /// The method gets called by <see cref="Program"/> at startup, BEFORE building the app is completed.
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder, ILoggerFactory loggerFactory)
    {
        var services = builder.Services;
        var environment = builder.Environment;

        services.AddHttpsRedirection(options =>
        {
            options.RedirectStatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status301MovedPermanently;
        });
            
        // Add custom navigation menu items to the League default navigation system
        services.AddScoped<IMainNavigationNodeBuilder, CustomMainNavigationNodeBuilder>();

        if (environment.IsDevelopment())
        {
            // Add runtime compilation for the app
            services.AddRazorPages().AddRazorRuntimeCompilation();
            // Add runtime compilation for the League RCL
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                var leagueLibraryPath = Path.GetFullPath(
                    Path.Combine(environment.ContentRootPath, "..", nameof(League)));

                options.FileProviders.Add(new PhysicalFileProvider(leagueLibraryPath));
            });
        }
    }

    /// <summary>
    /// The method gets called by <see cref="Program"/> at startup, AFTER building the app is completed.
    /// </summary>
    public static void Configure(WebApplication app, ILoggerFactory loggerFactory)
    {
        var env = app.Environment;

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
        
        app.UseHttpsRedirection();

        // DO NOT add the default endpoints! => Url.Action(...) could produce Urls violating attribute routes
        // and e.g. showing wrong navigation links:
        // app.UseEndpoints(endpoints => { endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}"); });
    }
}
