using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="WireGuardServer"/> with default values.
/// </summary>
public class WireGuardServerFactory : IFactory<Network, WireGuardServer>
{
    private readonly IWireGuardFacade _wg;
    private readonly IIPAddressStrategy _ip;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardServerFactory"/>.
    /// </summary>
    public WireGuardServerFactory(IWireGuardFacade wg, IIPAddressStrategy ip)
    {
        _wg = wg;
        _ip = ip;
    }

    /// <inheritdoc/>
    public WireGuardServer Create(Network network)
    {
        WireGuardServer result = new()
        {
            Name = network.WireGuard.Name,
            Port = network.WireGuard.Port,
            PrivateKey = network.WireGuard.PrivateKey,
            PublicKey = network.WireGuard.PublicKey,
            Dns = network.WireGuard.Dns
        };

        if (result.Name.IsNullOrEmpty())
            result.Name = $"wg{network.Number}";

        if (result.Port == default)
            result.Port = 61000 + network.Number;

        if (result.PrivateKey.IsNullOrEmpty())
            result.PrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.PublicKey.IsNullOrEmpty())
            result.PublicKey = _wg.GeneratePublicKey(result.PrivateKey) ?? throw new("Failed to generate WireGuard public key");

        if (!result.Dns.Any())
            result.Dns = new[]
            {
                _ip.GetWireGuardDnsIPv4(network),
                _ip.GetWireGuardDnsIPv6(network)
            };

        return result;
    }
}
