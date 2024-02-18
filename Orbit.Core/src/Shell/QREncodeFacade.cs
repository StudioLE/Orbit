using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StudioLE.Extensions.System;

namespace Orbit.Shell;

/// <inheritdoc cref="IQREncodeFacade"/>
// ReSharper disable once InconsistentNaming
public class QREncodeFacade : IQREncodeFacade
{
    private readonly ILogger<QREncodeFacade> _logger;

    private const int Timeout = 1000;

    /// <summary>
    /// The DI constructor for <see cref="QREncodeFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public QREncodeFacade(ILogger<QREncodeFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? GenerateSvg(string source)
    {
        Process cmd = new();
        cmd.StartInfo.FileName = "qrencode";
        cmd.StartInfo.Arguments = "-t svg";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        try
        {
            cmd.Start();
        }
        catch (Exception e)
        {
            _logger.LogError("Failed to start qrencode. Are you sure it's installed?");
            _logger.LogDebug(e, e.Message);
            return null;
        }

        cmd.StandardInput.WriteLine(source);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();
        string output = cmd.StandardOutput.ReadToEnd();
        cmd.WaitForExit(Timeout);
        if (cmd.ExitCode == 0)
            return output;
        string[] messages =
        {
            $"The executed command failed: {cmd}",
            $"ExitCode: {cmd.ExitCode}",
            $"StandardOutput: {cmd.StandardOutput.ReadToEnd()}",
            $"StandardError: {cmd.StandardError.ReadToEnd()}"
        };
        _logger.LogError(messages.Join());
        return null;
    }
}
