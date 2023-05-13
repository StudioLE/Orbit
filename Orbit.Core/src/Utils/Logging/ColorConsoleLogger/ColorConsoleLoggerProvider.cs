using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.ColorConsoleLogger;

/// <summary>
/// A factory to create <see cref="ColorConsoleLogger"/>.
/// </summary>
[ProviderAlias("ColorConsole")]
public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new ColorConsoleLogger();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
    }
}
