using Microsoft.Extensions.Options;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Serialization;
using Orbit.Core.Utils.Serialization.Yaml;
using StudioLE.Core.Patterns;
using StudioLE.Core.Serialization;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Generation;

public class CloudInitFactory : IFactory<Instance, string>
{
    private readonly CloudInitOptions _options;
    private readonly ISerializer _serializer;
    private readonly WireGuardConfigFactory _wireGuardConfigFactory;
    private readonly NetplanFactory _netplanFactory;
    private readonly InstallFactory _installFactory;

    /// <summary>
    /// DI constructor for <see cref="CloudInitFactory"/>.
    /// </summary>
    public CloudInitFactory(
        IOptions<CloudInitOptions> options,
        ISerializer serializer,
        WireGuardConfigFactory wireGuardConfigFactory,
        NetplanFactory netplanFactory,
        InstallFactory installFactory)
    {
        _options = options.Value;
        _serializer = serializer;
        _wireGuardConfigFactory = wireGuardConfigFactory;
        _netplanFactory = netplanFactory;
        _installFactory = installFactory;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        YamlNode resource = EmbeddedResourceHelpers.GetYaml("Resources/Templates/user-config-template.yml");
        if (resource is not YamlMappingNode yaml)
            throw new($"Expected a {nameof(YamlMappingNode)}");

        // Hostname
        yaml.SetValue("hostname", instance.Name, false);

        // Groups
        yaml.SetValue("groups", new[] { "docker" }, false);

        // Users
        YamlSequenceNode users = new()
        {
            new YamlMappingNode
            {
                { "name", _options.SudoUser },
                { "groups", "sudo, docker" },
                { "shell", "/bin/bash" },
                { "sudo", "ALL=(ALL) NOPASSWD:ALL" },
                { "lock_passwd", "true" },
                { "ssh_authorized_keys", _options.SshAuthorizedKeys }
            },
            new YamlMappingNode
            {
                { "name", _options.User },
                { "shell", "/bin/bash" },
                { "lock_passwd", "true" },
                { "ssh_authorized_keys", _options.SshAuthorizedKeys }
            }
        };
        yaml.SetValue("users", users, false);

        // WireGuard
        YamlMappingNode[] wgNodes = instance
            .WireGuard
            .Select(wg =>
            {
                string config = _wireGuardConfigFactory.Create(wg);
                return new YamlMappingNode
                {
                    { "name", wg.Name },
                    { "config_path", $"/etc/wireguard/{wg.Name}.conf" },
                    { "content", new YamlScalarNode(config) { Style = ScalarStyle.Literal } }
                };
            })
            .ToArray();
        YamlMappingNode wgInterfaces = new()
        {
            { "interfaces", new YamlSequenceNode(wgNodes) }
        };
        yaml.SetValue("wireguard", wgInterfaces, false);

        // Write files
        YamlSequenceNode? writeFiles = yaml.GetValue<YamlSequenceNode>("write_files");
        if (writeFiles is null)
        {
            writeFiles = new();
            yaml.SetValue("write_files", writeFiles);
        }

        // Write sshd_config
        string sshdConfigContent = EmbeddedResourceHelpers.GetText("Resources/Templates/sshd_config");
        YamlMappingNode sshdConfigNode = new()
        {
            { "path", "/etc/ssh/sshd_config" },
            { "append", "true" },
            { "content", new YamlScalarNode(sshdConfigContent) { Style = ScalarStyle.Literal } }
        };
        writeFiles.Add(sshdConfigNode);

        // Write netplan config
        string netplanContent = _netplanFactory.Create(instance);
        YamlMappingNode netplanNode = new()
        {
            { "path", "/etc/netplan/10-custom.yaml" },
            { "content", new YamlScalarNode(netplanContent) { Style = ScalarStyle.Literal } }
        };
        writeFiles.Add(netplanNode);

        // Write 50-orbit-configure
        string configureContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/50-orbit-configure");
        YamlMappingNode configureNode = new()
        {
            { "path", "/var/lib/cloud/scripts/per-instance/50-orbit-configure" },
            { "permissions", "0o500" },
            { "content", new YamlScalarNode(configureContent) { Style = ScalarStyle.Literal } }
        };
        writeFiles.Add(configureNode);

        // Write 60-orbit-install
        string installContent = _installFactory.Create(instance);
        YamlMappingNode installNode = new()
        {
            { "path", "/var/lib/cloud/scripts/per-instance/50-orbit-install" },
            { "permissions", "0o500" },
            { "content", new YamlScalarNode(installContent) { Style = ScalarStyle.Literal } }
        };
        writeFiles.Add(installNode);

        // Serialize
        string output = "#cloud-config" + Environment.NewLine + Environment.NewLine;
        output += _serializer.Serialize(yaml);
        return output;
    }
}
