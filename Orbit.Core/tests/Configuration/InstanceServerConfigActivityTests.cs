using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Configuration;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using Tectonic;

namespace Orbit.Core.Tests.Configuration;

internal sealed class InstanceServerConfigActivityTests
{
    private readonly IContext _context = new NUnitContext();
    private CommandContext _commandContext = null!;
    private InstanceServerConfigActivity _activity = null!;
    private ServerConfigurationProvider _provider = null!;
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
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(0), "Log Count");
        Assert.That(output, Is.Empty, "Output");
        ServerConfiguration? config = _provider.Get(inputs.Instance);
        Assert.That(config, Is.Not.Null);
    }
}
