using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Schema;
using Orbit.Core.Tests.Resources;
using Orbit.Core.Utils.DataAnnotations;
using StudioLE.Core.Serialization;
using StudioLE.Core.System;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Schema;

internal sealed class InstanceTests
{
    private readonly IVerify _verify = new NUnitVerify();
    private readonly InstanceFactory _instanceFactory;
    private readonly ISerializer _serializer;

    public InstanceTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
    }

    [Test]
    public async Task Instance_Validate_Default()
    {
        // Arrange
        Instance instance = new();

        // Act
        bool isValid = instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        await _verify.String(errors.Join());
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(errors, Is.Not.Empty);
        });
    }

    [Test]
    public async Task Instance_Validate_Review()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new()
        {
            Server = "server-01",
            WireGuard =
            {
                PrivateKey = MockWireGuardFacade.PrivateKey,
                PublicKey = MockWireGuardFacade.PublicKey
            }
        });

        // Act
        bool isValid = instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        await _verify.AsSerialized(instance, _serializer);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.True);
            Assert.That(errors, Is.Empty);
        });
    }
}
