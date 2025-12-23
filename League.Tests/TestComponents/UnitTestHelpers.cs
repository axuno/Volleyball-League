using System.Globalization;
using Axuno.VirtualFileSystem;
using League.Caching;
using League.Identity;
using League.Tests.TestComponents;
using League.TextTemplatingModule;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NLog;
using NLog.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.MultiTenancy;

namespace League.Tests;

public class UnitTestHelpers
{
    private ServiceProvider? _serviceProvider;
    private readonly TenantContext _tenantContext;
    private readonly string _configPath;

    public UnitTestHelpers()
    {
        _configPath = DirectoryLocator.GetTargetConfigurationPath();
        var msSqlPath = Path.GetFullPath(Path.Combine(DirectoryLocator.GetTargetProjectPath(typeof(League.LeagueStartup)), @"..\..\MsSqlDb"));

        _tenantContext = new TenantContext
        {
            DbContext =
            {
                Catalog = "LeagueIntegration",
                Schema = "dbo",
                ConnectionKey = "dummy"
            }
        };
        _tenantContext.DbContext.ConnectionString =
            $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={msSqlPath}\\LeagueIntegrationTest.mdf;Database={_tenantContext.DbContext.Catalog};Integrated Security=true";

        // Make sure we can connect to the database
        using (var connection = new Microsoft.Data.SqlClient.SqlConnection(_tenantContext.DbContext.ConnectionString))
            try
            {
                connection.Open();
            }
            finally
            {
                connection.Close();
            }

        InitializeLlBlGenPro();
    }

    private void InitializeLlBlGenPro()
    {
        RuntimeConfiguration.ConfigureDQE<SD.LLBLGen.Pro.DQE.SqlServer.SQLServerDQEConfiguration>(c => c
            .SetTraceLevel(System.Diagnostics.TraceLevel.Verbose)
            .AddDbProviderFactory(typeof(Microsoft.Data.SqlClient.SqlClientFactory)));

        RuntimeConfiguration.Tracing.SetTraceLevel("ORMPersistenceExecution", System.Diagnostics.TraceLevel.Verbose);
        RuntimeConfiguration.Tracing.SetTraceLevel("ORMPlainSQLQueryExecution", System.Diagnostics.TraceLevel.Verbose);

        LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(DirectoryLocator.GetTargetConfigurationPath(), "NLog.Internal.config"));
        TournamentManager.AppLogging.LoggerFactory = GetStandardServiceProvider().GetRequiredService<ILoggerFactory>();
    }

    public ITenantContext GetTenantContext()
    {
        return _tenantContext;
    }

    public UserStore GetUserStore()
    {
        return new(_tenantContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null!).Object);
    }

    public RoleStore GetRoleStore()
    {
        return new(_tenantContext, new NullLogger<League.Identity.RoleStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null!).Object);
    }

    public ServiceProvider GetStandardServiceProvider()
    {
        return _serviceProvider ??= new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            })
            .AddLocalization()
            .BuildServiceProvider();
    }

    public static ServiceProvider GetTextTemplatingServiceProvider(ITenantContext tenantContext)
    {
        return new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            })
            .AddLocalization()
            .AddSingleton(sp => new Axuno.Tools.DateAndTime.TimeZoneConverter(
                "Europe/Berlin", CultureInfo.CurrentCulture))
            .AddTransient<ITenantContext>(sp => tenantContext)
            .AddTextTemplatingModule(vfs =>
            {
                // The complete Templates folder is embedded in the project file
                vfs.FileSets.AddEmbedded<LeagueTemplateRenderer>(nameof(League) + ".Templates");
                // To use physical files: vfs.FileSets.AddPhysical(Path.Combine(Directory.GetCurrentDirectory(), "Templates"));
            },
                locOpt =>
                {
                },
                renderOptions =>
                {
                    renderOptions.MemberNotFoundAction = RenderErrorAction.ThrowError;
                    renderOptions.VariableNotFoundAction = RenderErrorAction.ThrowError;
                })
            .BuildServiceProvider();
    }

    public static ServiceProvider GetReportSheetCacheServiceProvider(ITenantContext tenantContext, Microsoft.AspNetCore.Hosting.IWebHostEnvironment webHostEnvironment, IEnumerable<KeyValuePair<string, string?>> browserPath)
    {
        return new ServiceCollection()
            .AddLogging(builder =>
            {
                builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.AddNLog(new NLogProviderOptions
                {
                    CaptureMessageTemplates = true,
                    CaptureMessageProperties = true
                });
            })
            .AddTransient<IConfiguration>(sp =>
            {
                var c = new ConfigurationManager();
                c.AddInMemoryCollection(browserPath);
                return c;
            })
            .AddTransient<ITenantContext>(sp => tenantContext)
            .AddTransient<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>(sp => webHostEnvironment)
            .AddTransient<ReportSheetCache>()
            .AddLocalization()
            .BuildServiceProvider();
    }

    /// <summary>
    /// Creates a test application factory for integration testing with .NET 10.
    /// Requires a Program class from the host application.
    /// </summary>
    /// <typeparam name="T">The Program class entry point of the application being tested.</typeparam>
    /// <returns>WebApplicationFactory configured for testing.</returns>
    public static WebApplicationFactory<T> GetLeagueTestApplicationFactory<T>()
        where T : class
    {
        return new();
    }

    /// <summary>
    /// Creates an HttpClient for testing via WebApplicationFactory.
    /// </summary>
    /// <typeparam name="TProgram">The Program class entry point of the application being tested.</typeparam>
    /// <param name="factory">The WebApplicationFactory used to create the client. The caller is responsible for disposing this factory.</param>
    /// <returns>HttpClient connected to the test server.</returns>
    public static HttpClient GetLeagueTestHttpClient<TProgram>(out WebApplicationFactory<TProgram> factory)
        where TProgram : class
    {
        factory = GetLeagueTestApplicationFactory<TProgram>();
        return factory.CreateClient();
    }

    private void HowToUseServices()
    {
        var logger = (Microsoft.Extensions.Logging.ILogger) GetStandardServiceProvider().GetRequiredService(typeof(ILogger<UnitTestHelpers>));
        logger.LogError("error");
        var localizer = (IStringLocalizer) GetStandardServiceProvider().GetRequiredService(typeof(IStringLocalizer<League.Controllers.Account>));
        _ = localizer["This is your password recovery key"].Value;

        var mlLoc = new MultiLanguageIdentityErrorDescriber((IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>) GetStandardServiceProvider().GetRequiredService(typeof(IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>)));
        _ = mlLoc.ConcurrencyFailure().Description;
    }
}
