using System.Runtime.InteropServices;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.CloudInit;
using Orbit.Core.Tests.Resources;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.CloudInit;

internal sealed class GenerateUserConfigTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CommandContext _commandContext;
    private readonly GenerateUserConfig _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public GenerateUserConfigTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateUserConfig>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateUserConfig_Execute()
    {
        // Arrange
        GenerateUserConfig.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        Assert.That(output, Is.Empty, "Output");
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateUserConfig.FileName);
        Assert.That(resource, Is.Not.Null);

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
