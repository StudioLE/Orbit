using Orbit.Core.Activities;
using Orbit.Core.Schema;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    public CreateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
    }

    [Test]
    public void Create_Execute_Default()
    {
        // TODO: Use moq to create a logger
        // https://stackoverflow.com/a/58697253/247218

        // Arrange
        Instance sourceInstance = new();
        Create create = new();

        // Act
        Instance? createdInstance = create.Execute(sourceInstance);

        // Assert
        if(createdInstance is null)
            Assert.Fail();
        else
            Assert.That(sourceInstance.Id, Is.EqualTo(createdInstance.Id));
    }
}
