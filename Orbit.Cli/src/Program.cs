using Orbit.Cli;
using Orbit.Cli.Utils.Converters;

ConverterResolver resolver = DependencyInjectionHelper.DefaultConverterResolver();
Cli entry = new(args, resolver);
await entry.Run();
