using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Orbit.Shell;

public abstract class ShellCommand
{
    protected readonly ILogger _logger;
    private TaskCompletionSource<int> _taskCompletionSource = null!;

    /// <summary>
    /// DI constructor for <see cref="ShellCommand"/>.
    /// </summary>
    protected ShellCommand(ILogger logger)
    {
        _logger = logger;
    }

    protected Task<int> Execute(string commandPath, string arguments, params string[] standardInput)
    {
        _taskCompletionSource = new();
        Process process = CreateProcess(commandPath, arguments);
        if(!StartProcess(process))
            return Task.FromResult(255);
        SubscribeEvents(process);
        if (standardInput.Length > 0)
            WriteStandardInput(process, standardInput);
        return _taskCompletionSource.Task;
    }

    private static Process CreateProcess(string commandPath, string arguments)
    {
        Process process = new()
        {
            EnableRaisingEvents = true,
            StartInfo = new()
            {
                FileName = commandPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                CreateNoWindow = true,
                UseShellExecute = false
            }
        };
        return process;
    }

    private void SubscribeEvents(Process process)
    {
        process.OutputDataReceived += OnOutputDataReceivedInternal;
        process.ErrorDataReceived += OnErrorDataReceivedInternal;
        process.Exited += OnExitedInternal;
    }

    private void UnsubscribeEvents(Process process)
    {
        process.OutputDataReceived -= OnOutputDataReceivedInternal;
        process.ErrorDataReceived -= OnErrorDataReceivedInternal;
        process.Exited -= OnExitedInternal;
    }

    private void OnOutputDataReceivedInternal(object _, DataReceivedEventArgs args)
    {
        OnOutput(args.Data);
    }

    private void OnErrorDataReceivedInternal(object _, DataReceivedEventArgs args)
    {
        OnError(args.Data);
    }

    private void OnExitedInternal(object sender, EventArgs eventArgs)
    {
        if (sender is not Process process)
            return;
        OnExited(process);
        UnsubscribeEvents(process);
        int exitCode = process.ExitCode;
        process.Dispose();
        _taskCompletionSource.SetResult(exitCode);
    }

    protected virtual void OnOutput(string? output)
    {
        if (output is not null)
            _logger.LogInformation(output);
    }

    protected virtual void OnError(string? error)
    {
        if (error is not null)
            _logger.LogError(error);
    }

    protected virtual void OnExited(Process process)
    {
    }

    private bool StartProcess(Process process)
    {
        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            _logger.LogError($"Failed to start {process.StartInfo.FileName}. Are you sure it's installed?");
            _logger.LogDebug(e, e.Message);
            return false;
        }
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return true;
    }

    private static void WriteStandardInput(Process process, IEnumerable<string> standardInput)
    {
        foreach (string str in standardInput)
            process.StandardInput.WriteLine(str);
        process.StandardInput.Flush();
        process.StandardInput.Close();
    }
}
