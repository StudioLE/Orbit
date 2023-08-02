using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class GenerateServerConfigurationTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CommandContext _context;
    private readonly GenerateServerConfiguration _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public GenerateServerConfigurationTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _context = host.Services.GetRequiredService<CommandContext>();
        _activity = host.Services.GetRequiredService<GenerateServerConfiguration>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateServerConfiguration_Execute()
    {
        // Arrange
        GenerateServerConfiguration.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        GenerateServerConfiguration.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(_context.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated server configuration"));
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateServerConfiguration.FileName);
        Assert.That(resource, Is.Not.Null);
        await _verify.String(resource!);
    }
}
