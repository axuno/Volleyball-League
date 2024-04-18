#region ** Usings **

using JSNLog;
using League.Identity;
using League.ModelBinders;
using League.Routing;
using MailMergeLib;
using MailMergeLib.AspNet;
using MailMergeLib.MessageStore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;

using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System.Diagnostics;
using Axuno.BackgroundTask;
using Axuno.VirtualFileSystem;
using League.BackgroundTasks;
using League.Caching;
using League.ConfigurationPoco;
using League.Emailing;
using League.MultiTenancy;
using TournamentManager.DI;
using TournamentManager.MultiTenancy;
using League.TextTemplatingModule;

#endregion

namespace League;

/// <summary>
/// The startup class to set up and configure the <see cref="League"/> Razor Class Library.
/// Used in a derived class.
/// </summary>
public static class LeagueStartup
{
    /// <summary>
    /// The name of the configuration folder, which is relative to <see ref="IWebHostEnvironment.ContentRootPath"/>.
    /// </summary>
    public const string ConfigurationFolder = "Configuration";

    private static void ConfigureLLblGenPro(TenantStore tenantStore, IWebHostEnvironment environment, IConfiguration configuration)
    {
        foreach (var tenant in tenantStore.GetTenants().Values)
        {
            var connectionString = configuration.GetConnectionString(tenant.DbContext.ConnectionKey);
            RuntimeConfiguration.AddConnectionString(tenant.DbContext.ConnectionKey, connectionString);
            // Enable low-level (result set) caching when specified in selected queries
            // The cache of a query can be overwritten using property 'OverwriteIfPresent'
            CacheController.RegisterCache(connectionString, new ResultsetCache());
            CacheController.CachingEnabled = true;
        }

        if (environment.IsProduction())
        {
            RuntimeConfiguration.ConfigureDQE<SD.LLBLGen.Pro.DQE.SqlServer.SQLServerDQEConfiguration>(c => c
                .SetTraceLevel(TraceLevel.Off)
                .AddDbProviderFactory(typeof(System.Data.SqlClient.SqlClientFactory)));
        }
        else
        {
            RuntimeConfiguration.ConfigureDQE<SD.LLBLGen.Pro.DQE.SqlServer.SQLServerDQEConfiguration>(c => c
                .SetTraceLevel(TraceLevel.Verbose)
                .AddDbProviderFactory(typeof(System.Data.SqlClient.SqlClientFactory)));

            RuntimeConfiguration.Tracing.SetTraceLevel("ORMPersistenceExecution", TraceLevel.Verbose);
            RuntimeConfiguration.Tracing.SetTraceLevel("ORMPlainSQLQueryExecution", TraceLevel.Verbose);
        }
    }

