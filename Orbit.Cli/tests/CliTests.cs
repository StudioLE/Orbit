using Orbit.Cli.Utils.Converters;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;

namespace Orbit.Cli.Tests;

internal sealed class CliTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());

    [Test]
    [Explicit]
    public async Task Cli_Create()
    {
        // Arrange
        string[] args =
        {
            "create",
            "--host", "1",
            "--type", "G1"
        };
        ConverterResolver resolver = DependencyInjectionHelper.DefaultConverterResolver();
        Cli entry = new(args, resolver);

        // Act
        string console = await CaptureConsoleOutput(async () => await entry.Run());
        Console.WriteLine(console);

        // Assert
        Assert.That(console.StartsWith("Created "));
    }

    private static async Task<string> CaptureConsoleOutput(Func<Task> func)
    {
        // Temporarily capture the console output
        string console;
        await using (StringWriter consoleWriter = new())
        {
            Console.SetOut(consoleWriter);
            await func.Invoke();
            console = consoleWriter.ToString();
        }

        // Reset the console output
        StreamWriter standardOutputWriter = new(Console.OpenStandardOutput())
        {
            AutoFlush = true
        };
        Console.SetOut(standardOutputWriter);
        return console;
    }
}
