using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.ColorConsoleLogger;

/// <summary>
/// Methods to help with <see cref="ColorConsoleLogger"/>
/// </summary>
public static class ColorConsoleLoggerExtensions
{
    /// <summary>
    /// Add a <see cref="ColorConsoleLogger"/>.
    /// </summary>
    public static ILoggingBuilder AddColorConsoleLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, ColorConsoleLoggerProvider>());
        return builder;
    }
}
