using System.CommandLine.IO;
using Microsoft.Extensions.Logging;
using StudioLE.Core.System;

namespace StudioLE.CommandLine.Logging;

public class LogWriter : IStandardStreamWriter
{
    private readonly ILogger _logger;
    private readonly LogLevel _logLevel;
    private readonly List<string> _lineBuffer = new();

    public LogWriter(ILogger logger, LogLevel logLevel)
    {
        _logger = logger;
        _logLevel = logLevel;
    }

    /// <inheritdoc />
    public void Write(string? value)
    {
        if (value is null || value == "\r")
            return;
        bool isLine = false;
        if (value.EndsWith("\r\n"))
        {
            isLine = true;
            value = value.Substring(0, value.Length - 2);
        }
        else if (value.EndsWith("\n"))
        {
            isLine = true;
            value = value.Substring(0, value.Length - 1);
        }
        _lineBuffer.Add(value);
        if (isLine)
            Flush();
    }

    public void Flush()
    {
        if (!_lineBuffer.Any())
            return;
        string line = _lineBuffer.Join(string.Empty);
        _logger.Log(_logLevel, line);
        _lineBuffer.Clear();
    }
}
