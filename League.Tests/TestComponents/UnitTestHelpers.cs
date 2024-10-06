using System.Globalization;
using Axuno.VirtualFileSystem;
using League.Caching;
using League.Identity;
using League.Tests.TestComponents;
using League.TextTemplatingModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
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
    private readonly ITenantContext _tenantContext;
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
            .AddDbProviderFactory(typeof(System.Data.SqlClient.SqlClientFactory)));

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
        return new UserStore(_tenantContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null!).Object);
    }

    public RoleStore GetRoleStore()
    {
        return new League.Identity.RoleStore(_tenantContext, new NullLogger<League.Identity.RoleStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null!).Object);
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

    public static ServiceProvider GetReportSheetCacheServiceProvider(ITenantContext tenantContext, IWebHostEnvironment webHostEnvironment, IEnumerable<KeyValuePair<string,string?>> browserPath)
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
            .AddTransient<IWebHostEnvironment>(sp => webHostEnvironment)
            .AddTransient<ReportSheetCache>()
            .AddLocalization()
            .BuildServiceProvider();
    }


    public static TestServer GetLeagueTestServer()
    {
        var server = new TestServer(new Microsoft.AspNetCore.Hosting.WebHostBuilder());

        return server;
    }

    private void HowToUseServices()
    {
        var logger = (Microsoft.Extensions.Logging.ILogger) GetStandardServiceProvider().GetRequiredService(typeof(ILogger<UnitTestHelpers>));
        logger.LogError("error");
        var localizer = (IStringLocalizer)GetStandardServiceProvider().GetRequiredService(typeof(IStringLocalizer<League.Controllers.Account>));
        _ = localizer["This is your password recovery key"].Value;

        var mlLoc = new MultiLanguageIdentityErrorDescriber((IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>)GetStandardServiceProvider().GetRequiredService(typeof(IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>)));
        _ = mlLoc.ConcurrencyFailure().Description;
    }
}
