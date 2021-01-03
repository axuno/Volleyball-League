using System.Globalization;
using System.IO;
using Axuno.VirtualFileSystem;
using League.Identity;
using League.Test.TestComponents;
using League.TextTemplatingModule;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NLog;
using NLog.Extensions.Logging;
using SD.LLBLGen.Pro.ORMSupportClasses;
using TournamentManager.MultiTenancy;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace League.Test
{
    public class UnitTestHelpers
    {
        private ServiceProvider _serviceProvider;
        private readonly ITenantContext _tenantContext;
        private string _configPath;

        public UnitTestHelpers()
        {
            _configPath = DirectoryLocator.GetTargetConfigurationPath();
            var msSqlPath = Path.Combine(DirectoryLocator.GetTargetProjectPath(), @"..\..\MsSqlDb");
            
            // For the unit tests we
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
            //RuntimeConfiguration.SetDependencyInjectionInfo(new[] { typeof(TournamentManager.Validators.UserEntityValidator).Assembly }, new[] { "TournamentManager.Validators" });

            RuntimeConfiguration.Tracing.SetTraceLevel("ORMPersistenceExecution", System.Diagnostics.TraceLevel.Verbose);
            RuntimeConfiguration.Tracing.SetTraceLevel("ORMPlainSQLQueryExecution", System.Diagnostics.TraceLevel.Verbose);

            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(Path.Combine(DirectoryLocator.GetTargetConfigurationPath(), "NLog.Internal.config"));
            TournamentManager.AppLogging.LoggerFactory = ServiceProvider.GetRequiredService<ILoggerFactory>();
        }

        public ITenantContext GetTenantContext()
        {
            return _tenantContext;
        }

        public UserStore GetUserStore()
        {
            return new UserStore(_tenantContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null).Object);
        }

        public RoleStore GetRoleStore()
        {
            return new RoleStore(_tenantContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null).Object);
        }

        public ServiceProvider ServiceProvider
        {
            get
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
                    .AddTextTemplatingModule()
                    .BuildServiceProvider();
            }
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
                    sp.GetRequiredService<NodaTime.TimeZones.DateTimeZoneCache>(), "Europe/Berlin",
                    CultureInfo.CurrentCulture,
                    NodaTime.TimeZones.Resolvers.LenientResolver))
                .AddTransient<ITenantContext>(sp => tenantContext)
                .AddTextTemplatingModule()
                .AddTextTemplatingModule(vfs =>
                    {
                        // Test can run using the physical path or with embedded resources:
                        
                        // The Templates\Emails folder is embedded in the project file
                        vfs.FileSets.AddEmbedded<Startup>($"{nameof(League)}.Templates");
                        
                        // Use the templates from the project "Templates" folder
                        var leagueProjectPath = DirectoryLocator.GetTargetProjectPath();
                        vfs.FileSets.AddPhysical(Path.Combine(leagueProjectPath, "Templates"));
                    },
                    locOpt =>
                    {
                    })
                .BuildServiceProvider();
        }
        

        public TestServer GetLeagueTestServer()
        {
            var server = new TestServer(new Microsoft.AspNetCore.Hosting.WebHostBuilder()
                .UseStartup<Startup>());
            
            return server;
        }
        
        private void HowToUseServices()
        {
            var logger = (ILogger) ServiceProvider.GetRequiredService(typeof(ILogger<UnitTestHelpers>));
            logger.LogError("error");
            var localizer = (IStringLocalizer)ServiceProvider.GetRequiredService(typeof(IStringLocalizer<League.Controllers.Account>));
            var translated = localizer["This is your password recovery key"].Value;

            var mlLoc = new MultiLanguageIdentityErrorDescriber((IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>)ServiceProvider.GetRequiredService(typeof(IStringLocalizer<MultiLanguageIdentityErrorDescriberResource>)));
            translated = mlLoc.ConcurrencyFailure().Description;
        }
    }
}
