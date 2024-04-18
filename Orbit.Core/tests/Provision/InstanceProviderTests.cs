using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Instances;
using Orbit.Schema;

namespace Orbit.Core.Tests.Provision;

internal sealed class InstanceProviderTests
{
    private readonly InstanceId _instanceId = new("instance-10");
    private InstanceFactory _instanceFactory;
    private InstanceProvider _instances;

    [SetUp]
    public async Task SetUp()
    {
        IHost host = await TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _instances = host.Services.GetRequiredService<InstanceProvider>();
    }

    [Test]
    [Category("Misc")]
    public async Task InstanceProvider_In_Sequence()
    {
        await InstanceProvider_Get_Before();
        await InstanceProvider_GetAll_Before();
        await InstanceProvider_Put();
        await InstanceProvider_Get_After();
        await InstanceProvider_GetAll_After();
    }

    private async Task InstanceProvider_Get_Before()
    {
        // Arrange
        // Act
        Instance? instance = await _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Null);
    }

    private async Task InstanceProvider_GetAll_Before()
    {
        // Arrange
        // Act
        Instance[] ids = await (await _instances.GetAll()).ToArrayAsync();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(1));
    }

    private async Task InstanceProvider_Put()
    {
        // Arrange
        Instance instance = await _instanceFactory.Create(new());

        // Act
        bool result = await _instances.Put(instance);

        // Assert
        Assert.That(result, Is.True);
    }

    private async Task InstanceProvider_Get_After()
    {
        // Arrange
        // Act
        Instance? instance = await _instances.Get(_instanceId);

        // Assert
        Assert.That(instance, Is.Not.Null);
    }

    private async Task InstanceProvider_GetAll_After()
    {
        // Arrange
        // Act
        Instance[] ids = await (await _instances.GetAll()).ToArrayAsync();

        // Assert
        Assert.That(ids.Count, Is.EqualTo(2));
    }
}
