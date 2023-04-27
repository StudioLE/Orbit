using Microsoft.Extensions.Logging;

namespace StudioLE.CommandLine.Utils.Logging.TestLogger;

public class TestLogger : ILogger
{
    private readonly List<TestLog> _logs = new();

    public IReadOnlyCollection<TestLog> Logs => _logs;

    private TestLogger()
    {
    }

    public void Clear()
    {
        _logs.Clear();
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
        if (_instance == null)
            lock (_lock)
                _instance ??= new();
        return _instance;
    }
}
