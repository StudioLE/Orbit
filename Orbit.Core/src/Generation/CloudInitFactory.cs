using Microsoft.Extensions.Options;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using Orbit.Core.Utils.Serialization;
using StudioLE.Core.Patterns;
using StudioLE.Core.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
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
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/user-config-template.yml");

        yaml["hostname"].SetValue(instance.Name);
        yaml["users"][0]["name"].SetValue(_options.SudoUser);
        yaml["users"][1]["name"].SetValue(_options.User);
        yaml["users"][0].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        yaml["users"][1].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        YamlMappingNode[] interfaces = instance
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
        yaml["wireguard"].Replace("interfaces", new YamlSequenceNode(interfaces));
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
            "00-log.sh",
            "10-install-docker.sh",
            "15-install-docker-compose.sh",
            "99-log.sh"
        };
        foreach (string installer in installers)
        {
            string content = EmbeddedResourceHelpers.GetText($"Resources/Scripts/{installer}");
            content = content.Replace("${SUDO_USER}", _options.SudoUser);
            KeyValuePair<YamlNode, YamlNode>[] nodes =
            {
                new("path", $"/var/lib/cloud/scripts/per-instance/{installer}"),
                new("permissions", "0o500"),
                new("content", new YamlScalarNode(content) { Style = ScalarStyle.Literal })
            };
            yaml["write_files"].Add(new YamlMappingNode(nodes));
        }
        // ReSharper restore StringLiteralTypo

        string output = "#cloud-config" + Environment.NewLine + Environment.NewLine;
        output += _serializer.Serialize(yaml);
        return output;
    }
}
