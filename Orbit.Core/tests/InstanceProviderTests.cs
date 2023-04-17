using Orbit.Core.Schema;

namespace Orbit.Core.Tests;

internal sealed class InstanceProviderTests
{
    private readonly InstanceProvider _provider = InstanceProvider.CreateTemp();

    [Test]
    public void InstanceProvider_In_Sequence()
    {
        InstanceProvider_Get_Empty();
        InstanceProvider_GetAllIds_Empty();
        InstanceProvider_Put();
        InstanceProvider_Get();
        InstanceProvider_GetAllIds();
    }

    private void InstanceProvider_Get_Empty()
    {
        // Arrange
        // Act
        Instance? instance = _provider.Get("01-01");

        // Assert
        Assert.That(instance, Is.Null);
    }

    private void InstanceProvider_GetAllIds_Empty()
    {
        // Arrange
        // Act
        string[] ids = _provider.GetAllIds().ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(0));
    }

    private void InstanceProvider_Put()
    {
        // Arrange
        Instance instance = new();
        instance.Review(_provider);

        // Act
        bool result = _provider.Put(instance);

        // Assert
        Assert.That(result, Is.True);
    }

    private void InstanceProvider_Get()
    {
        // Arrange
        // Act
        Instance? instance = _provider.Get("01-01");

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private void InstanceProvider_GetAllIds()
    {
        // Arrange
        // Act
        string[] ids = _provider.GetAllIds().ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(1));
    }
}
