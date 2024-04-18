using Orbit.Schema;
using Orbit.Servers;
using Orbit.Utils;
using Orbit.WireGuard;
using StudioLE.Patterns;

namespace Orbit.Instances;

/// <summary>
/// A factory for creating <see cref="Instance"/> with default values.
/// </summary>
public class InstanceFactory : IFactory<Instance, Task<Instance>>
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

    private readonly ServerProvider _servers;
    private readonly InstanceProvider _instances;
    private readonly WireGuardClientFactory _wireGuardClientFactory;
    private readonly HardwareFactory _hardwareFactory;
    private readonly OSFactory _osFactory;

    /// <summary>
    /// The DI constructor for <see cref="InstanceFactory"/>.
    /// </summary>
    public InstanceFactory(
        ServerProvider servers,
        InstanceProvider instances,
        OSFactory osFactory,
        HardwareFactory hardwareFactory,
        WireGuardClientFactory wireGuardClientFactory)
    {
        _servers = servers;
        _instances = instances;
        _osFactory = osFactory;
        _hardwareFactory = hardwareFactory;
        _wireGuardClientFactory = wireGuardClientFactory;
    }

    /// <inheritdoc/>
    public async Task<Instance> Create(Instance instance)
    {
        if (instance.Server.IsDefault())
            instance.Server = await DefaultServer();
        if (instance.Connections.IsDefault())
            instance.Connections = [instance.Server];
        instance.Hardware = _hardwareFactory.Create(instance.Hardware);
        instance.OS = _osFactory.Create(instance.OS);
        if (instance.Number.IsDefault())
            instance.Number = await DefaultNumber();
        if (instance.Role.IsDefault())
            instance.Role = DefaultRole;
        if (instance.Name.IsDefault())
            instance.Name = new($"instance-{instance.Number:00}");
        instance.WireGuard = await _wireGuardClientFactory.Create(instance);
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

    private async Task<ServerId> DefaultServer()
    {
        Server[] servers = await _servers.GetAll();
        Server? serverQuery = servers
            .OrderBy(x => x.Number)
            .FirstOrNull();
        if(serverQuery is not Server server)
            throw new("Server must be set if more than one exist.");
        return server.Name;
    }

    private async Task<int> DefaultNumber()
    {
        IAsyncEnumerable<Instance> instances = await _instances.GetAll();
        int[] numbers = await instances
            .Select(x => x.Number)
            .ToArrayAsync();
        int finalNumber = numbers.Length != 0
            ? numbers.Max()
            : DefaultInstanceNumber - 1;
        return finalNumber + 1;
    }
}
