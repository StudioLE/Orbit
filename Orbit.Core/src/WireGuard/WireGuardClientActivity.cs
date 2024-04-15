using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using Orbit.Clients;
using Orbit.Schema;
using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Tectonic;
using Tectonic.Assets;

namespace Orbit.WireGuard;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard config for a <see cref="Client"/>.
/// </summary>
public class WireGuardClientActivity : IActivity<WireGuardClientActivity.Inputs, WireGuardClientActivity.Outputs>
{
    private readonly ILogger<WireGuardClientActivity> _logger;
    private readonly ClientProvider _clients;
    private readonly WireGuardConfigProvider _provider;
    private readonly WireGuardClientConfigFactory _factory;
    private readonly IQREncodeFacade _qr;

    /// <summary>
    /// DI constructor for <see cref="WireGuardClientActivity"/>.
    /// </summary>
    public WireGuardClientActivity(
        ILogger<WireGuardClientActivity> logger,
        ClientProvider clients,
        WireGuardConfigProvider provider,
        WireGuardClientConfigFactory factory,
        IQREncodeFacade qr)
    {
        _logger = logger;
        _clients = clients;
        _provider = provider;
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
        public ClientId Client { get; set; }
    }

    /// <summary>
    /// The outputs for <see cref="WireGuardClientActivity"/>.
    /// </summary>
    public class Outputs
    {
        /// <summary>
        /// The status.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The generated assets.
        /// </summary>
        public IReadOnlyCollection<InternalAsset> Assets { get; set; } = Array.Empty<InternalAsset>();
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        Client? clientQuery = _clients.Get(inputs.Client);
        if (clientQuery is not Client client)
            return Failure(HttpStatusCode.NotFound, "The client does not exist.");
        bool isValid = client.TryValidate(_logger);
        if (!isValid)
            return Failure(HttpStatusCode.BadRequest);
        List<InternalAsset> assets = new();
        foreach (WireGuardClient wg in client.WireGuard)
        {
            string fileName = $"{wg.Interface.Name}.conf";
            string config = _factory.Create(wg);
            assets.Add(new()
            {
                Name = fileName,
                Content = config
            });
            // TODO: Make save optional
            if (!_provider.Put(client.Name, fileName, config))
                return Failure(HttpStatusCode.InternalServerError, "Failed to write the wireguard config file.");
            string svg = _qr.GenerateSvg(config);
            if (string.IsNullOrEmpty(svg))
            {
                _logger.LogWarning("Failed to generate the QR code.");
                continue;
            }
            assets.Add(new()
            {
                Name = fileName + ".svg",
                ContentType = "image/svg+xml",
                Content = svg
            });
            // TODO: Make save optional
            if (!_provider.Put(client.Name, fileName + ".svg", svg))
                return Failure(HttpStatusCode.InternalServerError, "Failed to write the QR code file.");
        }
        return Success(assets);
    }

    private Task<Outputs> Success(IReadOnlyCollection<InternalAsset> assets)
    {
        Outputs outputs = new()
        {
            Status = new(HttpStatusCode.OK),
            Assets = assets
        };
        return Task.FromResult(outputs);
    }

    private Task<Outputs> Failure(HttpStatusCode statusCode, string? error = null)
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
