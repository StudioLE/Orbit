using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using StudioLE.Serialization;

namespace Orbit.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for a client.
/// </summary>
public class GenerateServerConfigurationForClient : IActivity<GenerateServerConfigurationForClient.Inputs, GenerateServerConfiguration.Outputs>
{
    private readonly ILogger<GenerateServerConfigurationForClient> _logger;
    private readonly IEntityProvider<Client> _clients;
    private readonly IEntityProvider<Network> _networks;
    private readonly CommandContext _context;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;
    private readonly ISerializer _serializer;

    /// <summary>
    /// DI constructor for <see cref="GenerateServerConfigurationForClient"/>.
    /// </summary>
    public GenerateServerConfigurationForClient(
        ILogger<GenerateServerConfigurationForClient> logger,
        IEntityProvider<Client> clients,
        IEntityProvider<Network> networks,
        CommandContext context,
        WireGuardSetCommandFactory wireGuardSetCommandFactory,
        ISerializer serializer)
    {
        _logger = logger;
        _clients = clients;
        _networks = networks;
        _context = context;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
        _serializer = serializer;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateServerConfigurationForClient"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the client to launch.
        /// </summary>
        [Required]
        [NameSchema]
        public string Client { get; set; } = string.Empty;
    }

    /// <inheritdoc/>
    public Task<GenerateServerConfiguration.Outputs> Execute(Inputs inputs)
    {
        Client client = new();
        List<KeyValuePair<string, PreparedShellCommand>> commands = new();
        Func<bool>[] steps =
        {
            () => GetClient(inputs.Client, out client),
            () => ValidateClient(client),
            () => SetWireGuardPeer(client, commands),
            () => WriteConfiguration(client, commands)
        };
        bool isSuccess = steps.All(step => step.Invoke());
        if (isSuccess)
        {
            _logger.LogInformation("Generated server configuration");
            return Task.FromResult(new GenerateServerConfiguration.Outputs());
        }
        _logger.LogError("Failed to generate server configuration");
        _context.ExitCode = 1;
        return Task.FromResult(new GenerateServerConfiguration.Outputs());
    }

    private bool GetClient(string clientName, out Client client)
    {
        Client? result = _clients.Get(new ClientId(clientName));
        client = result!;
        if (result is null)
        {
            _logger.LogError("The client does not exist.");
            return false;
        }
        return true;
    }

    private bool ValidateClient(Client client)
    {
        return client.TryValidate(_logger);
    }

    private bool SetWireGuardPeer(Client client, List<KeyValuePair<string, PreparedShellCommand>> commands)
    {
        foreach (WireGuardClient wg in client.WireGuard)
        {
            Network? network = _networks.Get(new NetworkId(wg.Network));
            if (network is null)
            {
                _logger.LogError("Failed to get network");
                return false;
            }
            PreparedShellCommand command = _wireGuardSetCommandFactory.Create(wg);
            commands.Add(new(network.Server, command));
        }
        return true;
    }

    private bool WriteConfiguration(Client client, List<KeyValuePair<string, PreparedShellCommand>> commands)
    {
        Dictionary<string, PreparedShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());

        string serialized = _serializer.Serialize(dictionary);
        if (!_clients.PutResource(new ClientId(client.Name), GenerateServerConfiguration.FileName, serialized))
        {
            _logger.LogError("Failed to write server configuration.");
            return false;
        }
        return true;
    }
}
