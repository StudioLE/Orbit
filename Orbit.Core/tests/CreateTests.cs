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
        IResult createdResult = Create.Execute(instance);
        createdResult.ThrowOnFailure();

        // Assert
        IResult<Instance> apiResult = Api.TryGetInstance(instance.Id);
        Instance apiInstance = apiResult.GetValueOrThrow();
        Assert.That(instance.Id, Is.EqualTo(apiInstance.Id));
    }
}
