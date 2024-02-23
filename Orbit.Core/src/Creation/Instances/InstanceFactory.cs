using Orbit.Provision;
using Orbit.Schema;
using Orbit.Utils;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Creation.Instances;

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
            instance = instance with
            {
                Server = DefaultServer()
            };
        if (instance.Connections.IsDefault())
            instance = instance with
            {
                Connections = [instance.Server]
            };
        instance = instance with
        {
            Hardware = _hardwareFactory.Create(instance.Hardware)
        };
        instance = instance with
        {
            OS = _osFactory.Create(instance.OS)
        };
        if(instance.Number.IsDefault())
            instance = instance with
            {
                Number = DefaultNumber()
            };
        if(instance.Role.IsDefault())
            instance = instance with
            {
                Role = DefaultRole
            };
        if(instance.Name.IsDefault())
            instance = instance with
            {
                Name = $"instance-{instance.Number:00}"
            };
        if(instance.Interfaces.IsDefault())
            instance = instance with
            {
                Interfaces = DefaultInterfaces(instance)
            };
        instance = instance with
        {
            WireGuard = _wireGuardClientFactory.Create(instance)
        };
        if(instance.Mounts.IsDefault())
            instance = instance with
            {
                Mounts = Array.Empty<Mount>()
            };
        if(instance.Domains.IsDefault())
            instance = instance with
            {
                Domains = Array.Empty<string>()
            };
        if(instance.Install.IsDefault())
            instance = instance with
            {
                Install = _defaultInstall
            };
        if(instance.Run.IsDefault())
            instance = instance with
            {
                Run = _defaultRun
            };
        return instance;
    }

    private Interface[] DefaultInterfaces(Instance result)
    {
        Interface internalInterface = _internalInterfaceFactory.Create(result);
        Interface? externalInterfaceQuery = _externalInterfaceFactory.Create(result);
        return externalInterfaceQuery is Interface externalInterface
            ? [internalInterface, externalInterface]
            : [internalInterface];
    }

    private string DefaultServer()
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
