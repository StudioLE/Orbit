using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Configuration;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Extensions.Logging.Cache;

namespace Orbit.Core.Tests.Configuration;

internal sealed class ClientServerConfigActivityTests
{
    private ClientServerConfigActivity _activity = null!;
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
        _activity = provider.GetRequiredService<ClientServerConfigActivity>();
        _provider = provider.GetRequiredService<ServerConfigurationProvider>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task ClientServerConfigActivity_Execute()
    {
        // Arrange
        ClientServerConfigActivity.Inputs inputs = new()
        {
            Client = new(MockConstants.ClientName)
        };

        // Act
        ClientServerConfigActivity.Outputs outputs = await _activity.Execute(inputs);

        // Assert
        Assert.That(outputs.Status.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        ServerConfiguration? config = await _provider.Get(inputs.Client);
        Assert.That(config, Is.Not.Null);
    }
}
