using Microsoft.Extensions.Logging;

namespace StudioLE.CommandLine.Utils.Logging.TestLogger;

public class TestLog
{
    public LogLevel LogLevel { get; set; }

    public EventId EventId { get; set; }

    public Type? State { get; set; }

    public Exception? Exception { get; set; }

    public string Message { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString()
    {
        return $"{LogLevel}: {Message}";
    }
}
