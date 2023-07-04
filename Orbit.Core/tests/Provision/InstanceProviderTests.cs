using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Provision;
using Orbit.Core.Schema;

namespace Orbit.Core.Tests.Provision;

internal sealed class InstanceProviderTests
{
    private readonly InstanceId _instanceId = new("instance-01");
    private readonly InstanceFactory _instanceFactory;
    private readonly IEntityProvider<Instance> _instances;

    public InstanceProviderTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _instances = host.Services.GetRequiredService<IEntityProvider<Instance>>();
    }

    [Test]
    public void InstanceProvider_In_Sequence()
    {
        InstanceProvider_Get_Empty();
        InstanceProvider_GetAllNamesInCluster_Empty();
        InstanceProvider_Put();
        InstanceProvider_Get();
        InstanceProvider_GetAllNamesInCluster();
    }

    private void InstanceProvider_Get_Empty()
    {
        // Arrange
        // Act
        Instance? instance = _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Null);
    }

    private void InstanceProvider_GetAllNamesInCluster_Empty()
    {
        // Arrange
        // Act
        string[] names = _instances.GetIndex().ToArray();

        // Assert
        Assert.That(names.Count, Is.EqualTo(0));
    }

    private void InstanceProvider_Put()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new()
        {
            Server = "server-01"
        });

        // Act
        bool result = _instances.Put(instance);

        // Assert
        Assert.That(result, Is.True);
    }

    private void InstanceProvider_Get()
    {
        // Arrange
        // Act
        Instance? instance = _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private void InstanceProvider_GetAllNamesInCluster()
    {
        // Arrange
        // Act
        string[] ids = _instances.GetIndex().ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(1));
    }
}
