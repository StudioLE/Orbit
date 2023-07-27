using Microsoft.Extensions.Options;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using Orbit.Core.Utils.Serialization;
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

    /// <summary>
    /// DI constructor for <see cref="CloudInitFactory"/>.
    /// </summary>
    public CloudInitFactory(
        IOptions<CloudInitOptions> options,
        ISerializer serializer,
        WireGuardConfigFactory wireGuardConfigFactory)
    {
        _options = options.Value;
        _serializer = serializer;
        _wireGuardConfigFactory = wireGuardConfigFactory;
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

        // Write per-instance files
        string[] installerFiles =
        {
            "00-log.sh",
            "10-install-docker.sh",
            "90-install-network-test.sh",
            "99-log.sh"
        };
        YamlMappingNode[] installerNodes = installerFiles
            .Select(fileName =>
            {
                string content = EmbeddedResourceHelpers.GetText($"Resources/Scripts/{fileName}");
                content = content.Replace("${SUDO_USER}", _options.SudoUser);
                return new YamlMappingNode
                {
                    { "path", $"/var/lib/cloud/scripts/per-instance/{fileName}" },
                    { "permissions", "0o500" },
                    { "content", new YamlScalarNode(content) { Style = ScalarStyle.Literal } }
                };
            })
            .ToArray();
        writeFiles.AddRange(installerNodes);

        // Boot Command
        string bootCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/bootcmd.sh");
        string[] bootCmdLines = bootCmdContent.SplitIntoLines().ToArray();
        yaml.SetValue("bootcmd", bootCmdLines, false);

        // Run Command
        string runCmdContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/runcmd.sh");
        string[] runCmdLines = runCmdContent.SplitIntoLines().ToArray();
        yaml.SetValue("runcmd", runCmdLines, false);

        // Serialize
        string output = "#cloud-config" + Environment.NewLine + Environment.NewLine;
        output += _serializer.Serialize(yaml);
        return output;
    }
}
