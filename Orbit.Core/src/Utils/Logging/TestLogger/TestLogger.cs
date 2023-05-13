using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// An <see cref="ILogger"/> which stores a collection of all logs to review later.
/// </summary>
public class TestLogger : ILogger
{
    private readonly List<TestLog> _logs = new();

    /// <summary>
    /// The logs.
    /// </summary>
    public IReadOnlyCollection<TestLog> Logs => _logs;

    private TestLogger()
    {
    }

    /// <inheritdoc />
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logs.Add(new()
        {
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

    // Singleton pattern
    // https://refactoring.guru/design-patterns/singleton/csharp/example#example-1

    private static TestLogger? _instance;
    private static readonly object _lock = new();


    public static TestLogger GetInstance()
    {
        // TODO: Replace this with the IHostBuilder singleton.
        if (_instance == null)
            lock (_lock)
                _instance ??= new();
        return _instance;
    }
}
