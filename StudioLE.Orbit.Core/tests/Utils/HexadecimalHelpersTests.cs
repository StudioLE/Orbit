using NUnit.Framework;
using StudioLE.Orbit.Utils;

namespace StudioLE.Orbit.Core.Tests.Utils;

[TestFixture]
internal class HexadecimalHelpersTests
{
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(123)]
    [TestCase(1234)]
    [TestCase(9999)]
    public void HexadecimalHelpers_ToUShort(int intValue)
    {
        // Arrange
        // Act
        string intString = intValue.ToString();
        ushort? ushortValue = HexadecimalHelpers.ToUShort(intString);

        // Assert
        string hexString = ushortValue?.ToString("X") ?? string.Empty;
        Assert.That(hexString, Is.EqualTo(intString));
    }

    [TestCase(-1)]
    [TestCase(10000)]
    public void HexadecimalHelpers_ToUShort_Invalid(int intValue)
    {
        // Arrange
        // Act
        string intString = intValue.ToString();
        ushort? ushortValue = HexadecimalHelpers.ToUShort(intString);

        // Assert
        Assert.That(ushortValue, Is.Null);
    }
}
