using Orbit.Core.Providers;
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

    private readonly EntityProvider _provider;
    private readonly WireGuardFactory _wireGuardFactory;
    private readonly HardwareFactory _hardwareFactory;
    private readonly OSFactory _osFactory;
    private readonly NetworkFactory _networkFactory;

    /// <summary>
    /// The DI constructor for <see cref="InstanceFactory"/>.
    /// </summary>
    public InstanceFactory(
        EntityProvider provider,
        NetworkFactory networkFactory,
        OSFactory osFactory,
        HardwareFactory hardwareFactory,
        WireGuardFactory wireGuardFactory)
    {
        _provider = provider;
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
            ? DefaultHost()
            : source.Server;

        result.Cluster = source.Cluster.IsNullOrEmpty()
            ? throw new("Cluster not set")
            : source.Cluster;

        result.WireGuard = _wireGuardFactory.Create(source.WireGuard);
        result.Hardware = _hardwareFactory.Create(source.Hardware);
        result.OS = _osFactory.Create(source.OS);

        result.Number = source.Number == default
            ? DefaultNumber(result)
            : source.Number;

        result.Role = source.Role.IsNullOrEmpty()
            ? DefaultRole
            : source.Role;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"{result.Cluster}-{result.Number:00}"
            : source.Name;

        result.Network = _networkFactory.Create(result);

        return result;
    }

    private string DefaultHost()
    {
        return _provider
                   .Server
                   .GetAllNames()
                   .FirstOrDefault()
               ?? throw new("Host must be set if more than one exist.");
    }

    private int DefaultNumber(Instance instance)
    {
        int[] numbers = _provider
            .Instance
            .GetAllInCluster(instance.Cluster)
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultInstanceNumber - 1;
        return finalNumber + 1;
    }
}
