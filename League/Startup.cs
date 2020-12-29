#region ** Usings **

using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Caching;
using JSNLog;
using League.DI;
using League.Identity;
using League.ModelBinders;
using League.Routing;
using MailMergeLib;
using MailMergeLib.AspNet;
using MailMergeLib.MessageStore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NLog.Web;
using SD.LLBLGen.Pro.ORMSupportClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Axuno.BackgroundTask;
using League.BackgroundTasks;
using TournamentManager.Data;
using League.BackgroundTasks.Email;
using League.ConfigurationPoco;
using League.MultiTenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Rewrite;
using TournamentManager.DI;
using TournamentManager.MultiTenancy;
using SiteContext = League.DI.SiteContext;

#endregion

namespace League
{
    public class Startup
    {
        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="webHostEnvironment"></param>
        public Startup(IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
        {
            Configuration = configuration;
            WebHostEnvironment = webHostEnvironment;
        }

        private void ConfigureLlblgenPro(TenantStore tenantStore)
        {
            foreach (var tenant in tenantStore.GetTenants().Values)
            {
                var connectionString = Configuration.GetConnectionString(tenant.DbContext.ConnectionKey);
                RuntimeConfiguration.AddConnectionString(tenant.DbContext.ConnectionKey, connectionString);
                // Enable low-level (result set) caching when specified in selected queries
                // The cache of a query can be overwritten using property 'OverwriteIfPresent'
                CacheController.RegisterCache(connectionString, new ResultsetCache());
                CacheController.CachingEnabled = true;
            }

            //RuntimeConfiguration.SetDependencyInjectionInfo(new[] { typeof(TournamentManager.EntityValidators.UserEntityValidator).Assembly }, new[] { "TournamentManager.Validators" });
            
            if (WebHostEnvironment.IsProduction())
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
        /// Gets the application configuration properties of this application.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Gets the information about the web hosting environment of this application.
        /// </summary>
        public IWebHostEnvironment WebHostEnvironment { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
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
                .SetApplicationName("League")
                .SetDefaultKeyLifetime(TimeSpan.FromDays(360))
                .PersistKeysToFileSystem(
                    new DirectoryInfo(Path.Combine(WebHostEnvironment.ContentRootPath, "DataProtectionKeys")))
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
            var regionInfo = new RegionInfo(Configuration.GetSection("RegionInfo").Value ?? "us");
            services.AddSingleton<RegionInfo>(regionInfo);

            // The default culture of this app is "en". Supported cultures: en, de
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(Configuration.GetSection("CultureInfo:Culture").Value ?? $"en-{regionInfo.TwoLetterISORegionName}");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(Configuration.GetSection("CultureInfo:UiCulture").Value ?? $"en-{regionInfo.TwoLetterISORegionName}");
            
            // DO NOT USE `options => options.ResourcesPath = "..."` because then resource files in other locations won't be recognized (e.g. resx in the same folder as the controller class)
            services.AddLocalization();

            #region **** New Multi Tenancy (new in v4.3.0) *****************************

            services.AddSingleton<TournamentManager.MultiTenancy.TenantStore>(sp =>
            {
                var store = new TournamentManager.MultiTenancy.TenantStore(Configuration, sp.GetRequiredService<ILogger<TournamentManager.MultiTenancy.TenantStore>>())
                {
                    GetTenantConfigurationFiles = () =>
                        Directory.GetFiles(Path.Combine(WebHostEnvironment.ContentRootPath, Program.ConfigurationFolder),
                            $"Tenant.*.{WebHostEnvironment.EnvironmentName}.config", SearchOption.TopDirectoryOnly)
                }.LoadTenants();
                ConfigureLlblgenPro(store);
                return store;
            });
            
            services.AddScoped<MultiTenancy.TenantResolver>();
            services.AddScoped<TournamentManager.MultiTenancy.ITenantContext>(sp => sp.GetRequiredService<MultiTenancy.TenantResolver>().Resolve());
            
            #endregion

            #region **** Obsolete Multi Tenancy (bridge to new Multi Tenancy) **********
            
#pragma warning disable 618
            services.AddSingleton<OrganizationContextResolver>(sp =>

            {
                var tenantStore = sp.GetRequiredService<TournamentManager.MultiTenancy.TenantStore>();
                return new OrganizationContextResolver(tenantStore,
                        sp.GetRequiredService<ILogger<OrganizationContextResolver>>());
            });

            services.AddScoped<SiteContext>();
#pragma warning restore 618           
            #endregion
            
            services.Configure<IISOptions>(options => { });

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            // Make UrlHelper injectable to any component in the HttpContext
            services.AddScoped<IUrlHelper>(sp => {
                var actionContext = sp.GetRequiredService<IActionContextAccessor>().ActionContext;
                var factory = sp.GetRequiredService<IUrlHelperFactory>();
                return factory.GetUrlHelper(actionContext);
            });

            services.AddSingleton<IConfiguration>(Configuration);

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
           
            var socialLogins = Configuration.GetSection(nameof(SocialLogins)).Get<SocialLogins>();
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
                            new Dictionary<string, string>
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
                            new Dictionary<string, string>
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
                            new Dictionary<string, string>
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
                    Configuration.Bind("IdentityOptions", options); // bind to IdentityOptions section of appsettings.json
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
            services.Configure<LeagueUserValidatorOptions>(Configuration.GetSection(nameof(LeagueUserValidatorOptions)));

            #endregion

            #region *** Authorization ***

            services.AddAuthorization(options =>
            {
                // Used on controller method level
                options.AddPolicy(Authorization.PolicyName.MatchPolicy, policy => policy.RequireRole(Identity.Constants.RoleName.SystemManager, Identity.Constants.RoleName.TournamentManager, Identity.Constants.RoleName.TeamManager));
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
                    var success = await tenantContext.DbContext.AppDb.UserRepository.SetLastLoginDateAsync(context.Principal.Identity.Name, null, CancellationToken.None);
                };
            });
            
            services.ConfigureExternalCookie(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                // MUST use the default options.CookieManager: After callback from social login, "organization" url segment will not be set,
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
                        Path.Combine(WebHostEnvironment.ContentRootPath, Program.ConfigurationFolder,
                            $@"MailMergeLib.{WebHostEnvironment.EnvironmentName}.config"),
                        System.Text.Encoding.UTF8);
                    var fms = FileMessageStore.Deserialize(Path.Combine(WebHostEnvironment.ContentRootPath, Program.ConfigurationFolder,
                        "MailMergeLibMessageStore.config"), System.Text.Encoding.UTF8);
                    for (var i = 0; i < fms.SearchFolders.Length; i++)
                    {
                        // make relative paths absolute - ready to use
                        fms.SearchFolders[i] = Path.Combine(WebHostEnvironment.WebRootPath, fms.SearchFolders[i]);
                    }
                    options.MessageStore = fms;
                });

