using Orbit.Cli.Utils.Converters;
using Orbit.Core.Schema;

namespace Orbit.Cli;

public static class DependencyInjectionHelper
{
    public static ConverterResolver DefaultConverterResolver()
    {
        return new ConverterResolverBuilder()
            .Register<int, StringToInteger>()
            .Register<double, StringToDouble>()
            .Register<Platform, StringToEnum<Platform>>()
            .Build();
    }
}
