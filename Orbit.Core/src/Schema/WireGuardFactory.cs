using Orbit.Core.Provision;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="WireGuard"/> with default values.
/// </summary>
public class WireGuardFactory : IFactory<Instance, WireGuard>
{
    private readonly IWireGuardFacade _wg;
    private readonly IEntityProvider<Server> _servers;
    private static readonly string[] _defaultAllowedIPs =
    {
        "0.0.0.0/0",
        "::/0"
    };

    /// <summary>
    /// The DI constructor for <see cref="WireGuardFactory"/>.
    /// </summary>
    public WireGuardFactory(IWireGuardFacade wg, IEntityProvider<Server> servers)
    {
        _wg = wg;
        _servers = servers;
    }

    /// <inheritdoc/>
    public WireGuard Create(Instance instance)
    {
        WireGuard result = new()
        {
            PrivateKey = instance.WireGuard.PrivateKey,
            PublicKey = instance.WireGuard.PublicKey,
            Addresses = instance.WireGuard.Addresses,
            ServerPublicKey = instance.WireGuard.ServerPublicKey,
            AllowedIPs = instance.WireGuard.AllowedIPs,
            Endpoint = instance.WireGuard.Endpoint
        };

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if(result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");


        Server server = _servers.Get(new ServerId(instance.Server)) ?? throw new("Failed to get server.");

        if (!result.Addresses.Any())
            result.Addresses = new[]
            {
                $"10.{server.Number}.{instance.Number}.0/24",
                $"fc00:{server.Number}:{instance.Number}::/32"
            };

        if(result.ServerPublicKey.IsNullOrEmpty())
            result.ServerPublicKey = server.WireGuard.PublicKey ?? throw new("Failed to get WireGuard public key from server.");

        if (!result.AllowedIPs.Any())
            result.AllowedIPs = _defaultAllowedIPs;

        if (result.Endpoint.IsNullOrEmpty())
            result.Endpoint = server.Address + ":51820";

        return result;
    }
}
