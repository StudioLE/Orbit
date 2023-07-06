using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// Methods to help with <see cref="TestLogger"/>.
/// </summary>
public static class TestLoggerExtensions
{
    /// <summary>
    /// Add a <see cref="TestLogger"/>.
    /// </summary>
    public static ILoggingBuilder AddTestLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>());
        return builder;
    }

    public static IReadOnlyCollection<TestLog> GetTestLogs(this IServiceProvider services)
    {
        TestLoggerProvider provider = services
                                          .GetServices<ILoggerProvider>()
                                          .OfType<TestLoggerProvider>()
                                          .FirstOrDefault()
                                      ?? throw new($"Failed to get {nameof(TestLoggerProvider)}.");
        return provider.Logs;
    }
}
