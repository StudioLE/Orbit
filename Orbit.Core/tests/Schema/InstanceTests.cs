using Orbit.Core.Schema;
using Orbit.Core.Utils;
using StudioLE.Core.System;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Core.Tests.Schema;

internal sealed class InstanceTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());

    [Test]
    public void Instance_Validate_Default()
    {
        // Arrange
        Instance instance = new();

        // Act
        bool isValid =  instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        Assert.Multiple(() =>
        {
            _verify.String(errors.Join());
            Assert.That(isValid, Is.False);
            Assert.That(errors, Is.Not.Empty);
        });
    }

    [Test]
    public void Instance_Validate_Review()
    {
        // Arrange
        Instance instance = new();
        instance.Review();

        // Act
        bool isValid =  instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        Assert.Multiple(() =>
        {
            _verify.AsYaml(instance);
            Assert.That(isValid, Is.True);
            Assert.That(errors, Is.Empty);
        });
    }
}
