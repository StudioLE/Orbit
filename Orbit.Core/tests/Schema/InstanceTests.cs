using NUnit.Framework;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.System;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Schema;

internal sealed class InstanceTests
{
    private readonly IContext _context = new NUnitContext();

    [Test]
    [Category("Misc")]
    public async Task Instance_Validate_Default()
    {
        // Arrange
        Instance instance = new();

        // Act
        bool isValid = instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        await _context.Verify(errors.Join());
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.False);
            Assert.That(errors, Is.Not.Empty);
        });
    }
}
