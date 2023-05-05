using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Pipelines;
using Renci.SshNet;
using StudioLE.Core.System;

namespace Orbit.Core.Activities;

public class Launch : IActivity<Launch.Inputs, Launch.Outputs>
{
    private readonly ILogger<Launch> _logger;
    private readonly EntityProvider _provider;
    private Instance _instance = null!;
    private ConnectionInfo _connection = null!;
    private string _tempFilePath = string.Empty;

    public class Inputs
    {
        [Required]
        [NameSchema]
        public string Cluster { get; set; } = string.Empty;

        [Required]
        [NameSchema]
        public string Instance { get; set; } = string.Empty;
    }

    public class Outputs
    {
        public int ExitCode { get; set; }
    }

    public Launch(ILogger<Launch> logger, EntityProvider provider)
    {
        _logger = logger;
        _provider = provider;
    }

    public Task<Outputs> Execute(Inputs inputs)
    {
        PipelineBuilder<Task<Outputs>> builder = new PipelineBuilder<Task<Outputs>>()
            .OnSuccess(OnSuccess)
            .OnFailure(OnFailure)
            .Then(() => GetInstance(inputs.Cluster, inputs.Instance))
            .Then(() => _instance.TryValidate(_logger))
            .Then(CreateConnection)
            .Then(UploadCloudInit)
            .Then(MultipassLaunch);
        Pipeline<Task<Outputs>> pipeline = builder.Build();
        return pipeline.Execute();
    }

    private bool GetInstance(string clusterName, string instanceName)
    {
        Instance? instance = _provider.Instance.Get(clusterName, instanceName);
        if (instance is null)
        {
            _logger.LogError("The instance does not exist.");
            return false;
        }
        _instance = instance;
        return true;
    }

    private bool CreateConnection()
    {
        Server? server = _provider.Server.Get(_instance.Server);
        if(server is null)
        {
            _logger.LogError("Failed to get server");
            return false;
        }
        _tempFilePath = $"/home/{server.Ssh.User}/orbit/tmp/{Guid.NewGuid()}.yaml";
        _connection = server.CreateConnection();
        return true;
    }

    private bool UploadCloudInit()
    {
        Stream? stream = _provider.Instance.GetResourceStream(_instance.Cluster, _instance.Name, "user-config.yml");
        if (stream is null)
        {
            _logger.LogError("Failed to get user-config");
            return false;
        }

        // using MemoryStream stream = new();
        // using StreamWriter writer = new(stream);
        // writer.Write(stream);
        // writer.Flush();

        using SftpClient sftp = new(_connection);
        sftp.Connect();
        sftp.UploadFile(stream, _tempFilePath);
        // string blah = sftp.ReadAllText(_tempFilePath);

        return true;
    }

    private bool MultipassLaunch()
    {
        using SshClient ssh = new(_connection);
        ssh.Connect();
        string[] command =
        {
            "multipass launch",
            $"--cpus {_instance.Hardware.Cpus}",
            $"--memory {_instance.Hardware.Memory}G",
            $"--disk {_instance.Hardware.Disk}G",
            $"--name {_instance.Name}",
            $"--cloud-init {_tempFilePath}"
        };
        string? output = ssh.ExecuteToLogger(_logger, command.Join(" "));
        return output is not null;
    }

    private static Task<Outputs> OnSuccess()
    {
        Outputs outputs = new()
        {
            ExitCode = 0
        };
        return Task.FromResult(outputs);
    }

    private static Task<Outputs> OnFailure()
    {
        Outputs outputs = new()
        {
            ExitCode = 1
        };
        return Task.FromResult(outputs);
    }
}
