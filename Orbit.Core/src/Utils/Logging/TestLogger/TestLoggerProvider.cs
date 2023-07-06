using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// A factory to create <see cref="TestLogger"/>.
/// </summary>
[ProviderAlias("Test")]
public class TestLoggerProvider : ILoggerProvider
{
    private ILogger? _logger;

    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        _logger ??= new TestLogger();
        return _logger;
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
