using Microsoft.Extensions.Options;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils.Yaml;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.CloudInit;

public class UserConfigFactory : IFactory<Instance, string>
{
    private readonly CloudInitOptions _options;
    private readonly WireGuardClientConfigFactory _wgConfigFactory;
    private readonly NetplanFactory _netplanFactory;
    private readonly InstallFactory _installFactory;
    private readonly RunFactory _runFactory;

    /// <summary>
    /// DI constructor for <see cref="UserConfigFactory"/>.
    /// </summary>
    public UserConfigFactory(
        IOptions<CloudInitOptions> options,
        WireGuardClientConfigFactory wgConfigFactory,
        NetplanFactory netplanFactory,
        InstallFactory installFactory,
        RunFactory runFactory)
    {
        _options = options.Value;
        _wgConfigFactory = wgConfigFactory;
        _netplanFactory = netplanFactory;
        _installFactory = installFactory;
        _runFactory = runFactory;
    }

    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        string sshdConfigContent = EmbeddedResourceHelpers.GetText("Resources/Templates/sshd_config");
        string netplanContent = _netplanFactory.Create(instance);
        string configureContent = EmbeddedResourceHelpers.GetText("Resources/Scripts/50-orbit-configure");
        string installContent = _installFactory.Create(instance);
        string runContent = _runFactory.Create(instance);
        string wireguardSection = WireGuardSection(instance);
        return $"""
            #cloud-config

            packages:
            - resolvconf
            ssh_genkeytypes:
            - rsa
            - ed25519
            hostname: {instance.Name}
            groups:
            - docker
            users:
            - name: {_options.SudoUser}
              groups: sudo, docker
              shell: /bin/bash
              sudo: ALL=(ALL) NOPASSWD:ALL
              lock_passwd: true
              ssh_authorized_keys:{_options.SshAuthorizedKeys.AsYamlSequence(1)}
            - name: user
              shell: /bin/bash
              lock_passwd: true
              ssh_authorized_keys:{_options.SshAuthorizedKeys.AsYamlSequence(1)}
            {wireguardSection}
            write_files:
            - path: /etc/ssh/sshd_config
              append: true
              content: |
            {sshdConfigContent.Indent(2)}
            - path: /etc/netplan/10-custom.yaml
              content: |
            {netplanContent.Indent(2)}
            - path: /var/lib/cloud/scripts/per-instance/30-orbit-configure
              permissions: 0o500
              content: |
            {configureContent.Indent(2)}
            - path: /var/lib/cloud/scripts/per-instance/50-orbit-install
              permissions: 0o500
              content: |
            {installContent.Indent(2)}
            - path: /var/lib/cloud/scripts/per-instance/70-orbit-run
              permissions: 0o500
              content: |
            {runContent.Indent(2)}

            """;
    }

    private string WireGuardSection(Instance instance)
    {
        if (instance.WireGuard.Length == 0)
            return string.Empty;
        string output = """
            wireguard:
              interfaces:

            """;
        foreach (WireGuardClient wg in instance.WireGuard)
        {
            string config = _wgConfigFactory
                .Create(wg)
                .Indent(3);
            output += $"""
                  - name: {wg.Interface.Name}
                    config_path: /etc/wireguard/{wg.Interface.Name}.conf
                    content: |
                {config}
                """;
        }
        return output;
    }
}
