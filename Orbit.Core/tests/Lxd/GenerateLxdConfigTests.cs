using Tectonic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Lxd;
using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;

namespace Orbit.Core.Tests.Lxd;

internal sealed class GenerateLxdConfigTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CommandContext _commandContext;
    private readonly GenerateLxdConfig _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public GenerateLxdConfigTests()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateLxdConfig>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateLxdConfig_Execute()
    {
        // Arrange
        GenerateLxdConfig.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        Assert.That(output, Is.Empty, "Output");
        string? resource = _instances.GetArtifact(new InstanceId(inputs.Instance), GenerateLxdConfig.FileName);
        Assert.That(resource, Is.Not.Null);

        // TODO: We have no easy way to normalize the MacAddresses for unstructured data
        // // Yaml serialization is inconsistent on Windows
        // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //     return;
        // await _context.Verify(resource!);
    }
}
