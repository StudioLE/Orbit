using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Generation;
using Orbit.Provision;
using Orbit.Schema;

namespace Orbit.Creation;

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
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="CreateNetwork"/>.
    /// </summary>
    public CreateNetwork(
        ILogger<CreateNetwork> logger,
        IEntityProvider<Network> networks,
        IEntityProvider<Server> servers,
        NetworkFactory factory,
        WireGuardServerConfigFactory wgConfigFactory,
        CommandContext context)
    {
        _logger = logger;
        _networks = networks;
        _servers = servers;
        _factory = factory;
        _wgConfigFactory = wgConfigFactory;
        _context = context;
    }

    /// <inheritdoc/>
    public Task<Network?> Execute(Network network)
    {
        network = _factory.Create(network);
        if (!_networks.Put(network))
            return Failure(network, "Failed to write the network file.");
        string config = _wgConfigFactory.Create(network);
        string fileName = WireGuardServerConfigFactory.GetFileName(network);
        if (!_servers.PutResource(new ServerId(network.Server), fileName, config))
            return Failure(network, "Failed to write the wireguard config file.");

        _logger.LogInformation($"Created network {network.Name}");
        return Success(network);
    }

    private Task<Network?> Success(Network? network)
    {
        _context.ExitCode = 0;
        return Task.FromResult(network);
    }

    private Task<Network?> Failure(Network? network, string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(network);
    }
}
