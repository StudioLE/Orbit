using Orbit.Core.Schema;
using Orbit.Core.Utils;
using StudioLE.Core.Results;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    [Test]
    public void Create_Execute_Default()
    {
        // Arrange
        Instance instance = new();
        // ILoggerFactory loggerFactory = LoggerFactory.Create(_ => { });
        // ILogger<Create> logger = loggerFactory.CreateLogger<Create>();

        // Act
        IResult<Instance> createdResult = Create.Execute(instance);

        // Assert
        Instance createdInstance = createdResult.GetValueOrThrow();
        IResult<Instance> apiResult = Api.TryGetInstance(instance.Id);
        Instance apiInstance = apiResult.GetValueOrThrow();
        Assert.That(createdInstance.Id, Is.EqualTo(apiInstance.Id));
    }
}
