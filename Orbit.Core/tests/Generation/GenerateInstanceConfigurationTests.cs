using System.Runtime.InteropServices;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Generation;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Generation;

internal sealed class GenerateInstanceConfigurationTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CommandContext _commandContext;
    private readonly GenerateInstanceConfiguration _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public GenerateInstanceConfigurationTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateInstanceConfiguration>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateInstanceConfiguration_Execute()
    {
        // Arrange
        GenerateInstanceConfiguration.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated instance configuration"));
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateInstanceConfiguration.FileName);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(output));

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
