using System;
using System.Globalization;
using Axuno.TextTemplating;
using Axuno.TextTemplating.VirtualFiles;
using Axuno.VirtualFileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
#nullable enable
namespace League.TextTemplatingModule
{
    public static class TextTemplatingServiceCollectionExtensions
    {
        public static IServiceCollection AddTextTemplatingModule(this IServiceCollection services, Action<VirtualFileSystemOptions>? virtualFileSystemOptions = null, Action<LocalizationOptions>? localizationOptions = null)
        {
            services.Configure<VirtualFileSystemOptions>(virtualFileSystemOptions ?? (options => { }));
            services.Configure<LocalizationOptions>(localizationOptions ?? (options => { }));

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
            services.AddSingleton<IStringLocalizerFactory>(s => new ResourceManagerStringLocalizerFactory(s.GetRequiredService<IOptions<LocalizationOptions>>(), NullLoggerFactory.Instance));
            services.AddSingleton<ITemplateDefinitionManager, TemplateDefinitionManager>();
            services.AddTransient<ITemplateContentProvider, TemplateContentProvider>();
            services.AddTransient<ITemplateRenderer, LeagueTemplateRenderer>();

            // The following services have probably already been registered
            
            services.TryAddSingleton(typeof(IStringLocalizer<>), typeof(StringLocalizer<>));

            #region ** Timezone service **

            services.TryAddSingleton<NodaTime.TimeZones.DateTimeZoneCache>(sp =>
                new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default));
            
            var tzId = "America/New_York"; // America/New_York
            // TimeZoneConverter will use the culture of the current scope
            services.TryAddTransient<Axuno.Tools.DateAndTime.TimeZoneConverter>(sp => new Axuno.Tools.DateAndTime.TimeZoneConverter(
                sp.GetRequiredService<NodaTime.TimeZones.DateTimeZoneCache>(), tzId, CultureInfo.GetCultureInfo("en"),
                NodaTime.TimeZones.Resolvers.LenientResolver));

            #endregion
            
            return services;
        }
    }
}
