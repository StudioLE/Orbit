using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="Network"/> with default values.
/// </summary>
public class NetworkFactory : IFactory<Network, Network>
{
    private const string DefaultName = "network";
    private const int DefaultNumberValue = 1;
    private readonly IEntityProvider<Network> _networks;
    private readonly IWireGuardFacade _wg;

    public NetworkFactory(IEntityProvider<Network> networks, IWireGuardFacade wg)
    {
        _networks = networks;
        _wg = wg;
    }

    /// <inheritdoc />
    public Network Create(Network source)
    {
        Network result = new();

        result.Server = source.Server.IsNullOrEmpty()
            ? throw new("Can't create a network without a server.")
            : source.Server;

        result.Number = source.Number == default
            ? DefaultNumber()
            : source.Number;

        result.Dns = source.Dns.IsNullOrEmpty()
            ? $"10.{result.Number}.1.0"
            : source.Dns;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"{DefaultName}-{result.Number:00}"
            : source.Name;

        result.ExternalIPv4 = source.ExternalIPv4.IsNullOrEmpty()
            ? "127.0.0.1"
            : source.ExternalIPv4;

        result.ExternalIPv6 = source.ExternalIPv6.IsNullOrEmpty()
            ? "::1"
            : source.ExternalIPv6;

        if (result.WireGuardPrivateKey.IsNullOrEmpty())
            result.WireGuardPrivateKey = _wg.GeneratePrivateKey() ?? throw new("Failed to generate WireGuard private key");

        if (result.WireGuardPublicKey.IsNullOrEmpty())
            result.WireGuardPublicKey = _wg.GeneratePublicKey(result.WireGuardPrivateKey) ?? throw new("Failed to generate WireGuard public key");

        result.WireGuardPort = source.WireGuardPort == default
            ? 51820
            : source.WireGuardPort;

        return result;
    }

    private int DefaultNumber()
    {
        int[] numbers = _networks
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultNumberValue - 1;
        return finalNumber  + 1;
    }
}
