using Axuno.TextTemplating;
using Axuno.TextTemplating.VirtualFiles;
using Axuno.VirtualFileSystem;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;

namespace League.TextTemplatingModule;

public static class TextTemplatingServiceCollectionExtensions
{
    public static IServiceCollection AddTextTemplatingModule(this IServiceCollection services, Action<VirtualFileSystemOptions>? virtualFileSystemOptions = null, Action<LocalizationOptions>? localizationOptions = null, Action<LeagueTemplateRendererOptions>? renderOptions = null)
    {
        services.Configure<VirtualFileSystemOptions>(virtualFileSystemOptions ?? (options => { }));
        services.Configure<LocalizationOptions>(localizationOptions ?? (options => { }));
        services.Configure<LeagueTemplateRendererOptions>(renderOptions ?? (options => { }));

        services.Configure<TextTemplatingOptions>(options =>
        {
            // Don't forget to add content and definition providers as services
            options.DefinitionProviders.Add(typeof(EmailTemplateDefinitionProvider));
            options.ContentContributors.Add(typeof(VirtualFileTemplateContentContributor));
        });

        services.AddTransient<EmailTemplateDefinitionProvider>();
        services.AddSingleton<IFileProvider, VirtualFileProvider>();
        services.AddSingleton<IDynamicFileProvider, DynamicFileProvider>();
        services.AddTransient<VirtualFileTemplateContentContributor>();
        services.AddSingleton<ILocalizedTemplateContentReaderFactory, LocalizedTemplateContentReaderFactory>();
           
        services.AddSingleton<ITemplateDefinitionManager, TemplateDefinitionManager>();
        services.AddTransient<ITemplateContentProvider, TemplateContentProvider>();
        services.AddTransient<ITemplateRenderer, LeagueTemplateRenderer>();

        // The following services have probably already been registered
            
        services.TryAddSingleton<IStringLocalizerFactory>(s => new ResourceManagerStringLocalizerFactory(s.GetRequiredService<IOptions<LocalizationOptions>>(), NullLoggerFactory.Instance));
        services.TryAddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

        #region ** Timezone service **
            
        var ianaTzId = "America/New_York"; // America/New_York
        // TimeZoneConverter will use the culture of the current scope
        services.TryAddTransient<Axuno.Tools.DateAndTime.TimeZoneConverter>(sp => new(
            ianaTzId, CultureInfo.GetCultureInfo("en")));

        #endregion
            
        return services;
    }
}
