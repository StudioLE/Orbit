namespace Orbit.Schema;

public class ShellCommand
{
    public string Command { get; set; } = string.Empty;

    public string? SuccessMessage { get; set; }

    public string? ErrorMessage { get; set; }
}
