using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Orbit.WireGuard;
using StudioLE.Serialization;
using Tectonic;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for a <see cref="Client"/>.
/// </summary>
public class ClientServerConfigActivity : IActivity<ClientServerConfigActivity.Inputs, string>
{
    /// <summary>
    ///
    /// </summary>
    public const string FileName = "server-config.yml";
    private readonly ILogger<ClientServerConfigActivity> _logger;
    private readonly IEntityProvider<Client> _clients;
    private readonly ServerConfigurationProvider _provider;
    private readonly CommandContext _context;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;
    private readonly ISerializer _serializer;

    /// <summary>
    /// DI constructor for <see cref="ClientServerConfigActivity"/>.
    /// </summary>
    public ClientServerConfigActivity(
        ILogger<ClientServerConfigActivity> logger,
        IEntityProvider<Client> clients,
        ServerConfigurationProvider provider,
        CommandContext context,
        WireGuardSetCommandFactory wireGuardSetCommandFactory,
        ISerializer serializer)
    {
        _logger = logger;
        _clients = clients;
        _provider = provider;
        _context = context;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
        _serializer = serializer;
    }

    /// <summary>
    /// The inputs for <see cref="ClientServerConfigActivity"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the client to launch.
        /// </summary>
        [Required]
        [NameSchema]
        [Argument]
        public string Client { get; set; } = string.Empty;
    }

    /// <inheritdoc/>
    public Task<string> Execute(Inputs inputs)
    {
        Client? clientQuery = _clients.Get(new ClientId(inputs.Client));
        if (clientQuery is not Client client)
            return Failure("The client does not exist.");
        if (!client.TryValidate(_logger))
            return Failure();
        List<KeyValuePair<ServerId, ShellCommand>> commands = new();
        // Set WireGuard peer
        foreach (WireGuardClient wg in client.WireGuard)
        {
            ShellCommand command = _wireGuardSetCommandFactory.Create(wg);
            commands.Add(new(wg.Interface.Server, command));
        }
        // Write configuration
        Dictionary<ServerId, ShellCommand[]> dictionary = commands
            .GroupBy(x => x.Key, x => x.Value)
            .ToDictionary(x => x.Key, x => x.ToArray());
        if (!_provider.Put(client.Name, dictionary))
            return Failure("Failed to write server configuration.");
        return Success(string.Empty);
    }

    private Task<string> Success(string output)
    {
        _context.ExitCode = 0;
        return Task.FromResult(output);
    }

    private Task<string> Failure(string error = "", int exitCode = 1)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        _context.ExitCode = exitCode;
        return Task.FromResult(error);
    }
}
