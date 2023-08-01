using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Creation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;

namespace Orbit.Core.Tests.Provision;

internal sealed class InstanceProviderTests
{
    private readonly InstanceId _instanceId = new("instance-10");
    private readonly InstanceFactory _instanceFactory;
    private readonly IEntityProvider<Instance> _instances;

    public InstanceProviderTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
    }

    [Test]
    [Category("Misc")]
    public void InstanceProvider_In_Sequence()
    {
        InstanceProvider_Get_Before();
        InstanceProvider_GetIndex_Before();
        InstanceProvider_Put();
        InstanceProvider_Get_After();
        InstanceProvider_GetIndex_After();
    }

    private void InstanceProvider_Get_Before()
    {
        // Arrange
        // Act
        Instance? instance = _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Null);
    }

    private void InstanceProvider_GetIndex_Before()
    {
        // Arrange
        // Act
        string[] ids = _instances.GetIndex().ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(1));
    }

    private void InstanceProvider_Put()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new());

        // Act
        bool result = _instances.Put(instance);

        // Assert
        Assert.That(result, Is.True);
    }

    private void InstanceProvider_Get_After()
    {
        // Arrange
        // Act
        Instance? instance = _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private void InstanceProvider_GetIndex_After()
    {
        // Arrange
        // Act
        string[] ids = _instances.GetIndex().ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(2));
    }
}
