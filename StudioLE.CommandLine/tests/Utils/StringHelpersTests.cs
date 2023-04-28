using NUnit.Framework;
using StudioLE.CommandLine.Utils;

namespace StudioLE.CommandLine.Tests.Utils;

internal sealed class StringHelpersTests
{
    [TestCase("HelloWorld", "Hello World")]
    [TestCase("IP", "IP")]
    [TestCase("IPAddress", "IP Address")]
    [TestCase("SSHKey", "SSH Key")]
    [TestCase("helloWorld", "hello World")]
    [TestCase("3D", "3D")]
    [TestCase("3Dimensional", "3Dimensional")]
    [TestCase("BarryODoyle", "Barry O Doyle")]
    [TestCase("helloWorldIPAddressSSHKey", "hello World IP Address SSH Key")]
    public void StringHelpers_PascalToTitleCase(string source, string expected)
    {
        // Arrange
        // Act
        string actual = StringHelpers.PascalToTitleCase(source);

        // Assert
        Assert.That(actual, Is.EqualTo(expected));
    }
}
