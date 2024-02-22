using Orbit.Schema;
using Orbit.Utils.Networking;
using StudioLE.Patterns;

namespace Orbit.Creation.Servers;

/// <summary>
/// A factory for creating a network <see cref="Interface"/> to connect to the internal network.
/// </summary>
public class BridgeInterfaceFactory : IFactory<Server, Interface>
{
    /// <inheritdoc/>
    public Interface Create(Server server)
    {
        return new()
        {
            Name = "br" + server.Number,
            Server = server.Name,
            Type = NetworkType.Bridge,
            MacAddress = MacAddressHelpers.Generate(),
            Addresses =
            [
                $"10.0.{server.Number}.1/24",
                $"fc00::0:{server.Number}:1/112"
            ],
            Gateways =
            [
                $"10.0.{server.Number}.1",
                $"fc00::0:{server.Number}:1"
            ],
            Subnets =
            [
                $"10.0.{server.Number}.0/24",
                $"fc00::0:{server.Number}:0/112"
            ],
            Dns =
            [
                $"10.0.{server.Number}.2",
                $"fc00::0:{server.Number}:2"
            ]
        };
    }
}
