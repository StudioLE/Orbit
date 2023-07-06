using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// A factory to create <see cref="TestLogger"/>.
/// </summary>
[ProviderAlias("Test")]
public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<TestLog> _logs = new();
    private ILogger? _logger;

    /// <summary>
    /// The logs.
    /// </summary>
    public IReadOnlyCollection<TestLog> Logs => _logs.AsReadOnly();

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        _logger ??= new TestLogger(_logs.Add);
        return _logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
