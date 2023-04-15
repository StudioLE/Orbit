using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orbit.Core.Schema;
using Create = Orbit.Core.Activities.Create;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    [Test]
    public void Create_Execute_Default()
    {
        // TODO: Use moq to create a logger
        // https://stackoverflow.com/a/58697253/247218

        // Arrange
        Instance sourceInstance = new();

        string[] args = { "--environment", "Development" };
        using IHost host = Host
            .CreateDefaultBuilder(args)
            .RegisterCreateServices()
            .Build();
        Create create = host.Services.GetRequiredService<Create>();

        // Act
        Instance? createdInstance = create.Execute(sourceInstance);

        // Assert
        if(createdInstance is null)
            Assert.Fail();
        else
            Assert.That(sourceInstance.Id, Is.EqualTo(createdInstance.Id));
    }
}
