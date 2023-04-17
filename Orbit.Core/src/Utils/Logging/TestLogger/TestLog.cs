using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

public class TestLog
{
    public LogLevel LogLevel { get; set; }

    public EventId EventId { get; set; }

    public Type? State { get; set; }

    public Exception? Exception { get; set; }

    public string Message { get; set; } = string.Empty;
}
