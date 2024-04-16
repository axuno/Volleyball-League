using System.Text;
using Microsoft.Extensions.Logging;

namespace TournamentManager.Tests.TestComponents;

/// <summary>
/// Simple <see cref="ILogger"/> implementation for a unit test logger,
/// which will write log messages to the NUnit output window
/// </summary>
public class NUnitLogger : ILogger, IDisposable
{
    private readonly Action<string> _output = Console.WriteLine;
    private readonly string _category = string.Empty;

    /// <summary>
    /// Gets a new instance of an <see cref="NUnitLogger"/>.
    /// </summary>
    /// <returns>Returns a new instance of an <see cref="NUnitLogger"/>.</returns>
    public static ILogger Create()
    {
        return new NUnitLogger();
    }

    protected static ILogger Create(string category)
    {
        return new NUnitLogger(category);
    }

    /// <summary>
    /// CTOR.
    /// </summary>
    public NUnitLogger()
    { }

    protected NUnitLogger(string category)
    {
        _category = category;
    }

    /// <summary>
    /// Disposes the instance of this logger.
    /// </summary>
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Gets or sets the minimum log level
    /// </summary>
    public LogLevel LogLevel { get; set; } = LogLevel.None;

    /// <summary>
    /// Creates the log entry.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="state"></param>
    /// <param name="exception"></param>
    /// <param name="formatter"></param>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var sb = new StringBuilder();
        sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff zzz"));
        sb.Append(" [");
        sb.Append(logLevel.ToString());
        sb.Append("] ");
        sb.Append(_category);
        sb.Append(": ");

        if (exception != null)
        {
            sb.Append(formatter(state, exception));
            sb.AppendLine(exception.ToString());
        }

        _output(sb.ToString());
    }

    /// <summary>
    /// Determines whether the log entry should be written depending on the <see cref="LogLevel"/>.
    /// </summary>
    /// <param name="logLevel"></param>
    /// <returns>Returns <see langword="true"/> if the log <see cref="LogLevel"/> parameter is greater or equal than the minimum <see cref="LogLevel"/>, else false.</returns>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel;
    }

    /// <summary>
    /// Gets the instance of this logger as a logical operation scope.
    /// </summary>
    /// <typeparam name="TState"></typeparam>
    /// <param name="state"></param>
    /// <returns>Returns the instance of this logger.</returns>
    public IDisposable BeginScope<TState>(TState state) where TState : notnull
    {
        return this;
    }
}
