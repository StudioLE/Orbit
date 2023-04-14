using Orbit.Cli.Utils.Composition;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using Orbit.Core.Schema;
using StudioLE.Core.System;

namespace Orbit.Cli.Tests;

internal sealed class ObjectTreeTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());

    [Test]
    public async Task ObjectTree_FlattenProperties()
    {
        // Arrange
        ObjectTree objectTree = ObjectTree.Create<Instance>();

        // Act
        ObjectTreeProperty[] properties = objectTree.FlattenProperties().ToArray();
        string[] output = properties.Select(x => $"{x.FullKey}: {x.Type}").ToArray();

        // Preview
        Console.WriteLine(output.Join());

        // Assert
        await _verify.String(output.Join());
    }
}
