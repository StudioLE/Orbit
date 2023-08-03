using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Generation;
using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Creation;

/// <summary>
/// An <see cref="IActivity"/> to create and store the yaml configuration of a virtual machine network.
/// </summary>
public class CreateNetwork : IActivity<Network, Network?>
{
    private readonly ILogger<CreateNetwork> _logger;
    private readonly IEntityProvider<Network> _networks;
    private readonly IEntityProvider<Server> _servers;
    private readonly NetworkFactory _factory;
    private readonly WireGuardServerConfigFactory _wgConfigFactory;

    /// <summary>
    /// DI constructor for <see cref="CreateNetwork"/>.
    /// </summary>
    public CreateNetwork(
        ILogger<CreateNetwork> logger,
        IEntityProvider<Network> networks,
        IEntityProvider<Server> servers,
        NetworkFactory factory,
        WireGuardServerConfigFactory wgConfigFactory)
    {
        _logger = logger;
        _networks = networks;
        _servers = servers;
        _factory = factory;
        _wgConfigFactory = wgConfigFactory;
    }

    /// <inheritdoc/>
    public Task<Network?> Execute(Network network)
    {
        Func<bool>[] steps =
        {
            () => UpdateNetworkProperties(ref network),
            () => ValidateNetwork(network),
            () => PutNetwork(network),
            () => PutWireGuardConfig(network)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation($"Created network {network.Name}");
            return Task.FromResult<Network?>(network);
        }
        _logger.LogError("Failed to create network.");
        return Task.FromResult<Network?>(null);
    }

    private bool UpdateNetworkProperties(ref Network network)
    {
        network = _factory.Create(network);
        return true;
    }

    private bool ValidateNetwork(Network network)
    {
        return network.TryValidate(_logger);
    }

    private bool PutNetwork(Network network)
    {
        if (_networks.Put(network))
            return true;
        _logger.LogError("Failed to write the network file.");
        return false;
    }

    private bool PutWireGuardConfig(Network network)
    {
        string config = _wgConfigFactory.Create(network);
        string fileName = WireGuardServerConfigFactory.GetFileName(network);
        if (_servers.PutResource(new ServerId(network.Server), fileName, config))
            return true;
        _logger.LogError("Failed to write the wireguard config file.");
        return false;
    }
}
