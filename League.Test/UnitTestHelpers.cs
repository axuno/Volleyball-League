﻿using System.IO;
using League.Identity;
using League.Test.Identity;
using League.Test.TestComponents;
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
using TournamentManager.Data;
using TournamentManager.MultiTenancy;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using SiteContext = League.DI.SiteContext;

namespace League.Test
{
    public class UnitTestHelpers
    {
        private ServiceProvider _serviceProvider;
        private readonly SiteContext _siteContext;
        private string _configPath;

        public UnitTestHelpers()
        {
            _configPath = DirectoryLocator.GetTargetConfigurationPath();
            
            var store = new TournamentManager.MultiTenancy.TenantStore(new ConfigurationSection(null, null), new NullLogger<TenantStore>())
            {
                GetTenantConfigurationFiles = () =>
                    Directory.GetFiles(_configPath,
                        $"Tenant.*.Development.config", SearchOption.TopDirectoryOnly)
            }.LoadTenants();
           

            foreach (var tenant in store.GetTenants().Values)
            {
                tenant.DbContext.Catalog = "LeagueIntegration";
                var msSqlPath = Path.Combine(DirectoryLocator.GetTargetProjectPath(), @"..\..\MsSqlDb");
                tenant.DbContext.ConnectionString =
                    $"Server=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={msSqlPath}\\LeagueIntegrationTest.mdf;Database={tenant.DbContext.Catalog};Integrated Security=true";
            }
            

            InitializeLlBlGenPro();

            _siteContext = new SiteContext(dbContextResolver.Resolve("dbo").OrganizationKey, new OrganizationContextResolver(dbContextResolver, new NullLogger<OrganizationContextResolver>(), _configPath), orgSiteList);
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

        public SiteContext GetsiteContext()
        {
            return _siteContext;
        }

        public UserStore GetUserStore()
        {
            return new UserStore(_siteContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null).Object);
        }

        public RoleStore GetRoleStore()
        {
            return new RoleStore(_siteContext, new NullLogger<UserStore>(), new UpperInvariantLookupNormalizer(), new Mock<MultiLanguageIdentityErrorDescriber>(null).Object);
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
                    .BuildServiceProvider();
            }
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
