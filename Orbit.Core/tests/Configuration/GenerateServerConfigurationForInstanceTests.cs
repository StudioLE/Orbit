using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Configuration;
using Orbit.Core.Tests.Resources;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Configuration;

internal sealed class GenerateServerConfigurationForInstanceTests
{
    private readonly IContext _context = new NUnitContext();
    private CommandContext _commandContext = null!;
    private GenerateServerConfigurationForInstance _activity = null!;
    private IEntityProvider<Instance> _instances = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public void SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateServerConfigurationForInstance>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateServerConfigurationForInstance_Execute()
    {
        // Arrange
        GenerateServerConfigurationForInstance.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        Assert.That(output, Is.Empty, "Output");
        string? resource = _instances.GetArtifact(new InstanceId(inputs.Instance), GenerateServerConfigurationForInstance.FileName);
        Assert.That(resource, Is.Not.Null);
        await _context.Verify(resource!);
    }
}
