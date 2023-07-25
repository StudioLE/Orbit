using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;
using Orbit.Core.Utils.Serialization;
using StudioLE.Core.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the cloud init user-data yaml for a virtual machine instance.
/// </summary>
public class GenerateCloudInit : IActivity<GenerateCloudInit.Inputs, string>
{
    public const string FileName = "user-config.yml";
    private readonly ILogger<GenerateCloudInit> _logger;
    private readonly CloudInitOptions _options;
    private readonly IEntityProvider<Instance> _instances;
    private readonly ISerializer _serializer;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="GenerateCloudInit"/>.
    /// </summary>
    public GenerateCloudInit(
        ILogger<GenerateCloudInit> logger,
        IOptions<CloudInitOptions> options,
        IEntityProvider<Instance> instances,
        ISerializer serializer,
        CommandContext context)
    {
        _logger = logger;
        _options = options.Value;
        _instances = instances;
        _serializer = serializer;
        _context = context;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateCloudInit"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to launch.
        /// </summary>
        [Required]
        [NameSchema]
        public string Instance { get; set; } = string.Empty;
    }

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Instance instance = new();
        string output = string.Empty;
        string wireGuardConfig = string.Empty;
        Func<bool>[] steps =
        {
            () => ValidateOptions(),
            () => GetInstance(inputs.Instance, out instance),
            () => ValidateInstance(instance),
            () => GetWireGuardConfig(instance, out wireGuardConfig),
            () => CreateUserConfig(instance, wireGuardConfig, out output)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Generated cloud-init for instance {instance.Name}");
            return Task.FromResult(output);
        }
        _logger.LogError("Failed to generate cloud-init.");
        _context.ExitCode = 1;
        return Task.FromResult(output);
    }

    private bool GetWireGuardConfig(Instance instance, out string output)
    {
        string? result = _instances.GetResource(new InstanceId(instance.Name), GenerateWireGuard.FileName);
        if(result is not null)
        {
            output = result;
            return true;
        }
        _logger.LogError("Failed to retrieve WireGuard config.");
        output = string.Empty;
        return false;
    }

    private bool ValidateOptions()
    {
        return _options.TryValidate(_logger);
    }

    private bool GetInstance(string instanceName, out Instance instance)
    {
        Instance? result = _instances.Get(new InstanceId(instanceName));
        instance = result!;
        if (result is null)
        {
            _logger.LogError("The instance does not exist.");
            return false;
        }
        return true;
    }

    private bool ValidateInstance(Instance instance)
    {
        return instance.TryValidate(_logger);
    }

    private bool CreateUserConfig(Instance instance, string wireGuardConfig, out string output)
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/user-config-template.yml");

        yaml["hostname"].SetValue(instance.Name);
        yaml["users"][0]["name"].SetValue(_options.SudoUser);
        yaml["users"][1]["name"].SetValue(_options.User);
        yaml["users"][0].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        yaml["users"][1].Replace("ssh_authorized_keys", _options.SshAuthorizedKeys);
        YamlMappingNode wg0 = new()
        {
            { "name", "wg0" },
            { "config_path", "/etc/wireguard/wg0.conf" },
            { "content", new YamlScalarNode(wireGuardConfig) { Style = ScalarStyle.Literal } }
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

        output = "#cloud-config" + Environment.NewLine + Environment.NewLine;
        output += _serializer.Serialize(yaml);
        if (_instances.PutResource(new InstanceId(instance.Name), GenerateCloudInit.FileName, output))
            return true;
        _logger.LogError("Failed to write the user config file.");
        return false;
    }
}
