using Cascade.Workflows;
using Microsoft.Extensions.Logging;
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
    private readonly NetworkFactory _factory;

    /// <summary>
    /// DI constructor for <see cref="CreateNetwork"/>.
    /// </summary>
    public CreateNetwork(ILogger<CreateNetwork> logger, IEntityProvider<Network> networks, NetworkFactory factory)
    {
        _logger = logger;
        _networks = networks;
        _factory = factory;
    }

    /// <inheritdoc/>
    public Task<Network?> Execute(Network network)
    {
        Func<bool>[] steps =
        {
            () => UpdateNetworkProperties(ref network),
            () => ValidateNetwork(network),
            () => PutNetwork(network)
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
}
