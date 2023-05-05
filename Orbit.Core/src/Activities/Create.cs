using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Pipelines;
using StudioLE.Core.System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Activities;

public class Create : IActivity<Instance, Instance?>
{
    private readonly ILogger<Create> _logger;
    private readonly CreateOptions _options;
    private readonly EntityProvider _provider;
    private readonly InstanceFactory _factory;

    public Create(ILogger<Create> logger, CreateOptions options, EntityProvider provider, InstanceFactory factory)
    {
        _logger = logger;
        _options = options;
        _provider = provider;
        _factory = factory;
    }

    public Task<Instance?> Execute(Instance instance)
    {
        PipelineBuilder<Task<Instance?>> builder = new PipelineBuilder<Task<Instance?>>()
            .OnSuccess(() => OnSuccess(instance))
            .OnFailure(OnFailure)
            .Then(() => _provider.IsValid)
            .Then(() => _options.TryValidate(_logger))
            .Then(() => GetOrCreateCluster(instance))
            .Then(() =>
            {
                instance = _factory.Create(instance);
                return true;
            })
            .Then(() => instance.TryValidate(_logger))
            .Then(() => PutInstance(instance))
            .Then(() => CreateNetworkConfig(instance))
            .Then(() => CreateUserConfig(instance));
        Pipeline<Task<Instance?>> pipeline = builder.Build();
        return pipeline.Execute();
    }

    private Task<Instance?> OnSuccess(Instance instance)
    {
        _logger.LogInformation($"Created instance {instance.Name}");
        return Task.FromResult<Instance?>(instance);
    }

    private static Task<Instance?> OnFailure()
    {
        return Task.FromResult<Instance?>(null);
    }

    private bool GetOrCreateCluster(Instance instance)
    {
        Cluster? cluster = _provider.Cluster.Get(instance.Cluster);
        if (cluster is null)
        {
            cluster = new();
            cluster.Review(_provider);
            if (!_provider.Cluster.Put(cluster))
            {
                _logger.LogError("Failed to write the cluster file.");
                return false;
            }
            instance.Cluster = cluster.Name;
        }
        return cluster.TryValidate(_logger);
    }

    private bool PutInstance(Instance instance)
    {
        if (_provider.Instance.Put(instance))
            return true;
        _logger.LogError("Failed to write the instance file.");
        return false;
    }

    private string CreateWireGuardConfig(Instance instance)
    {
        List<string> lines = new()
        {
            $"""
            [Interface]
            PrivateKey = {instance.WireGuard.PrivateKey}
            """
        };
        foreach (string address in instance.WireGuard.Addresses)
            lines.Add($"Address = {address}");
        lines.Add($"""
            [Peer]
            PublicKey = {instance.WireGuard.ServerPublicKey}
            AllowedIPs = 10.8.0.0/24, fd0d:86fa:c3bc::/64
            Endpoint = 203.0.113.1:51820
            """);

        return lines.Join();
    }

    private bool CreateNetworkConfig(Instance instance)
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/network-config-template.yml");
        YamlNode adapter = yaml["network"]["ethernets"]["eth0"];

        adapter["addresses"][0].SetValue(instance.Network.Address);
        adapter["routes"][0]["via"].SetValue(instance.Network.Gateway);

        if (_provider.Instance.PutResource(instance.Cluster, instance.Name, "network-config.yml", yaml))
            return true;
        _logger.LogError("Failed to write the network config file.");
        return false;
    }

    private bool CreateUserConfig(Instance instance)
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/user-config-template.yml");

        yaml["hostname"].SetValue(instance.Name);
        yaml["users"][0]["name"].SetValue(_options.SudoUser);
        yaml["users"][1]["name"].SetValue(_options.User);
        yaml["users"][0].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        yaml["users"][1].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        string wireguardContent = CreateWireGuardConfig(instance);
        YamlMappingNode wg0 = new()
        {
            { "name", "wg0" },
            { "config_path", "/etc/wireguard/wg0.conf" },
            { "content", new YamlScalarNode(wireguardContent) { Style = ScalarStyle.Literal } }
        };
        yaml["wireguard"].Replace("interfaces", new YamlSequenceNode(wg0));
        // ReSharper disable StringLiteralTypo
        string bootCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/bootcmd.sh");
        string[] bootCmdLines = bootCmdContent.SplitIntoLines().ToArray();
        yaml["bootcmd"].AddRange(bootCmdLines);
        yaml["bootcmd"].SetSequenceStyle(SequenceStyle.Block);
        string runCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/runcmd.sh");
        string[] runCmdLines = runCmdContent.SplitIntoLines().ToArray();
        yaml["runcmd"].AddRange(runCmdLines);
        yaml["runcmd"].SetSequenceStyle(SequenceStyle.Block);
        string[] installers =
        {
            "10-install-docker.sh",
            "20-install-cri-dockerd.sh",
            "30-install-kubernetes.sh"
        };
        foreach (string installer in installers)
        {
            string content = EmbeddedResourceHelpers.GetText($"Resources/Scripts/{installer}");
            KeyValuePair<YamlNode, YamlNode>[] nodes = {
                new("path", $"/var/lib/cloud/scripts/per-instance/{installer}"),
                new("permissions", "0o500"),
                new("content", new YamlScalarNode(content) { Style = ScalarStyle.Literal })
            };
            yaml["write_files"].Add(new YamlMappingNode(nodes));
        }
        // ReSharper restore StringLiteralTypo

        if (_provider.Instance.PutResource(instance.Cluster, instance.Name, "user-config.yml", yaml))
            return true;
        _logger.LogError("Failed to write the user config file.");
        return false;
    }
}
