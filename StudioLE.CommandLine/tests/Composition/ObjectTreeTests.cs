using NUnit.Framework;
using StudioLE.CommandLine.Composition;
using StudioLE.CommandLine.Tests.Resources;
using StudioLE.Core.System;
using StudioLE.Verify.NUnit;

namespace StudioLE.CommandLine.Tests.Composition;

internal sealed class ObjectTreeTests
{
    private readonly Verify.Verify _verify = new(new NUnitVerifyContext());

    [Test]
    public async Task ObjectTree_FlattenProperties()
    {
        // Arrange
        ObjectTree objectTree = ObjectTree.Create<ExampleClass>();

        // Act
        ObjectTreeProperty[] properties = objectTree.FlattenProperties().ToArray();
        string[] output = properties.Select(x => $"{x.FullKey}: {x.Type}").ToArray();

        // Assert
        await _verify.String(output.Join());
    }
}
