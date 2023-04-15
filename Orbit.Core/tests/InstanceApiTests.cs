using StudioLE.Core.System;

namespace Orbit.Core.Tests;

internal sealed class InstanceApiTests
{
    [Test]
    public void InstanceApi_GetIds()
    {
        // Arrange
        // Act
        string[] nodeIds = InstanceApi.GetIds().ToArray();
        Console.WriteLine(nodeIds.Join());

        // Assert
        Assert.That(nodeIds, Is.Not.Empty);
    }
}