    /// <summary>
    /// This method MUST be called from the derived class.
    /// It is used to add required <see cref="League"/> services to the service container.
    /// </summary>
    public static void ConfigureServices(WebApplicationBuilder builder, ILoggerFactory loggerFactory)
    {
        var configuration = builder.Configuration;
        var environment = builder.Environment;
        var services = builder.Services;

        // Allow TournamentManager to make use of Microsoft.Extensions.Logging
        TournamentManager.AppLogging.Configure(loggerFactory);

        // Add services required for using options.
        services.AddOptions();

        #region * DataProtection service configuration *

        // Usage:
        // private readonly IDataProtector protector;
        // public SomeController(IDataProtectionProvider provider)
        // {  protector = provider.CreateProtector("isolation purpose");}
        // public IActionResult Test(string input)
        // { var protectedPayload = protector.Protect(input);
        // var unprotectedPayload = protector.Unprotect(protectedPayload)
        // ...}

        // required for cookies and session cookies (will throw CryptographicException without)
        services.AddDataProtection()
            .SetApplicationName(environment.ApplicationName)
            .SetDefaultKeyLifetime(TimeSpan.FromDays(360))
            .PersistKeysToFileSystem(
                new DirectoryInfo(Path.Combine(environment.ContentRootPath, "DataProtectionKeys")))
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            });

        // register the multi purpose DataProtector
        services.AddSingleton(typeof(League.DataProtection.DataProtector));

        #endregion

        // Configure form upload limits
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(fo =>
        {
            fo.ValueLengthLimit = int.MaxValue;
            fo.MultipartBodyLengthLimit = int.MaxValue;
        });

        // The default region of this app is "us", unless configured differently
        // The region info is used for country-specific data like phone numbers
        var regionInfo = new RegionInfo(configuration.GetSection("RegionInfo").Value ?? "us");
        services.AddSingleton<RegionInfo>(regionInfo);

        // The default culture of this app is "en". Supported cultures: en, de
        CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(configuration.GetSection("CultureInfo:Culture").Value ?? $"en-{regionInfo.TwoLetterISORegionName}");
        CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(configuration.GetSection("CultureInfo:UiCulture").Value ?? $"en-{regionInfo.TwoLetterISORegionName}");

        // DO NOT USE `options => options.ResourcesPath = "..."` because then resource files in other locations won't be recognized (e.g. resx in the same folder as the controller class)
        services.AddLocalization();

        #region ******** Multi Tenancy ********************************************

        var configSearchPattern = $"Tenant.*.{environment.EnvironmentName}.config";
        var configDirectory = Path.Combine(environment.ContentRootPath, ConfigurationFolder);
        
        var store = (TenantStore) new TenantStore(configuration)
        {
            GetTenantConfigurationFiles = () => Directory.GetFiles(
                configDirectory, configSearchPattern, SearchOption.TopDirectoryOnly)
        }.LoadTenants();

        var tenants = store.GetTenants().Values.ToList();
        if (!tenants.Any(t => t.IsDefault)) throw new InvalidOperationException("No default tenant configuration found.");
        tenants.ForEach(t =>
        {
            if (string.IsNullOrWhiteSpace(t.DbContext.ConnectionString))
                throw new InvalidOperationException($"Tenant '{t.Identifier}': Connection string for key '{t.DbContext.ConnectionKey}' not found.");
        });

        ConfigureLLblGenPro(store, environment, configuration);

        services.AddSingleton<TenantStore>(store);

        services.AddScoped<TenantResolver>();
        services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantResolver>().Resolve());

        // Enable hot ITenant configuration changes 
        services.AddSingleton<TenantConfigWatcher>(new TenantConfigWatcher(store, configDirectory,
            configSearchPattern));

        #endregion

        services.Configure<IISOptions>(options => { });

        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        // Make UrlHelper injectable to any component in the HttpContext
        services.AddScoped<IUrlHelper>(sp => {
            var actionContext = sp.GetRequiredService<IActionContextAccessor>().ActionContext!;
            var factory = sp.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext);
        });
        // TenantLink simplifies tenant-specific path/uri generation
        services.AddScoped<TenantLink>();
        services.AddScoped<ReportSheetCache>();

        services.AddMemoryCache(); // Adds a default in-memory cache implementation
        // MUST be before AddMvc!
        services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(60);
            options.Cookie.HttpOnly = true;
            options.Cookie.Name = ".sid";
            options.Cookie.IsEssential = true;
        });

        #region ** Identity and Authentication **

        var socialLogins = configuration.GetSection(nameof(SocialLogins)).Get<SocialLogins>()
                           ?? throw new InvalidOperationException("No social login configuration found.");

        services.AddAuthentication(options =>
            {
                options.DefaultScheme = IdentityConstants.ApplicationScheme;
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddFacebook(options =>
            {
                options.AppId = socialLogins.Facebook.AppId;
                options.AppSecret = socialLogins.Facebook.AppSecret;
                options.CallbackPath = new PathString("/signin-facebook"); // this path is used by the middleware only, no route necessary
                // add the facebook picture url as an additional claim
                options.ClaimActions.MapJsonKey("urn:facebook:picture", "picture", "picture.data.url");
                options.SaveTokens = true;
                options.CorrelationCookie.Name = ".CorrAuth.League";
                options.Events.OnRemoteFailure = context =>
                {
                    // Note: If this delegate is missing, errors with the external login lead to a System.Exception: access_denied;Description=Permissions error
                    var qsParameter =
                        new Dictionary<string, string?>
                        {
                            {"remoteError", context.Request.Query["error"].ToString()},
                        }.Where(item => !string.IsNullOrEmpty(item.Value)).ToDictionary(i => i.Key, i => i.Value);
                    // joins query strings from RedirectUri and qsParameter
                    var redirectUri = QueryHelpers.AddQueryString(context.Properties?.RedirectUri ?? "/", qsParameter);
                    context.Response.Redirect(redirectUri);
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            })
            .AddGoogle(options =>
            {
                options.ClientId = socialLogins.Google.ClientId;
                options.ClientSecret = socialLogins.Google.ClientSecret;
                options.CallbackPath = new PathString("/signin-google"); // this path is used by the middleware only, no route necessary
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url"); ;
                options.SaveTokens = true;
                options.CorrelationCookie.Name = ".CorrAuth.League";
                options.Events.OnRemoteFailure = context =>
                {
                    // Note: If this delegate is missing, errors with the external login lead to a System.Exception: access_denied;Description=Permissions error
                    var qsParameter =
                        new Dictionary<string, string?>
                        {
                            {"remoteError", context.Request.Query["error"].ToString()},
                        }.Where(item => !string.IsNullOrEmpty(item.Value)).ToDictionary(i => i.Key, i => i.Value);
                    // joins query strings from RedirectUri and qsParameter
                    var redirectUri = QueryHelpers.AddQueryString(context.Properties?.RedirectUri ?? "/", qsParameter);
                    context.Response.Redirect(redirectUri);
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            })
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = socialLogins.Microsoft.ClientId;
                options.ClientSecret = socialLogins.Microsoft.ClientSecret;
                options.CallbackPath = new PathString("/signin-microsoft"); // this path is used by the middleware only, no route necessary
                options.SaveTokens = true;
                options.CorrelationCookie.Name = ".CorrAuth.League";
                options.Events.OnRemoteFailure = context =>
                {
                    // Note: If this delegate is missing, errors with the external login lead to a System.Exception: access_denied;Description=Permissions error
                    var qsParameter =
                        new Dictionary<string, string?>
                        {
                            {"remoteError", context.Request.Query["error"].ToString()},
                        }.Where(item => !string.IsNullOrEmpty(item.Value)).ToDictionary(i => i.Key, i => i.Value);
                    // joins query strings from RedirectUri and qsParameter
                    var redirectUri = QueryHelpers.AddQueryString(context.Properties?.RedirectUri ?? "/", qsParameter);
                    context.Response.Redirect(redirectUri);
                    context.HandleResponse();
                    return Task.CompletedTask;
                };
            });

        // Add before Application and External Cookie configuration and ConfigureApplicationCookie
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                configuration.Bind("IdentityOptions", options); // bind to IdentityOptions section of appsettings.json
            })
            .AddDefaultTokenProviders()
            .AddUserStore<UserStore>()
            .AddRoleStore<RoleStore>()
            .AddErrorDescriber<MultiLanguageIdentityErrorDescriber>()
            .AddUserValidator<LeagueUserValidator<ApplicationUser>>(); // on top of default user validator

        // Make custom claims be added to the ClaimsPrincipal
        services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, LeagueClaimsPrincipalFactory>();

        // Defines the lifetime of tokens sent to users for email / password confirmation et al
        services.Configure<DataProtectionTokenProviderOptions>(options =>
            options.TokenLifespan = TimeSpan.FromDays(3)); // default: 1 day

        // Add for required user name length et al
        services.Configure<LeagueUserValidatorOptions>(configuration.GetSection(nameof(LeagueUserValidatorOptions)));

        #endregion

        #region *** Authorization ***

        services.AddAuthorization(options =>
        {
            // Used on controller method level
            options.AddPolicy(Authorization.PolicyName.MatchPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager, Identity.Constants.RoleName.TeamManager));
            // Used on controller method level
            options.AddPolicy(Authorization.PolicyName.OverruleResultPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager, Identity.Constants.RoleName.TeamManager));
            // Used in team views
            options.AddPolicy(Authorization.PolicyName.SeeTeamContactsPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager, Identity.Constants.RoleName.TeamManager, Identity.Constants.RoleName.Player));
            // Used for my team views
            options.AddPolicy(Authorization.PolicyName.MyTeamPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager, Identity.Constants.RoleName.TeamManager, Identity.Constants.RoleName.Player));
            options.AddPolicy(Authorization.PolicyName.MyTeamAdminPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager));
        });

        // Handler for match date, venue and result authorization
        services.AddSingleton<IAuthorizationHandler, Authorization.MatchAuthorizationHandler>();
        // Handler for team, team venue, team members authorization
        services.AddSingleton<IAuthorizationHandler, Authorization.TeamAuthorizationHandler>();
        // Handler for venue authorization
        services.AddSingleton<IAuthorizationHandler, Authorization.VenueAuthorizationHandler>();

        #endregion

        #region ** Application and External Cookie configuration **

        services.Configure<SecurityStampValidatorOptions>(options =>
        {
            // This determines the time span after which the validity of the authentication cookie
            // will be checked against persistent storage. It is accomplished by calling the
            // SecurityStampValidator for every request to the server. If the current time minus the
            // cookie's issue time is less or equal to ValidationInterval, a call to
            // SignInManager<TUser>.ValidateSecurityStampAsync will occur. This means ValidationInterval = TimeSpan.Zero
            // leads to calling the ValidateSecurityStampAsync for each request.
            options.ValidationInterval = TimeSpan.FromMinutes(15); // default: 30 minutes
        });

        services.ConfigureApplicationCookie(options =>
        {
            // Cookie settings
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            options.CookieManager = new LeagueCookieManager();
            options.Cookie.Name = ".Auth"; // will be set by LeagueCookieManager
            options.Cookie.Path = "/"; // may be set by LeagueCookieManager
            options.LoginPath = new PathString("/account/sign-in");
            options.LogoutPath = new PathString("/account/sign-in");
            options.AccessDeniedPath = new PathString("/error/access-denied");
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; // don't use Strict here
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToAccessDenied = context =>
            {
                var returnUrl = "?ReturnUrl=" + context.Request.Path + context.Request.QueryString;
                // fires with [Authorize] attribute, when the user is authenticated, but does not have enough privileges
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                // other context properties can be set, but are not considered in the redirect, though
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.AccessDeniedPath) + returnUrl);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToLogin = context =>
            {
                var returnUrl = "?ReturnUrl=" + context.Request.Path + context.Request.QueryString;
                // fires with [Authorize] attribute, when the user is not authenticated
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                // other context properties can be set, but are not considered in the redirect, though
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.LoginPath) + returnUrl);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToLogout = context =>
            {
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.LogoutPath));
                return Task.CompletedTask;
            };
            options.Events.OnSignedIn += async context =>
            {
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                if (context.Principal?.Identity?.Name != null)
                {
                    var success = await tenantContext.DbContext.AppDb.UserRepository.SetLastLoginDateAsync(context.Principal.Identity.Name, null, CancellationToken.None);
                }
            };
        });

        services.ConfigureExternalCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
            // MUST use the default options.CookieManager: After callback from social login, tenant url segment will not be set,
            // because the CallbackPath from the provider will be sth. like "/signin-facebook" with cookie path to the same path.
            options.Cookie.Name = ".ExtAuth.League";
            options.Cookie.Path = "/";
            options.LoginPath = new PathString("/account/sign-in");
            options.LogoutPath = new PathString("/account/sign-in");
            options.AccessDeniedPath = new PathString("/account/access-denied");
            options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Lax; // don't use Strict here
            options.ExpireTimeSpan = TimeSpan.FromDays(30);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToAccessDenied = context =>
            {
                var returnUrl = "?ReturnUrl=" + context.Request.Path + context.Request.QueryString;
                // fires with [Authorize] attribute, when the user is authenticated, but does not have enough privileges
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                // other context properties can be set, but are not considered in the redirect, though
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.AccessDeniedPath) + returnUrl);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToLogin = context =>
            {
                var returnUrl = "?ReturnUrl=" + context.Request.Path + context.Request.QueryString;
                // fires with [Authorize] attribute, when the user is not authenticated
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                // other context properties can be set, but are not considered in the redirect, though
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.LoginPath) + returnUrl);
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToLogout = context =>
            {
                var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
                context.Response.Redirect(new PathString($"/{tenantContext.SiteContext.UrlSegmentValue}").Add(context.Options.LogoutPath));
                return Task.CompletedTask;
            };
        });

        #endregion

        #region *** MailMergeLib as a service ***

        services.AddMailMergeService(
            options =>
            {
                options.Settings = Settings.Deserialize(
                    Path.Combine(environment.ContentRootPath, ConfigurationFolder,
                        $@"MailMergeLib.{environment.EnvironmentName}.config"),
                    System.Text.Encoding.UTF8);
                var fms = FileMessageStore.Deserialize(Path.Combine(environment.ContentRootPath, ConfigurationFolder,
                    "MailMergeLibMessageStore.config"), System.Text.Encoding.UTF8)!;
                for (var i = 0; i < fms.SearchFolders.Length; i++)
                {
                    // make relative paths absolute - ready to use
                    fms.SearchFolders[i] = Path.Combine(environment.WebRootPath, fms.SearchFolders[i]);
                }
                options.MessageStore = fms;
            });

        #endregion

        #region ** Timezone service per request **

        services.AddSingleton<NodaTime.TimeZones.DateTimeZoneCache>(sp =>
            new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default));

        var tzId = configuration.GetSection("TimeZone").Value ?? "America/New_York";
        // TimeZoneConverter will use the culture of the current scope
        services.AddScoped(sp => new Axuno.Tools.DateAndTime.TimeZoneConverter(
            sp.GetRequiredService<NodaTime.TimeZones.DateTimeZoneCache>(), tzId, CultureInfo.CurrentCulture,
            NodaTime.TimeZones.Resolvers.LenientResolver));

        #endregion

        #region ** Phone number service **

        services.AddSingleton<TournamentManager.DI.PhoneNumberService>(sp =>
            new PhoneNumberService(PhoneNumbers.PhoneNumberUtil.GetInstance()));

        #endregion

        services.AddSingleton<Helpers.MetaDataHelper>();
        services.AddTransient<Middleware.ClientAbortMiddleware>();

        services.Configure<CookiePolicyOptions>(options =>
        {
            // determines whether user consent for non-essential cookies is needed for a given request.
            options.CheckConsentNeeded = _ => false;
            options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Lax;
            options.Secure = CookieSecurePolicy.SameAsRequest; // SameSite=None requires Secure
        });

        // expand search path to tenant key sub-paths per HttpRequest
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new Views.LeagueViewLocationExpander()); // R# will not resolve
            // R# will resolve: options.ViewLocationFormats.Add("/Views/Path/{0}.cshtml");
        });

        #region *** Request Localization ***

        if (bool.TryParse(configuration.GetSection("CultureInfo:CulturePerRequest").Value, out var isCulturePerRequest) && isCulturePerRequest)
        {
            var supportedCultures = new[]
            {
                new CultureInfo("en"),
                new CultureInfo("de")
            };

            services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(CultureInfo.DefaultThreadCurrentCulture);
                // Formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;
                // UI strings that we have localized.
                options.SupportedUICultures = supportedCultures;
                // e.g.: "en-US" => "en"
                options.FallBackToParentCultures = true;
                options.FallBackToParentUICultures = true;

                // Select the CookieRequestCultureProvider from the default RequestCultureProviders
                // and set another cookie name than CookieRequestCultureProvider.DefaultCookieName
                var cookieProvider = options.RequestCultureProviders
                    .OfType<CookieRequestCultureProvider>()
                    .FirstOrDefault();
                if (cookieProvider != null) cookieProvider.CookieName = ".PreferredLanguage";
            });
        }

        #endregion

        services.AddRouting(options =>
            {
                options.ConstraintMap.Add(TenantRouteConstraint.Name, typeof(TenantRouteConstraint));
                options.LowercaseQueryStrings = false; // true does not work for UrlBase64 encoded strings!
                options.LowercaseUrls = true;
                options.AppendTrailingSlash = false;
            }
        );

        services.AddRazorPages().AddRazorPagesOptions(options => {  });

        // Custom ViewLocalizer, necessary for views located in a library (still required in NET60):
        services.AddTransient<IViewLocalizer, League.Views.ViewLocalizer>();

        var mvcBuilder = services.AddMvc(options =>
            {
                options.EnableEndpointRouting = true; // the default
                // Add model binding messages for errors that do not reach data annotation validation
                options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueMustNotBeNull));
                options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, val) => string.Format(Resources.ModelBindingMessageResource.AttemptedValueIsInvalid, x, val));
                options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueIsInvalid, x));
                options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueMustBeANumber, x));
                options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => Resources.ModelBindingMessageResource.MissingKeyOrValue);
            })
            .AddSessionStateTempDataProvider()
            .AddDataAnnotationsLocalization()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
            .AddMvcOptions(options =>
            {
                // Insert custom model binder providers before SimpleTypeModelBinderProvider
                options.ModelBinderProviders.Insert(0, new StringTrimmingModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider());
                options.ModelBinderProviders.Insert(0, new TimeOnlyModelBinderProvider());
            })
            .AddViewOptions(options => options.HtmlHelperOptions.ClientValidationEnabled = false)
            .AddControllersAsServices(); // will add controllers with ServiceLifetime.Transient

        #region *** Text Templating ***
        
        services.AddTextTemplatingModule(vfs =>
            {
                // The complete Templates folder is embedded in the project file
                vfs.FileSets.AddEmbedded<LeagueTemplateRenderer>(nameof(League) + ".Templates");
                // vfs.FileSets.AddPhysical(Path.Combine(Directory.GetCurrentDirectory(), "Templates"));
            },
            locOpt =>
            {
            },
            renderOptions =>
            {
#if DEBUG
                renderOptions.MemberNotFoundAction = RenderErrorAction.ThrowError;
                renderOptions.VariableNotFoundAction = RenderErrorAction.ThrowError;
#else
                renderOptions.MemberNotFoundAction = RenderErrorAction.MaintainToken;
                renderOptions.VariableNotFoundAction = RenderErrorAction.MaintainToken;
#endif
            });
        
        #endregion

        #region *** HostedServices related ***

        // RazorViewToStringRenderer must be used with the current HttpContext
        // Note: RazorViewToStringRenderer is currently only used for MatchReport
        services.AddTransient<RazorViewToStringRenderer>();
        services.Configure<BackgroundQueueConfig>(config => config.OnException = _ => { });
        services.AddSingleton<IBackgroundQueue, BackgroundQueue>();
        services.AddConcurrentBackgroundQueueService();
        services.AddTransient<SendEmailTask>();
        services.AddTransient<RankingUpdateTask>();

        #endregion
    }

    /// <summary>
    /// This method MUST be called from the derived class.
    /// It is used to configure the <see cref="League"/> Razor Class Library.
    /// </summary>
    /// <param name="app"></param>
    /// <param name="loggerFactory"></param>
    public static void Configure(WebApplication app, ILoggerFactory loggerFactory)
    {
        var environment = app.Environment;

        var configuration = app.Configuration;

        #region *** Logging ***

        var logger = loggerFactory.CreateLogger(nameof(LeagueStartup));

        // add the JSNLog middleware before the UseStaticFiles middleware.
        var jsNLogConfiguration =
            new JsnlogConfiguration
            {
                loggers = new List<Logger>
                {
                    new() { name = "JsLogger" }
                }
            };
        app.UseJSNLog(new LoggingAdapter(loggerFactory), jsNLogConfiguration);

        #endregion

        #region *** Ensure file system setup ***

        foreach (var folder in new[]{ RankingUpdateTask.RankingImageFolder, Models.UploadViewModels.TeamPhotoStaticFile.TeamPhotoFolder})
        {
            var folderName = Path.Combine(environment.WebRootPath, folder);
            if (!Directory.Exists(folderName))
            {
                logger.LogInformation("Folder '{folderName}' does not exist.", folderName);
                try
                {
                    Directory.CreateDirectory(folderName);
                    logger.LogInformation("Folder '{folderName}' created.", folderName);
                }
                catch (Exception e)
                {
                    logger.LogCritical(e, "Folder '{folderName}': Does not exist and could not be created.", folderName);
                }
            }
        }

        #endregion

        #region *** Static files ***
        // For static files in the wwwroot folder
        app.UseDefaultFiles();

        app.UseStaticFiles(new StaticFileOptions
        {
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromHours(1)
                };
            }
        });
        // For static files using a content type provider:
        var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
        // Make sure .webmanifest files don't cause a 404
        provider.Mappings[".webmanifest"] = "application/manifest+json";
        app.UseStaticFiles(new StaticFileOptions
        {
            ContentTypeProvider = provider,
            OnPrepareResponse = ctx =>
            {
                var headers = ctx.Context.Response.GetTypedHeaders();
                headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = TimeSpan.FromHours(1)
                };
            }
        });
        #endregion

        app.UseCookiePolicy();

        // Must be before UseMvc to avoid InvalidOperationException
        app.UseSession();

        if (bool.TryParse(configuration.GetSection("CultureInfo:CulturePerRequest").Value, out var isCulturePerRequest) && isCulturePerRequest)
            app.UseRequestLocalization(); // options are defined in services

        app.UseRouting();

        // UseAuthentication and UseAuthorization: after UseRouting and UseCors, but before UseEndpoints
        app.UseAuthentication();
        app.UseAuthorization();

        // Suppress exceptions when the connection is closed by the client
        // Controllers and Razor Pages should be next in sequence
        app.UseMiddleware<Middleware.ClientAbortMiddleware>();
        app.MapControllers();
        app.MapRazorPages();
    }

    /// <summary>
    /// Initializes the ranking table and charts on startup.
    /// </summary>
    /// <param name="tenantStore"></param>
    /// <param name="queue"></param>
    /// <param name="serviceProvider"></param>
    public static void InitializeRankingAndCharts(TenantStore tenantStore, IBackgroundQueue queue, IServiceProvider serviceProvider)
    {
        foreach (var tenant in tenantStore.GetTenants().Values.Where(tc => !(string.IsNullOrEmpty(tc.Identifier) || tc.IsDefault)))
        {
            var rankingUpdateTask = serviceProvider.GetRequiredService<RankingUpdateTask>();
            rankingUpdateTask.TenantContext = tenant;
            rankingUpdateTask.TournamentId = tenant.TournamentContext.MatchResultTournamentId;
            rankingUpdateTask.Timeout = TimeSpan.FromMinutes(5);
            rankingUpdateTask.EnforceUpdate = false;
            queue.QueueTask(rankingUpdateTask);
        }
    }
}
