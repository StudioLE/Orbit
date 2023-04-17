namespace Orbit.Cli.Tests;

internal sealed class CliTests
{
    [Test]
    public async Task Cli_Create()
    {
        // Arrange
        string[] args =
        {
            "create",
            "--host", "1",
            "--type", "G1"
        };
        Cli entry = new();

        // Act
        await entry.Run(args);
        // Console.WriteLine(console);

        // Assert
        // Assert.That(console.StartsWith("Created "));
    }

    [Test]
    [Explicit("Requires SSH")]
    public async Task Cli_Launch()
    {
        // Arrange
        string[] args =
        {
            "launch"
        };
        Cli entry = new();

        // Act
        await entry.Run(args);

        // Assert
    }
}
