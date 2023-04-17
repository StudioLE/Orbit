using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Orbit.Core.Utils.Logging;

// [UnsupportedOSPlatform("browser")]
[ProviderAlias("ColorConsole")]
public sealed class ColorConsoleLoggerProvider : ILoggerProvider
{
    private readonly IDisposable? _onChangeToken;
    private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers =
        new(StringComparer.OrdinalIgnoreCase);
    private ColorConsoleLoggerConfiguration _currentConfig;

    public ColorConsoleLoggerProvider(
        IOptionsMonitor<ColorConsoleLoggerConfiguration> config)
    {
        _currentConfig = config.CurrentValue;
        _onChangeToken = config.OnChange(updatedConfig => _currentConfig = updatedConfig);
    }

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new(name, GetCurrentConfig));
    }

    private ColorConsoleLoggerConfiguration GetCurrentConfig()
    {
        return _currentConfig;
    }

    public void Dispose()
    {
        _loggers.Clear();
        _onChangeToken?.Dispose();
    }
}
