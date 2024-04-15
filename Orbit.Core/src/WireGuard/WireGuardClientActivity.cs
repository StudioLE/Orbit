using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;
using Orbit.Clients;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;

namespace Orbit.WireGuard;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard config for a <see cref="Client"/>.
/// </summary>
public class WireGuardClientActivity : IActivity<WireGuardClientActivity.Inputs, string>
{
    private readonly ILogger<WireGuardClientActivity> _logger;
    private readonly ClientProvider _clients;
    private readonly WireGuardConfigProvider _provider;
    private readonly CommandContext _context;
    private readonly WireGuardClientConfigFactory _factory;
    private readonly IQREncodeFacade _qr;

    /// <summary>
    /// DI constructor for <see cref="WireGuardClientActivity"/>.
    /// </summary>
    public WireGuardClientActivity(
        ILogger<WireGuardClientActivity> logger,
        ClientProvider clients,
        WireGuardConfigProvider provider,
        CommandContext context,
        WireGuardClientConfigFactory factory,
        IQREncodeFacade qr)
    {
        _logger = logger;
        _clients = clients;
        _provider = provider;
        _context = context;
        _factory = factory;
        _qr = qr;
    }

    /// <summary>
    /// The inputs for <see cref="WireGuardClientActivity"/>.
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
        bool isValid = client.TryValidate(_logger);
        if (!isValid)
            return Failure();
        foreach (WireGuardClient wg in client.WireGuard)
        {
            string fileName = $"{wg.Interface.Name}.conf";
            string config = _factory.Create(wg);
            // TODO: Make save optional
            if (!_provider.Put(client.Name, fileName, config))
                return Failure("Failed to write the wireguard config file.");
            string svg = _qr.GenerateSvg(config);
            if (string.IsNullOrEmpty(svg))
            {
                _logger.LogWarning("Failed to generate the QR code.");
                continue;
            }
            // TODO: Make save optional
            if (!_provider.Put(client.Name, fileName + ".svg", svg))
                return Failure("Failed to write the QR code file.");
        }
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
