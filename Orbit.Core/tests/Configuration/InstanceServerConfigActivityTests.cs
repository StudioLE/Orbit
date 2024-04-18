using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Configuration;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Extensions.Logging.Cache;

namespace Orbit.Core.Tests.Configuration;

internal sealed class InstanceServerConfigActivityTests
{
    private InstanceServerConfigActivity _activity = null!;
    private ServerConfigurationProvider _provider = null!;
    private IReadOnlyCollection<LogEntry> _logs = null!;

    [SetUp]
    public async Task SetUp()
    {
#if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
#endif
        IHost host = await TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _activity = provider.GetRequiredService<InstanceServerConfigActivity>();
        _provider = provider.GetRequiredService<ServerConfigurationProvider>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task InstanceServerConfigActivity_Execute()
    {
        // Arrange
        InstanceServerConfigActivity.Inputs inputs = new()
        {
            Instance = new(MockConstants.InstanceName)
        };

        // Act
        InstanceServerConfigActivity.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(outputs.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        ServerConfiguration? config = await _provider.Get(inputs.Instance);
        Assert.That(config, Is.Not.Null);
    }
}
