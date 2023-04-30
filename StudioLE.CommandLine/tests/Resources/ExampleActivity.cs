using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace StudioLE.CommandLine.Tests.Resources;

public class ExampleActivity : IActivity<ExampleClass, ExampleClass>
{
    private readonly ILogger<ExampleActivity> _logger;

    public ExampleActivity(ILogger<ExampleActivity> logger)
    {
        _logger = logger;
    }

    public Task<ExampleClass> Execute(ExampleClass example)
    {
        ISerializer serializer = new SerializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        string yaml = serializer.Serialize(example);
        _logger.LogInformation(yaml);

        return Task.FromResult(example);
    }
}
