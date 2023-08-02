using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using Orbit.Core.Utils.Networking;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="Instance"/> with default values.
/// </summary>
public class InstanceFactory : IFactory<Instance, Instance>
{
    private const int DefaultInstanceNumber = 1;
    private const string DefaultRole = "node";
    private static readonly string[] _defaultInstall = {
        "bat",
        "micro",
        "figlet",
        "motd-hostname",
        "motd-system",
        "network-test",
        "upgrade-packages"
    };

    private readonly IEntityProvider<Server> _servers;
    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Instance> _instances;
    private readonly WireGuardClientFactory _wireGuardClientFactory;
    private readonly HardwareFactory _hardwareFactory;
    private readonly OSFactory _osFactory;

    /// <summary>
    /// The DI constructor for <see cref="InstanceFactory"/>.
    /// </summary>
    public InstanceFactory(
        IEntityProvider<Server> servers,
        IEntityProvider<Network> networks,
        IEntityProvider<Instance> instances,
        OSFactory osFactory,
        HardwareFactory hardwareFactory,
        WireGuardClientFactory wireGuardClientFactory)
    {
        _servers = servers;
        _networks = networks;
        _instances = instances;
        _osFactory = osFactory;
        _hardwareFactory = hardwareFactory;
        _wireGuardClientFactory = wireGuardClientFactory;
    }

    /// <inheritdoc />
    public Instance Create(Instance source)
    {
        Instance result = new();

        result.Server = source.Server.IsNullOrEmpty()
            ? DefaultServer()
            : source.Server;

        result.Networks = source.Networks.Any()
            ? source.Networks
            : new []{ DefaultNetwork() };

        result.MacAddress = source.MacAddress.IsNullOrEmpty()
            ? MacAddressHelpers.Generate()
            : source.MacAddress;

        result.Hardware = _hardwareFactory.Create(source.Hardware);
        result.OS = _osFactory.Create(source.OS);

        result.Number = source.Number == default
            ? DefaultNumber()
            : source.Number;

        result.Role = source.Role.IsNullOrEmpty()
            ? DefaultRole
            : source.Role;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"instance-{result.Number:00}"
            : source.Name;

        result.WireGuard = _wireGuardClientFactory.Create(result);
        result.Mounts = source.Mounts.Any()
            ? source.Mounts
            : Array.Empty<Mount>();

        result.Domains = source.Domains.Any()
            ? source.Domains
            : Array.Empty<string>();

        result.Install = source.Install.Any()
            ? source.Install
            : _defaultInstall;

        return result;
    }

    private string DefaultServer()
    {
        return _servers
                   .GetIndex()
                   .LastOrDefault()
               ?? throw new("Server must be set if more than one exist.");
    }

    private string DefaultNetwork()
    {
        return _networks
                   .GetIndex()
                   .FirstOrDefault()
               ?? throw new("Network must be set if more than one exist.");
    }

    private int DefaultNumber()
    {
        int[] numbers = _instances
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultInstanceNumber - 1;
        return finalNumber + 1;
    }
}
