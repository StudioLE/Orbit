using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Providers;
using Orbit.Core.Schema;

namespace Orbit.Core.Tests;

internal sealed class InstanceProviderTests
{
    private const string ClusterName = "cluster-01";
    private const string InstanceName = $"{ClusterName}-01";
    private readonly InstanceFactory _instanceFactory;
    private readonly EntityProvider _provider;

    public InstanceProviderTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _provider = host.Services.GetRequiredService<EntityProvider>();
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
        Instance? instance = _provider.Instance.Get(ClusterName, InstanceName);

        // Assert
        Assert.That(instance, Is.Null);
    }

    private void InstanceProvider_GetAllNamesInCluster_Empty()
    {
        // Arrange
        // Act
        string[] names = _provider.Instance.GetAllNamesInCluster(ClusterName).ToArray();

        // Assert
        Assert.That(names.Count, Is.EqualTo(0));
    }

    private void InstanceProvider_Put()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new()
        {
            Server = "server-01",
            Cluster = "cluster-01"
        });

        // Act
        bool result = _provider.Instance.Put(instance);

        // Assert
        Assert.That(result, Is.True);
    }

    private void InstanceProvider_Get()
    {
        // Arrange
        // Act
        Instance? instance = _provider.Instance.Get(ClusterName, InstanceName);

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private void InstanceProvider_GetAllNamesInCluster()
    {
        // Arrange
        // Act
        string[] ids = _provider.Instance.GetAllNamesInCluster(ClusterName).ToArray();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(1));
    }
}
