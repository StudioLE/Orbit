using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// An <see cref="ILogger"/> which stores a collection of all logs to review later.
/// </summary>
public class TestLogger : ILogger
{
    private readonly string _categoryName;
    private readonly Action<TestLog> _onLog;

    internal TestLogger(string categoryName, Action<TestLog> onLog)
    {
        _categoryName = categoryName;
        _onLog = onLog;
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _onLog.Invoke(new()
        {
            Category = _categoryName,
            LogLevel = logLevel,
            EventId = eventId,
            State = typeof(TState),
            Exception = exception,
            Message = formatter.Invoke(state, exception)
        });
    }

    /// <inheritdoc />
    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    /// <inheritdoc />
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return default;
    }
}
