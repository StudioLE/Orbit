using StudioLE.Core.System;

namespace Orbit.Core.Tests;

internal sealed class ApiTests
{
    [Test]
    public void Api_GetNodeIds()
    {
        // Arrange
        // Act
        string[] nodeIds = Api.GetInstanceIds().ToArray();
        Console.WriteLine(nodeIds.Join());

        // Assert
        Assert.That(nodeIds, Is.Not.Empty);
    }
}
