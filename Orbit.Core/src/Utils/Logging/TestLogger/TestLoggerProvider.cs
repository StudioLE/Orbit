using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// A factory to create <see cref="TestLogger"/>.
/// </summary>
[ProviderAlias("Test")]
public class TestLoggerProvider : ILoggerProvider
{
    private readonly List<TestLog> _logs = new();

    /// <summary>
    /// The logs.
    /// </summary>
    public IReadOnlyCollection<TestLog> Logs => _logs.AsReadOnly();

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_logs.Add);
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
