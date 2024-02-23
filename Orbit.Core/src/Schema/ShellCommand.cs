namespace Orbit.Schema;

public readonly record struct ShellCommand()
{
    public string Command { get; init; } = string.Empty;

    public string? SuccessMessage { get; init; } = null;

    public string? ErrorMessage { get; init; } = null;
}
