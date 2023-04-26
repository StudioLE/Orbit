using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Activities;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using Host = Microsoft.Extensions.Hosting.Host;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private readonly IHost _host;

    public CreateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        _host = Host
            .CreateDefaultBuilder()
            .RegisterTestLoggingProviders()
            .RegisterCreateServices()
            .RegisterTestInstanceProvider()
            .Build();
    }

    [Test]
    public async Task Create_Execute_Default()
    {
        // Arrange
        TestLogger logger = TestLogger.GetInstance();
        Instance sourceInstance = new()
        {
            Host = "host-01"
        };
        Create create = _host.Services.GetRequiredService<Create>();

        // Act
        Instance? createdInstance = create.Execute(sourceInstance);

        // Assert
        if (createdInstance is null)
            Assert.Fail();
        else
            await _verify.AsYaml(createdInstance);

        Assert.That(logger.Logs.Count, Is.EqualTo(1));
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo($"Created instance {sourceInstance.Name}"));
    }
}
