using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

public static class TestLoggerExtensions
{
    public static ILoggingBuilder AddTestLogger(this ILoggingBuilder builder)
    {
        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, TestLoggerProvider>());
        return builder;
    }
}
