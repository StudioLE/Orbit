using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Clients;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Orbit.WireGuard;
using Tectonic;

namespace Orbit.Configuration;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for a <see cref="Client"/>.
/// </summary>
public class ClientServerConfigActivity : IActivity<ClientServerConfigActivity.Inputs, ClientServerConfigActivity.Outputs>
{
    private readonly ILogger<ClientServerConfigActivity> _logger;
    private readonly ClientProvider _clients;
    private readonly ServerConfigurationProvider _provider;
    private readonly WireGuardSetCommandFactory _wireGuardSetCommandFactory;

    /// <summary>
    /// DI constructor for <see cref="ClientServerConfigActivity"/>.
    /// </summary>
    public ClientServerConfigActivity(
        ILogger<ClientServerConfigActivity> logger,
        ClientProvider clients,
        ServerConfigurationProvider provider,
        WireGuardSetCommandFactory wireGuardSetCommandFactory)
    {
        _logger = logger;
        _clients = clients;
        _provider = provider;
        _wireGuardSetCommandFactory = wireGuardSetCommandFactory;
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
        public ClientId Client { get; set; }
    }

    /// <summary>
    /// The outputs for <see cref="ClientServerConfigActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        Client? clientQuery = _clients.Get(inputs.Client);
        if (clientQuery is not Client client)
            return Failure(HttpStatusCode.NotFound, "The client does not exist.");
        if (!client.TryValidate(_logger))
            return Failure(HttpStatusCode.BadRequest);
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
            return Failure(HttpStatusCode.InternalServerError, "Failed to write server configuration.");
        return Success();
    }

    private Task<Outputs> Success()
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.Created)
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(HttpStatusCode statusCode, string error = "")
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        Outputs outputs = new()
        {
            Status = new(statusCode)
        };
        return Task.FromResult(outputs);
    }
}
