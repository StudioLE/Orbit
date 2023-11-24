using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StudioLE.Extensions.System;

namespace Orbit.Core.Shell;

/// <inheritdoc cref="IWireGuardFacade"/>
public class WireGuardFacade : IWireGuardFacade
{
    private readonly ILogger<WireGuardFacade> _logger;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public WireGuardFacade(ILogger<WireGuardFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string? GeneratePrivateKey()
    {
        Process cmd = new();
        cmd.StartInfo.FileName = "wg";
        cmd.StartInfo.Arguments = "genkey";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        // cmd.StandardInput.WriteLine(commandText);
        // cmd.StandardInput.Flush();
        // cmd.StandardInput.Close();
        cmd.WaitForExit();
        if (cmd.ExitCode == 0)
        {
            IReadOnlyCollection<string> lines = ReadAllLines(cmd.StandardOutput);
            if (lines.Count != 1)
                throw new("Expected only one line");
            return lines.First();
        }
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

    /// <inheritdoc/>
    public string? GeneratePublicKey(string privateKey)
    {
        Process cmd = new();
        // TODO: cmd is specific to windows...
        cmd.StartInfo.FileName = "wg";
        cmd.StartInfo.Arguments = "pubkey";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        cmd.StandardInput.WriteLine(privateKey);
        cmd.StandardInput.Flush();
        cmd.StandardInput.Close();

        cmd.WaitForExit();
        if (cmd.ExitCode == 0)
        {
            IReadOnlyCollection<string> lines = ReadAllLines(cmd.StandardOutput);
            if (lines.Count != 1)
                throw new("Expected only one line");
            return lines.First();
        }
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

    /// <inheritdoc/>
    public string? GeneratePreSharedKey()
    {
        Process cmd = new();
        cmd.StartInfo.FileName = "wg";
        cmd.StartInfo.Arguments = "genpsk";
        cmd.StartInfo.RedirectStandardInput = true;
        cmd.StartInfo.RedirectStandardOutput = true;
        cmd.StartInfo.CreateNoWindow = true;
        cmd.StartInfo.UseShellExecute = false;
        cmd.Start();

        // cmd.StandardInput.WriteLine(commandText);
        // cmd.StandardInput.Flush();
        // cmd.StandardInput.Close();
        cmd.WaitForExit();
        if (cmd.ExitCode == 0)
        {
            IReadOnlyCollection<string> lines = ReadAllLines(cmd.StandardOutput);
            if (lines.Count != 1)
                throw new("Expected only one line");
            return lines.First();
        }
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

    private static IReadOnlyCollection<string> ReadAllLines(TextReader reader)
    {
        List<string> lines = new();
        string? line;
        while ((line = reader.ReadLine()) is not null)
            lines.Add(line);
        return lines;
    }
}