            #endregion

            #region ** Timezone service per request **

            services.AddSingleton<NodaTime.TimeZones.DateTimeZoneCache>(sp =>
                new NodaTime.TimeZones.DateTimeZoneCache(NodaTime.TimeZones.TzdbDateTimeZoneSource.Default));
            
            var tzId = Configuration.GetSection("TimeZone").Value ?? "America/New_York";
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

            services.Configure<CookiePolicyOptions>(options =>
            {
                // determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
            });

            // expand search path to organization key sub-paths per HttpRequest
            services.Configure<RazorViewEngineOptions>(options =>
                {
                    options.ViewLocationExpanders.Add(new Views.LeagueViewLocationExpander()); // R# will not resolve
                    options.ViewLocationFormats.Add("/Views/Emails/{0}.cshtml"); // R# will resolve
                });

            #region *** Request Localization ***

            if (bool.Parse(Configuration.GetSection("CultureInfo:CulturePerRequest").Value))
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

            var mvcBuilder = services.AddMvc(options =>
                {
                    options.EnableEndpointRouting = true;
                    // Add model binding messages for errors that do not reach data annotation validation
                    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueMustNotBeNull));
                    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, val) => string.Format(Resources.ModelBindingMessageResource.AttemptedValueIsInvalid, x, val));
                    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueIsInvalid, x));
                    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x => string.Format(Resources.ModelBindingMessageResource.ValueMustBeANumber, x));
                    options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => Resources.ModelBindingMessageResource.MissingKeyOrValue);
                })
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddSessionStateTempDataProvider()
                .AddDataAnnotationsLocalization()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddMvcOptions(options =>
                {
                    // Insert custom model binder providers before SimpleTypeModelBinderProvider
                    options.ModelBinderProviders.Insert(0, new TimeSpanModelBinderProvider());
                    options.ModelBinderProviders.Insert(0, new DateTimeModelBinderProvider());
                    // Replace ComplexTypeModelBinder with TrimmingModelBinder (trims all strings in models)
                    options.ModelBinderProviders[options.ModelBinderProviders.TakeWhile(p => !(p is Microsoft.AspNetCore.Mvc.ModelBinding.Binders.ComplexTypeModelBinderProvider)).Count()] = new ModelBinders.TrimmingComplexModelBinderProvider();
                })
                .AddControllersAsServices(); // will add controllers with ServiceLifetime.Transient
#if DEBUG
            // Not to be added in production!
            if (WebHostEnvironment.IsDevelopment())
            {
                mvcBuilder.AddRazorRuntimeCompilation();
            }
