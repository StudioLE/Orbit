using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="Network"/> with default values.
/// </summary>
public class NetworkFactory : IFactory<Network, Network>
{
    private const string DefaultName = "network";
    private readonly IEntityProvider<Server> _servers;
    private readonly WireGuardServerFactory _wireGuardServerFactory;

    public NetworkFactory(IEntityProvider<Server> servers, WireGuardServerFactory wireGuardServerFactory)
    {
        _servers = servers;
        _wireGuardServerFactory = wireGuardServerFactory;
    }

    /// <inheritdoc/>
    public Network Create(Network source)
    {
        Network result = new();

        result.Server = source.Server.IsNullOrEmpty()
            ? DefaultServer()
            : source.Server;

        Server server = _servers.Get(new ServerId(source.Server)) ?? throw new("Failed to get server.");
        result.Number = source.Number == default
            ? server.Number
            : source.Number;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"{DefaultName}-{result.Number:00}"
            : source.Name;

        result.Interface = source.Interface.IsNullOrEmpty()
            ? "eno1"
            : source.Interface;

        result.ExternalIPv4 = source.ExternalIPv4.IsNullOrEmpty()
            ? "127.0.0.1"
            : source.ExternalIPv4;

        result.ExternalIPv6 = source.ExternalIPv6.IsNullOrEmpty()
            ? "::1"
            : source.ExternalIPv6;

        result.WireGuard = _wireGuardServerFactory.Create(result);

        return result;
    }

    private string DefaultServer()
    {
        return _servers
                   .GetIndex()
                   .LastOrDefault()
               ?? throw new("Server must be set if more than one exist.");
    }
}
