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

internal sealed class GenerateServerConfigurationTests
{
    private readonly IContext _context = new NUnitContext();
    private CommandContext _commandContext = null!;
    private GenerateServerConfiguration _activity = null!;
    private IEntityProvider<Instance> _instances = null!;
    private IEntityProvider<Client> _clients = null!;
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
        _activity = provider.GetRequiredService<GenerateServerConfiguration>();
        _instances = provider.GetRequiredService<IEntityProvider<Instance>>();
        _clients = provider.GetRequiredService<IEntityProvider<Client>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateServerConfiguration_Instance_Execute()
    {
        // Arrange
        GenerateServerConfiguration.Inputs inputs = new()
        {
            Instance = MockConstants.InstanceName
        };

        // Act
        GenerateServerConfiguration.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated server configuration"));
        string? resource = _instances.GetResource(new InstanceId(inputs.Instance), GenerateServerConfiguration.FileName);
        Assert.That(resource, Is.Not.Null);
        await _context.Verify(resource!);
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateServerConfiguration_Client_Execute()
    {
        // Arrange
        GenerateServerConfiguration.Inputs inputs = new()
        {
            Client = MockConstants.ClientName
        };

        // Act
        GenerateServerConfiguration.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated server configuration"));
        string? resource = _clients.GetResource(new ClientId(inputs.Client), GenerateServerConfiguration.FileName);
        Assert.That(resource, Is.Not.Null);
        await _context.Verify(resource!);
    }
}
