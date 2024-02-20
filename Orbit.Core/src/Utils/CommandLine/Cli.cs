using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Orbit.Utils.CommandLine;

/// <summary>
/// Execute a shell command - with optional standard input - and capture the output.
/// </summary>
/// <remarks>
/// <see cref="Cli"/> is a wrapper around <see cref="Process"/> that handles the
/// frustrations quirks of the dealing with <see cref="Process"/> directly.
/// </remarks>
public class Cli
{
    public ILogger? Logger { get; set; }

    /// <summary>
    /// The number of milliseconds to wait for the process to exit.
    /// </summary>
    public int TimeOut { get; set; } = 5000;

    /// <summary>
    /// The action to take when the process writes to standard output.
    /// </summary>
    public Action<string> OnOutput { get; set; }

    /// <summary>
    /// The action to take when the process writes to standard error.
    /// </summary>
    public Action<string> OnError { get; set; }

    /// <summary>
    /// An optional hook to configure the <see cref="ProcessStartInfo"/>.
    /// </summary>
    public Action<ProcessStartInfo>? ConfigureStartInfo { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="Cli"/>.
    /// </summary>
    public Cli()
    {
        OnOutput = OutputToLogger;
        OnError = ErrorToLogger;
    }

    /// <summary>
    /// DI constructor for <see cref="Cli"/>.
    /// </summary>
    public Cli(ILogger<Cli> logger) : this()
    {
        Logger = logger;
    }

    private enum ExitCode
    {
        Success,
        FailedToStart = 200,
        TimedOutWaitingForExit = 201,
        TimedOutWaitingForStandardOutput = 202,
        TimedOutWaitingForStandardError = 203
    }

    /// <summary>
    /// Execute a shell command - with optional standard input - and capture the output.
    /// </summary>
    /// <param name="commandPath">The command to execute.</param>
    /// <param name="arguments">The arguments to pass to the command.</param>
    /// <param name="standardInput">Optional strings to pass to standard input.</param>
    /// <returns>
    /// The process exit code if the command is executed,
    /// or an integer value of <see cref="ExitCode"/> if something goes wrong.
    /// </returns>
    public int Execute(string commandPath, string arguments, params string[] standardInput)
    {
        bool enableStandardInput = standardInput.Length > 0;
        using AutoResetEvent outputWaitHandle = new(false);
        using AutoResetEvent errorWaitHandle = new(false);
        using Process process = CreateProcess(commandPath, arguments, enableStandardInput);
        if (!StartProcess(process))
            return (int)ExitCode.FailedToStart;
        Subscribe(process, outputWaitHandle, errorWaitHandle);
        if (enableStandardInput)
            WriteStandardInput(process, standardInput);
        return WaitForCompletion(process, outputWaitHandle, errorWaitHandle);
    }

    /// <summary>
    /// <inheritdoc cref="Execute(string,string,string[])"/>
    /// </summary>
    /// <param name="commandWithArguments">The command to execute.</param>
    /// <returns>
    /// <inheritdoc cref="Execute(string,string,string[])"/>
    /// </returns>
    public int Execute(string commandWithArguments)
    {
        return Execute(commandWithArguments, string.Empty);
    }

    private Process CreateProcess(string commandPath, string arguments, bool enableStandardInput)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = commandPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = enableStandardInput,
            CreateNoWindow = true,
            UseShellExecute = false
        };
        ConfigureStartInfo?.Invoke(startInfo);
        Process process = new()
        {
            EnableRaisingEvents = true,
            StartInfo = startInfo
        };
        return process;
    }

    private bool StartProcess(Process process)
    {
        try
        {
            process.Start();
        }
        catch (Exception e)
        {
            Logger?.LogError($"Failed to start {process.StartInfo.FileName}. Are you sure it's installed?");
            Logger?.LogDebug(e, e.Message);
            return false;
        }
        return true;
    }

    private void Subscribe(Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle)
    {
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data is null)
                outputWaitHandle.Set();
            else
                OnOutput.Invoke(e.Data);
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is null)
                errorWaitHandle.Set();
            else
                OnError.Invoke(e.Data);
        };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
    }

    private void OutputToLogger(string output)
    {
        Logger?.LogInformation(output);
    }

    private void ErrorToLogger(string error)
    {
        Logger?.LogError(error);
    }

    private static void WriteStandardInput(Process process, IEnumerable<string> standardInput)
    {
        foreach (string str in standardInput)
            process.StandardInput.WriteLine(str);
        process.StandardInput.Flush();
        process.StandardInput.Close();
    }

    private int WaitForCompletion(Process process, AutoResetEvent outputWaitHandle, AutoResetEvent errorWaitHandle)
    {
        if (!process.WaitForExit(TimeOut))
        {
            Logger?.LogWarning($"The process {process.StartInfo.FileName} timed out after {TimeOut} milliseconds waiting for exit.");
            return (int)ExitCode.TimedOutWaitingForExit;
        }
        if (!outputWaitHandle.WaitOne(TimeOut))
        {
            Logger?.LogWarning($"The process {process.StartInfo.FileName} timed out after {TimeOut} milliseconds waiting for output.");
            return (int)ExitCode.TimedOutWaitingForStandardOutput;
        }
        if (!errorWaitHandle.WaitOne(TimeOut))
        {
            Logger?.LogWarning($"The process {process.StartInfo.FileName} timed out after {TimeOut} milliseconds waiting for errors.");
            return (int)ExitCode.TimedOutWaitingForStandardError;
        }
        return process.ExitCode;
    }
}
