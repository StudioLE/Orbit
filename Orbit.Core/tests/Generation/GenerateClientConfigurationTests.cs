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

internal sealed class GenerateClientConfigurationTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly CommandContext _commandContext;
    private readonly GenerateClientConfiguration _activity;
    private readonly IEntityProvider<Client> _clients;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public GenerateClientConfigurationTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost();
        using IServiceScope serviceScope = host.Services.CreateScope();
        IServiceProvider provider = serviceScope.ServiceProvider;
        _commandContext = provider.GetRequiredService<CommandContext>();
        _activity = provider.GetRequiredService<GenerateClientConfiguration>();
        _clients = provider.GetRequiredService<IEntityProvider<Client>>();
        _logs = provider.GetCachedLogs();
    }

    [Test]
    [Category("Activity")]
    public async Task GenerateClientConfiguration_Execute()
    {
        // Arrange
        GenerateClientConfiguration.Inputs inputs = new()
        {
            Client = MockConstants.ClientName
        };

        // Act
        string output = await _activity.Execute(inputs);

        // Assert
        Assert.That(_commandContext.ExitCode, Is.EqualTo(0), "ExitCode");
        Assert.That(_logs.Count, Is.EqualTo(1), "Logs Count");
        Assert.That(_logs.ElementAt(0).Message, Is.EqualTo("Generated client configuration"));
        string? resource = _clients.GetResource(new ClientId(inputs.Client), $"wg{MockConstants.NetworkNumber}.conf");
        Assert.That(resource, Is.Not.Null);

        // Yaml serialization is inconsistent on Windows
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return;
        await _context.Verify(resource!);
    }
}
