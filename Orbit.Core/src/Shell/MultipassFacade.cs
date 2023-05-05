using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Renci.SshNet;
using StudioLE.Core.System;

// ReSharper disable CommentTypo

namespace Orbit.Core.Shell;

public class MultipassFacade
{
    private readonly ILogger<MultipassFacade> _logger;
    private readonly EntityProvider _provider;

    public MultipassFacade(ILogger<MultipassFacade> logger, EntityProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public JObject? Info(Server server, string name)
    {
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        string? output = ssh.Execute(_logger, $"multipass info \"{name}\" --format json");
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


    public bool Launch(Instance instance)
    {
        Server server = _provider.Server.Get(instance.Server) ?? throw new("Failed to get server");
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        string[] command =
        {
            "multipass launch",
            $"--cpus {instance.Hardware.Cpus}",
            $"--memory {instance.Hardware.Memory}G",
            $"--disk {instance.Hardware.Disk}G",
            $"--name {instance.Name}"
        };
        string? output = ssh.ExecuteToLogger(_logger, command.Join(" "));
        return output is null;
    }

    public JObject? List(Server server)
    {
        ConnectionInfo connection = server.CreateConnection();
        using SshClient ssh = new(connection);
        ssh.Connect();
        string? output = ssh.Execute(_logger, "multipass list --format json");
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
}
