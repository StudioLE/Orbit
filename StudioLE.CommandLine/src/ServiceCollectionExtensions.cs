using Microsoft.Extensions.DependencyInjection;

namespace StudioLE.CommandLine;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCommandBuilderServices(this IServiceCollection services)
    {
        return services
            .AddTransient<CommandBuilder>()
            .AddTransient<CommandFactory>()
            .AddTransient<IIsParseableStrategy, IsParseableStrategy>()
            .AddTransient<ICommandOptionsStrategy, CommandOptionsStrategy>()
            .AddTransient<ICommandHandlerStrategy, CommandHandlerStrategy>();
    }
}
