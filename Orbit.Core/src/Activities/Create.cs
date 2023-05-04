using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;
using StudioLE.Core.System;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Activities;

public class Create : IActivity<Instance, Instance?>
{
    private readonly ILogger<Create> _logger;
    private readonly CreateOptions _options;
    private readonly EntityProvider _provider;
    private Instance _instance = new();

    public Create(ILogger<Create> logger, CreateOptions options, EntityProvider provider)
    {
        _logger = logger;
        _options = options;
        _provider = provider;
    }

    public Task<Instance?> Execute(Instance instance)
    {
        _instance = instance;

        if (!_provider.IsValid)
            return Failure();

        if (!_options.TryValidate(_logger))
            return Failure();

        if (!GetOrCreateCluster())
            return Failure();

        instance.Review(_provider);

        if (!_instance.TryValidate(_logger))
            return Failure();

        if (!CreateInstance())
            return Failure();

        if (!CreateNetworkConfig())
            return Failure();

        if (!CreateUserConfig())
            return Failure();

        _logger.LogInformation($"Created instance {_instance.Name}");

        return Success();
    }

    private Task<Instance?> Success()
    {
        return Task.FromResult<Instance?>(_instance);
    }

    private static Task<Instance?> Failure()
    {
        return Task.FromResult<Instance?>(null);
    }

    private bool GetOrCreateCluster()
    {
        Cluster? cluster = _provider.Cluster.Get(_instance.Cluster);
        if (cluster is null)
        {
            cluster = new();
            cluster.Review(_provider);
            if (!_provider.Cluster.Put(cluster))
            {
                _logger.LogError("Failed to write the cluster file.");
                return false;
            }
            _instance.Cluster = cluster.Name;
        }
        return cluster.TryValidate(_logger);
    }

    private bool CreateInstance()
    {
        if (_provider.Instance.Put(_instance))
            return true;
        _logger.LogError("Failed to write the instance file.");
        return false;
    }

    private string CreateWireGuardConfig()
    {
        List<string> lines = new()
        {
            $"""
            [Interface]
            PrivateKey = {_instance.WireGuard.PrivateKey}
            """
        };
        foreach (string address in _instance.WireGuard.Addresses)
            lines.Add($"Address = {address}");
        lines.Add($"""
            [Peer]
            PublicKey = {_instance.WireGuard.HostPublicKey}
            AllowedIPs = 10.8.0.0/24, fd0d:86fa:c3bc::/64
            Endpoint = 203.0.113.1:51820
            """);

        return lines.Join();
    }

    private bool CreateNetworkConfig()
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/network-config-template.yml");
        YamlNode adapter = yaml["network"]["ethernets"]["eth0"];

        adapter["addresses"][0].SetValue(_instance.Network.Address);
        adapter["routes"][0]["via"].SetValue(_instance.Network.Gateway);

        if (_provider.Instance.PutResource(_instance.Cluster, _instance.Name, "network-config.yml", yaml))
            return true;
        _logger.LogError("Failed to write the network config file.");
        return false;
    }

    private bool CreateUserConfig()
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/user-config-template.yml");

        yaml["hostname"].SetValue(_instance.Name);
        yaml["users"][0]["name"].SetValue(_options.SudoUser);
        yaml["users"][1]["name"].SetValue(_options.User);
        yaml["users"][0].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        yaml["users"][1].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        string wireguardContent = CreateWireGuardConfig();
        YamlMappingNode wg0 = new()
        {
            { "name", "wg0" },
            { "config_path", "/etc/wireguard/wg0.conf" },
            { "content", new YamlScalarNode(wireguardContent) { Style = ScalarStyle.Literal } }
        };
        yaml["wireguard"].Replace("interfaces", new YamlSequenceNode(wg0));
        // ReSharper disable StringLiteralTypo
        string bootCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/bootcmd.sh");
        yaml["bootcmd"].SetValue(bootCmdContent, ScalarStyle.Literal);
        string runCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/runcmd.sh");
        yaml["runcmd"].SetValue(runCmdContent, ScalarStyle.Literal);
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
                new("permissions", "0500"),
                new("content", new YamlScalarNode(content) { Style = ScalarStyle.Literal })
            };
            yaml["write_files"].Add(new YamlMappingNode(nodes));
        }
        // ReSharper restore StringLiteralTypo

        if (_provider.Instance.PutResource(_instance.Cluster, _instance.Name, "user-config.yml", yaml))
            return true;
        _logger.LogError("Failed to write the user config file.");
        return false;
    }
}
