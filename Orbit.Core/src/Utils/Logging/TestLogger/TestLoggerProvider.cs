using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// A factory to create <see cref="TestLogger"/>.
/// </summary>
[ProviderAlias("Test")]
public class TestLoggerProvider : ILoggerProvider
{
    /// <inheritdoc />
    public ILogger CreateLogger(string categoryName)
    {
        return TestLogger.GetInstance();
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}
