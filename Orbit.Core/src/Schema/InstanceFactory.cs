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
    private readonly IEntityProvider<Instance> _instances;
    private readonly WireGuardFactory _wireGuardFactory;
    private readonly HardwareFactory _hardwareFactory;
    private readonly OSFactory _osFactory;
    private readonly NetworkFactory _networkFactory;

    /// <summary>
    /// The DI constructor for <see cref="InstanceFactory"/>.
    /// </summary>
    public InstanceFactory(
        IEntityProvider<Server> servers,
        IEntityProvider<Instance> instances,
        NetworkFactory networkFactory,
        OSFactory osFactory,
        HardwareFactory hardwareFactory,
        WireGuardFactory wireGuardFactory)
    {
        _servers = servers;
        _instances = instances;
        _networkFactory = networkFactory;
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

        result.Network = _networkFactory.Create(result);
        result.WireGuard = _wireGuardFactory.Create(result);

        return result;
    }

    private string DefaultServer()
    {
        return _servers
                   .GetIndex()
                   .FirstOrDefault()
               ?? throw new("Server must be set if more than one exist.");
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
