using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Activities;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Activities;

internal sealed class GenerateTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly Generate _generate;
    private readonly IEntityProvider<Instance> _instances;

    public GenerateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _generate = host.Services.GetRequiredService<Generate>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
    }

    [Test]
    public async Task Generate_Execute()
    {
        // Arrange
        TestLogger logger = TestLogger.GetInstance();
        Generate.Inputs inputs = new()
        {
            Instance = "instance-01"
        };

        // Act
        Generate.Outputs outputs = await _generate.Execute(inputs);

        // Assert
        Assert.That(outputs.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(logger.Logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo($"Generated cloud-init for instance {inputs.Instance}"));
        string? userConfig = _instances.GetResource(new InstanceId(inputs.Instance), "user-config.yml");
        Assert.That(userConfig, Is.Not.Null);
        await _verify.String(userConfig!);
    }
}
