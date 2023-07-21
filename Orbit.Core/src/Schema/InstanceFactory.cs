using Orbit.Core.Provision;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="Instance"/> with default values.
/// </summary>
public class InstanceFactory : IFactory<Instance, Instance>
{
    private const int DefaultInstanceNumber = 1;
    private const string DefaultRole = "node";

    private readonly IEntityProvider<Server> _servers;
    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Instance> _instances;
    private readonly WireGuardFactory _wireGuardFactory;
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
        WireGuardFactory wireGuardFactory)
    {
        _servers = servers;
        _networks = networks;
        _instances = instances;
        _osFactory = osFactory;
        _hardwareFactory = hardwareFactory;
        _wireGuardFactory = wireGuardFactory;
    }

    /// <inheritdoc />
    public Instance Create(Instance source)
    {
        Instance result = new();

        result.Server = source.Server.IsNullOrEmpty()
            ? DefaultServer()
            : source.Server;

        result.Network = source.Network.IsNullOrEmpty()
            ? DefaultNetwork()
            : source.Network;

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

        result.WireGuard = _wireGuardFactory.Create(result);
        result.Mounts = source.Mounts.Any()
            ? source.Mounts
            : Array.Empty<Mount>();

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
