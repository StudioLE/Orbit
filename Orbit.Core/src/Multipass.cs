using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orbit.Core.SSH;
using Orbit.Core.Utils;
using Renci.SshNet;

namespace Orbit.Core;

public class Multipass : IDisposable
{
    private readonly ILogger<Multipass> _logger;
    private readonly SshClient _ssh;

    public Multipass(ILogger<Multipass> logger, ConnectionOptions connection)
    {
        _logger = logger;
        _ssh = new(connection.CreateConnection());
        _ssh.Connect();
    }

    private string? Execute(string commandText, Action<string> onLineEmitted)
    {
        SshCommand command = _ssh.CreateCommand(commandText);
        IAsyncResult result = command.BeginExecute();
        using StreamReader reader = new(command.OutputStream);
        while (!result.IsCompleted || !reader.EndOfStream)
        {
            // string output = reader.ReadToEnd();
            string? line = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(line))
                continue;
            line = line
                .Replace("\u0008/", "")
                .Replace("\u0008-", "")
                .Replace("\u0008\\", "")
                .Replace("\u0008|", "")
                .Replace("\u0008", "");
            if (string.IsNullOrWhiteSpace(line))
                continue;
            onLineEmitted.Invoke(line);
        }
        command.EndExecute(result);

        if(command.ExitStatus == 0)
            return command.Result;
        _logger.LogError("Failed to get multipass info.");
        if(!command.Error.IsNullOrEmpty())
            _logger.LogError(command.Error);
        if(!command.Error.IsNullOrEmpty())
            _logger.LogError(command.Error);
        return null;
    }

    private string? Execute(string commandText)
    {
        SshCommand command = _ssh.RunCommand(commandText);
        if(command.ExitStatus == 0)
            return command.Result;
        _logger.LogError("Failed to get multipass info.");
        if(!command.Error.IsNullOrEmpty())
            _logger.LogError(command.Error);
        if(!command.Error.IsNullOrEmpty())
            _logger.LogError(command.Error);
        return null;
    }


    public JObject? Info(string name)
    {
        string? output = Execute($"multipass info \"{name}\" --format json");
        return output is null
            ? null
            : JObject.Parse(output);
        //     {
        //         "errors": [
        //         ],
        //         "info": {
        //             "hello-world": {
        //                 "cpu_count": "1",
        //                 "disks": {
        //                     "sda1": {
        //                         "total": "5011288064",
        //                         "used": "1547599872"
        //                     }
        //                 },
        //                 "image_hash": "345fbbb6ec827ca02ec1a1ced90f7d40d3fd345811ba97c5772ac40e951458e1",
        //                 "image_release": "22.04 LTS",
        //                 "ipv4": [
        //                     "10.136.165.188"
        //                 ],
        //                 "load": [
        //                     0.02,
        //                     0.17,
        //                     0.09
        //                 ],
        //                 "memory": {
        //                     "total": 1012797440,
        //                     "used": 193384448
        //                 },
        //                 "mounts": {
        //                 },
        //                 "release": "Ubuntu 22.04.2 LTS",
        //                 "state": "Running"
        //             }
        //         }
        //     }
    }


    public bool Launch(string name, Action<string> onLineEmitted)
    {
        string? output = Execute("multipass launch", onLineEmitted);
        return output is null;
    }

    public JObject? List()
    {
        string? output = Execute("multipass list --format json");
        return output is null
            ? null
            : JObject.Parse(output);
        //     {
        //         "list": [
        //             {
        //                 "ipv4": [
        //                     "10.136.165.188"
        //                 ],
        //                 "name": "hello-world",
        //                 "release": "Ubuntu 22.04 LTS",
        //                 "state": "Running"
        //             }
        //         ]
        //     }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        // _ssh.Dispose();
    }
}
