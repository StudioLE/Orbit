using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Instances;

/// <summary>
/// A factory for creating <see cref="Instance"/> with default values.
/// </summary>
public class InstanceFactory : IFactory<Instance, Instance>
{
    private const int DefaultInstanceNumber = 1;
    private const string DefaultRole = "node";
    private static readonly string[] _defaultInstall =
    {
        "bat",
        "micro",
        "figlet",
        "motd-hostname",
        "motd-system",
        "network-test",
        "upgrade-packages"
    };
    private static readonly string[] _defaultRun =
    {
        "disable-motd",
        "network-test",
        "upgrade-packages"
    };

    private readonly IEntityProvider<Server> _servers;
    private readonly IEntityProvider<Instance> _instances;
    private readonly ExternalInterfaceFactory _externalInterfaceFactory;
    private readonly InternalInterfaceFactory _internalInterfaceFactory;
    private readonly WireGuardClientFactory _wireGuardClientFactory;
    private readonly HardwareFactory _hardwareFactory;
    private readonly OSFactory _osFactory;

    /// <summary>
    /// The DI constructor for <see cref="InstanceFactory"/>.
    /// </summary>
    public InstanceFactory(
        IEntityProvider<Server> servers,
        IEntityProvider<Instance> instances,
        ExternalInterfaceFactory externalInterfaceFactory,
        InternalInterfaceFactory internalInterfaceFactory,
        OSFactory osFactory,
        HardwareFactory hardwareFactory,
        WireGuardClientFactory wireGuardClientFactory)
    {
        _servers = servers;
        _instances = instances;
        _externalInterfaceFactory = externalInterfaceFactory;
        _internalInterfaceFactory = internalInterfaceFactory;
        _osFactory = osFactory;
        _hardwareFactory = hardwareFactory;
        _wireGuardClientFactory = wireGuardClientFactory;
    }

    /// <inheritdoc/>
    public Instance Create(Instance instance)
    {
        if (instance.Server.IsDefault())
            instance.Server = DefaultServer();
        if (instance.Connections.IsDefault())
            instance.Connections = [instance.Server];
        instance.Hardware = _hardwareFactory.Create(instance.Hardware);
        instance.OS = _osFactory.Create(instance.OS);
        if (instance.Number.IsDefault())
            instance.Number = DefaultNumber();
        if (instance.Role.IsDefault())
            instance.Role = DefaultRole;
        if (instance.Name.IsDefault())
            instance.Name = new($"instance-{instance.Number:00}");
        instance.WireGuard = _wireGuardClientFactory.Create(instance);
        if (instance.Mounts.IsDefault())
            instance.Mounts = Array.Empty<Mount>();
        if (instance.Domains.IsDefault())
            instance.Domains = Array.Empty<string>();
        if (instance.Install.IsDefault())
            instance.Install = _defaultInstall;
        if (instance.Run.IsDefault())
            instance.Run = _defaultRun;
        return instance;
    }

    private ServerId DefaultServer()
    {
        return _servers
                   .GetAll()
                   .OrderBy(x => x.Number)
                   .FirstOrNull()
                   ?.Name
               ?? throw new("Server must be set if more than one exist.");
    }

    private int DefaultNumber()
    {
        int[] numbers = _instances
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Length != 0
            ? numbers.Max()
            : DefaultInstanceNumber - 1;
        return finalNumber + 1;
    }
}