#endif
            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = StatusCodes.Status301MovedPermanently;
            });

            #region *** Add CloudScribeNavigation ***

            // CloudscribeNavigation requires:
            // ~/Views/Shared/NavigationNodeChildDropdownPartial.cshtml
            // ~/Views/Shared/NavigationNodeChildTreePartial.cshtml
            // ~/Views/Shared/NavigationNodeSideNavPartial.cshtml
            // ~/Views/Shared/Components/Navigation/*.cshtml
            // ~/Views/_ViewImports.cshtml: @using cloudscribe.Web.Navigation

            //services.AddCloudscribeNavigation(Configuration.GetSection("NavigationOptions"));
            services.AddCloudscribeNavigation(null);
            services.AddScoped<IOptions<NavigationOptions>, Navigation.LeagueSiteNavigationOptionsResolver>(); // resolve navigation xml files per organization
            services.AddScoped<INavigationTreeBuilder, Navigation.LeaguesNavigationTreeBuilder>(); //add top nav item for all leagues
            services.AddScoped<INavigationTreeBuilder, Navigation.InfosNavigationTreeBuilder>(); //add top nav item for info menu
            services.AddScoped<ITreeCache, Navigation.LeagueMemoryTreeCache>(); // cache navigation tree per organization
            #endregion

            #region *** HostedServices related ***
            
            services.AddSingleton<BackgroundWebHost>(sp => new BackgroundWebHost(services));
            services.Configure<BackgroundQueueConfig>(config => config.OnException = null);
            services.AddSingleton<IBackgroundQueue, BackgroundQueue>();
            services.AddConcurrentBackgroundQueueService();
            services.AddTransient<UserEmailTask>();
            services.AddTransient<FixtureEmailTask>();
            services.AddTransient<ResultEmailTask>();
            services.AddTransient<RankingUpdateTask>();
            services.AddTransient<ContactEmailTask>();
            services.AddTransient<TeamApplicationEmailTask>();
            
            #endregion
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="loggerFactory"></param>
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            #region *** Logging ***

            // Add NLog as logging provider. NLog config is set in Program.Main()
            // see: https://github.com/NLog/NLog/issues/2859
            app.ApplicationServices.SetupNLogServiceLocator();

            // Allow TournamentManager to make use of Microsoft.Extensions.Logging
            TournamentManager.AppLogging.Configure(loggerFactory);

            #endregion

            #region *** Rewrite ALL domains (even those without SSL certificate) to https://volleyball-liga.de ***

            {
                using var iisUrlRewriteStreamReader = File.OpenText(Path.Combine(WebHostEnvironment.ContentRootPath, Program.ConfigurationFolder, @"IisRewrite.config"));
                var options = new RewriteOptions().AddIISUrlRewrite(iisUrlRewriteStreamReader);
                app.UseRewriter(options);
            }
            
            #endregion

            if (env.IsDevelopment())
            {
                app.UseMiddleware<StackifyMiddleware.RequestTracerMiddleware>();
                // Turn production error behavior on=false/off=true
                if (true)
                {
                    app.UseDeveloperExceptionPage();
                    app.UseStatusCodePages();
                }
                else
                {
                    app.UseStatusCodePagesWithReExecute($"/{nameof(Controllers.Error)}/{{0}}");
                    app.UseExceptionHandler($"/{nameof(Controllers.Error)}/500");  
                }
            }
            else
            {
                app.UseStatusCodePagesWithReExecute($"/{nameof(Controllers.Error)}/{{0}}");
                app.UseExceptionHandler($"/{nameof(Controllers.Error)}/500");
                // instruct the browsers to always access the site via HTTPS
                app.UseHsts();
            }

            #region *** JsNLog ***
            // add the JSNLog middleware before the UseStaticFiles middleware. 
            var jsNLogConfiguration =
                new JsnlogConfiguration
                {
                    loggers = new List<Logger>
                    {
                        new Logger { name = "JsLogger" }
                    }
                };
            app.UseJSNLog(new LoggingAdapter(loggerFactory), jsNLogConfiguration);
            #endregion

            app.UseHttpsRedirection();

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
                        MaxAge = TimeSpan.FromDays(30)
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
                        MaxAge = TimeSpan.FromDays(30)
                    };
                }
            });
            #endregion

            app.UseCookiePolicy();
            
            // Must be before UseMvc to avoid InvalidOperationException
            app.UseSession();

            if (bool.Parse(Configuration.GetSection("CultureInfo:CulturePerRequest").Value))
                app.UseRequestLocalization(); // options are defined in services

            app.UseRouting();

            // UseAuthentication and UseAuthorization: after UseRouting and UseCors, but before UseEndpoints
            app.UseAuthentication();
            app.UseAuthorization();
            
            /* Before using endpoint routing, all anchor, form tags Url.(...) and RedirectToAction(...) tag helpers had to be updated with {organization}
               (ViewContext.RouteData.Values["organization"]) because ambient parameters are not preserved here (as opposed to IRoute) */
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            #region *** Initialize ranking tables and charts ***
            
            var tenantStore = app.ApplicationServices.GetRequiredService<TenantStore>();
            var queue = app.ApplicationServices.GetRequiredService<IBackgroundQueue>();
            
            foreach (var tenant in tenantStore.GetTenants().Values.Where(tc => !(string.IsNullOrEmpty(tc.Identifier) || tc.IsDefault)))  
            {
                var rankingUpdateTask = app.ApplicationServices.GetRequiredService<RankingUpdateTask>();
                rankingUpdateTask.TenantContext = tenant;
                rankingUpdateTask.TournamentId = tenant.TournamentContext.MatchResultTournamentId;
                rankingUpdateTask.Timeout = TimeSpan.FromMinutes(5);
                rankingUpdateTask.EnforceUpdate = false;
                queue.QueueTask(rankingUpdateTask);
            }
            
            #endregion
        }
    }
}
