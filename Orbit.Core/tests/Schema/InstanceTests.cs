using NUnit.Framework;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;
using StudioLE.Core.System;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Schema;

internal sealed class InstanceTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private readonly EntityProvider _provider = EntityProvider.CreateTemp();

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
        Instance instance = new()
        {
            Server = "server-01",
            Cluster = "cluster-01",
            WireGuard =
            {
                PrivateKey = "8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=",
                PublicKey = "Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE="
            }
        };
        instance.Review(_provider);

        // Act
        bool isValid = instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        await _verify.AsYaml(instance);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.True);
            Assert.That(errors, Is.Empty);
        });
    }
}
