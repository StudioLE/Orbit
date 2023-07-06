using Microsoft.Extensions.Logging;

namespace Orbit.Core.Utils.Logging.TestLogger;

/// <summary>
/// A log entry for <see cref="TestLogger"/>.
/// </summary>
public class TestLog
{
    /// <summary>
    /// The log category name.
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// The log level.
    /// </summary>
    public LogLevel LogLevel { get; set; }

    /// <summary>
    /// The event id.
    /// </summary>
    public EventId EventId { get; set; }

    /// <summary>
    /// The state.
    /// </summary>
    public Type? State { get; set; }

    /// <summary>
    /// The exception.
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// The log message.
    /// </summary>
    public string Message { get; set; } = string.Empty;
}
