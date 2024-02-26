namespace Orbit.Schema;

public record struct ShellCommand()
{
    public string Command { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; } = null;

    public string? ErrorMessage { get; set; } = null;
}
