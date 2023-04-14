using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema;
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
        ValidationContext context = new(instance);
        List<ValidationResult> results = new();
        bool isValid =  Validator.TryValidateObject(instance, context, results, validateAllProperties: true);
        string errors = results.Select(x => x.ErrorMessage ?? string.Empty).Join();
        Console.WriteLine(errors);

        // Assert
        Assert.Multiple(() =>
        {
            _verify.String(errors);
            Assert.That(isValid, Is.False);
            Assert.That(results, Is.Not.Empty);
        });
    }

    [Test]
    public void Instance_Validate_Review()
    {
        // Arrange
        Instance instance = new();
        instance.Review();

        // Act
        ValidationContext context = new(instance);
        List<ValidationResult> results = new();
        bool isValid =  Validator.TryValidateObject(instance, context, results);

        // Assert
        Assert.Multiple(() =>
        {
            _verify.AsYaml(instance);
            Assert.That(isValid, Is.True);
            Assert.That(results, Is.Empty);
        });
    }
}
