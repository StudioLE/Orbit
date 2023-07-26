using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Generation;

internal sealed class GenerateCloudInitTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CommandContext _context;
    private readonly GenerateWireGuard _wireguard;
    private readonly GenerateCloudInit _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public GenerateCloudInitTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _context = host.Services.GetRequiredService<CommandContext>();
        _wireguard = host.Services.GetRequiredService<GenerateWireGuard>();
        _activity = host.Services.GetRequiredService<GenerateCloudInit>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    public async Task GenerateCloudInit_Execute()
    {
        // Arrange
        await GenerateWireGuard();
        GenerateCloudInit.Inputs inputs = new()
        {
            Instance = "instance-01"
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_context.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(2), "Logs Count");
        Assert.That(_logs.ElementAt(1).Message, Is.EqualTo($"Generated cloud-init for instance {inputs.Instance}"));
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateCloudInit.FileName);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(output));
        await _verify.String(resource!);
    }

    private async Task GenerateWireGuard()
    {
        GenerateWireGuard.Inputs inputs = new()
        {
            Instance = "instance-01",
            Interface = "wg1"
        };
        await _wireguard.Execute(inputs);
    }
}
