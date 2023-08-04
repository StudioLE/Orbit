using System.Runtime.InteropServices;
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

internal sealed class GenerateInstanceConfigurationTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly CommandContext _context;
    private readonly GenerateInstanceConfiguration _activity;
    private readonly IEntityProvider<Instance> _instances;
    private readonly IReadOnlyCollection<TestLog> _logs;

    public GenerateInstanceConfigurationTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _context = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateInstanceConfiguration>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _logs = provider.GetTestLogs();
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
        Assert.That(_context.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated instance configuration"));
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateInstanceConfiguration.FileName);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource, Is.EqualTo(output));

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _verify.String(resource!);
    }
}
