using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace TournamentManager
{
    /// <summary>
    /// Allows for using the <see cref="Microsoft.Extensions.Logging"/> facade in library classes 
    /// without dependency injection.
    /// </summary>
    /// <remarks>
    /// Note: LoggerFactory.CreateLogger is thread-safe.
    /// Idea going back to https://stackify.com/net-core-loggerfactory-use-correctly/
    /// </remarks>
    public static class AppLogging
    {
        private static ILoggerFactory _factory;
        private static bool _useNullLogger;
        
        /// <summary>
        /// Configures the static instance of <see cref="AppLogging"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory"/></param> for creating <see cref="ILogger"/>s.
        public static void Configure(ILoggerFactory factory)
        {
            // ASP.NET will only write its internal logging to the LoggerFactory object 
            // that it creates at app startup.
            // So in Startup.Configure, we grab that reference of the LoggerFactory
            // and set it to this logging class so it becomes the primary reference.
            _factory = factory;
            
            // _factory.AddProvider(new NLog.Extensions.Logging.NLogLoggerProvider());
        }

        /// <summary>
        /// Gets or sets the <see cref="ILoggerFactory"/> for <see cref="AppLogging"/>.
        /// </summary>
        public static ILoggerFactory LoggerFactory
        {
            get
            {
                if (_factory == null)
                {
                    // Default LoggerFactory does not contain any ILoggerProvider 
                    // and must therefore be overwritten with the proper implementation.
                    if (UseNullLogger)
                        Configure(new NullLoggerFactory());
                    else
                        Configure(new LoggerFactory());
                    
                }
                return _factory;
            }

            set => _factory = value;
        }

        /// <summary>
        /// If set to <see langword="true"/>, <see cref="NullLogger"/>s will be created.
        /// Default is <see langword="false"/>.
        /// </summary>
        public static bool UseNullLogger
        {
            get => _useNullLogger;
            set
            {
                _useNullLogger = value;
                _factory = null;
            }
        }

        /// <summary>
        /// Creates an <see cref="ILogger"/> for type <see cref="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>Returns an <see cref="ILogger"/> instance for type <see cref="T"/>.</returns>
        public static ILogger CreateLogger<T>() => LoggerFactory.CreateLogger<T>();

        /// <summary>
        /// Creates an <see cref="ILogger"/> with name <see cref="name"/>.
        /// </summary>
        /// <param name="name">The name for the <see cref="ILogger"/>.</param>
        /// <returns>Returns an <see cref="ILogger"/> with name <see cref="name"/>.</returns>
        public static ILogger CreateLogger(string name) => LoggerFactory.CreateLogger(name);
    }
}