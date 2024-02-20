using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Creation.Instances;
using Orbit.Schema;
using Orbit.Utils.DataAnnotations;
using StudioLE.Extensions.System;
using StudioLE.Serialization;
using StudioLE.Verify;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests.Schema;

internal sealed class InstanceTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly InstanceFactory _instanceFactory;
    private readonly ISerializer _serializer;

    public InstanceTests()
    {
        IHost host = TestHelpers.CreateTestHost();
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        _serializer = host.Services.GetRequiredService<ISerializer>();
    }

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

    [Test]
    [Category("Misc")]
    public async Task Instance_Validate_Review()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(TestHelpers.GetExampleInstance());

        // Act
        bool isValid = instance.TryValidate(out IReadOnlyCollection<string> errors);
        Console.WriteLine(errors.Join());

        // Assert
        await _context.VerifyAsSerialized(instance, _serializer);
        Assert.Multiple(() =>
        {
            Assert.That(isValid, Is.True);
            Assert.That(errors, Is.Empty);
        });
    }
}
