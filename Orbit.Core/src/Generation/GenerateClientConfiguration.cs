using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Provision;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Shell;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard config a network client.
/// </summary>
public class GenerateClientConfiguration : IActivity<GenerateClientConfiguration.Inputs, string>
{
    private readonly ILogger<GenerateClientConfiguration> _logger;
    private readonly IEntityProvider<Client> _clients;
    private readonly CommandContext _context;
    private readonly WireGuardClientConfigFactory _factory;
    private readonly QREncodeFacade _qr;

    /// <summary>
    /// DI constructor for <see cref="GenerateClientConfiguration"/>.
    /// </summary>
    public GenerateClientConfiguration(
        ILogger<GenerateClientConfiguration> logger,
        IEntityProvider<Client> clients,
        CommandContext context,
        WireGuardClientConfigFactory factory,
        QREncodeFacade qr)
    {
        _logger = logger;
        _clients = clients;
        _context = context;
        _factory = factory;
        _qr = qr;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateClientConfiguration"/>.
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
    public async Task<string> Execute(Inputs inputs)
    {
        Client client = new();
        string output = string.Empty;
        Func<Task<bool>>[] steps =
        [
            () => Task.FromResult(GetClient(inputs.Client, out client)),
            () => Task.FromResult(ValidateClient(client)),
            () => CreateWireGuardConfig(client)
        ];
        foreach (Func<Task<bool>> step in steps)
        {
            if (await step.Invoke())
                continue;
            _logger.LogError("Failed to generate client configuration.");
            _context.ExitCode = 1;
            return output;
        }
        _logger.LogInformation("Generated client configuration");
        return output;
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

    private async Task<bool> CreateWireGuardConfig(Client client)
    {
        // TODO: for each wireguard create a config and save
        foreach (WireGuardClient wg in client.WireGuard)
        {
            string fileName = $"{wg.Name}.conf";
            string config = _factory.Create(wg);
            // TODO: Make save optional
            if (!_clients.PutResource(new ClientId(client.Name), fileName, config))
            {
                _logger.LogError("Failed to write the wireguard config file.");
                return false;
            }
            string? svg = await _qr.GenerateSvg(config);
            if (svg is null)
            {
                _logger.LogWarning("Failed to generate the QR code.");
                return true;
            }
            // TODO: Make save optional
            if (!_clients.PutResource(new ClientId(client.Name), fileName + ".svg", svg))
            {
                _logger.LogError("Failed to write the QR code file.");
                return false;
            }
        }
        return true;
    }
}
