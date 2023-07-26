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

internal sealed class GenerateWireGuardTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CommandContext _context;
    private readonly GenerateWireGuard _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public GenerateWireGuardTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        _context = host.Services.GetRequiredService<CommandContext>();
        _activity = host.Services.GetRequiredService<GenerateWireGuard>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
        _logs = host.Services.GetTestLogs();
    }

    [Test]
    public async Task GenerateWireGuard_Execute()
    {
        // Arrange
        GenerateWireGuard.Inputs inputs = new()
        {
            Instance = "instance-01",
            Interface = "wg1"
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_context.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo($"Generated WireGuard config for instance {inputs.Instance}"));
        string fileName = inputs.Interface + ".conf";
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), fileName);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(output));
        await _verify.String(resource!);
    }
}
