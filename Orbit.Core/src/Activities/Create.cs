using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Core.Schema;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Activities;

public class Create
{
    private readonly ILogger<Create> _logger;
    private readonly CreateOptions _options;
    private Instance _instance = new();

    public Create(ILogger<Create> logger, CreateOptions options)
    {
        _logger = logger;
        _options = options;
    }

    public Instance? Execute(Instance instance)
    {
        _instance = instance;
        instance.Review();

        if (!ValidateInstance())
            return null;

        if (!CreateInstance())
            return null;

        if (!CreateNetworkConfig())
            return null;

        if (!CreateUserConfig())
            return null;

        return _instance;
    }

    private bool ValidateInstance()
    {
        List<ValidationResult> results = new();
        ValidationContext context = new(_instance);
        if (Validator.TryValidateObject(_instance, context, results, validateAllProperties: true))
            return true;
        _logger.LogError("The following validation errors occured:");
        foreach (ValidationResult result in results)
            _logger.LogError(result.ErrorMessage);
        return false;
    }

    private bool CreateInstance()
    {
        if (InstanceApi.Put(_instance))
            return true;
        _logger.LogError("Failed to write the instance file.");
        return false;
    }

    private bool CreateNetworkConfig()
    {
        YamlNode yaml = EmbeddedResourceHelpers.GetYaml("Resources/Templates/network-config-template.yml");
        YamlNode adapter = yaml["network"]["ethernets"]["eth0"];

        adapter["addresses"][0].SetValue(_instance.Network.Address);
        adapter["routes"][0]["via"].SetValue(_instance.Network.Gateway);

        if (InstanceApi.PutResource(_instance.Id, "network-config.yml", yaml))
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

        if (InstanceApi.PutResource(_instance.Id, "user-config.yml", yaml))
            return true;
        _logger.LogError("Failed to write the user config file.");
        return false;
    }
}
