using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Core.Activities;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
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
    public void Create_Execute_Default()
    {
        // Arrange
        TestLogger logger = TestLogger.GetInstance();
        Instance sourceInstance = new();
        Create create = _host.Services.GetRequiredService<Create>();

        // Act
        Instance? createdInstance = create.Execute(sourceInstance);

        // Assert
        if(createdInstance is null)
            Assert.Fail();
        else
            Assert.That(sourceInstance.Id, Is.EqualTo(createdInstance.Id));

        Assert.That(logger.Logs.Count, Is.EqualTo(1));
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo("Created instance 01-01"));
    }
}
