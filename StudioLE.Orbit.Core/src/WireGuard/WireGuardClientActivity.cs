using System.ComponentModel.DataAnnotations;
using System.Net;
using Microsoft.Extensions.Logging;
using StudioLE.Orbit.Assets;
using StudioLE.Orbit.Clients;
using StudioLE.Orbit.Schema;
using StudioLE.Orbit.Schema.DataAnnotations;
using StudioLE.Orbit.Utils.DataAnnotations;
using Tectonic;
using FileInfo = StudioLE.Storage.Files.FileInfo;

namespace StudioLE.Orbit.WireGuard;

/// <summary>
/// An <see cref="IActivity"/> to generate the WireGuard config for a <see cref="Client"/>.
/// </summary>
public class WireGuardClientActivity : ActivityBase<WireGuardClientActivity.Inputs, WireGuardClientActivity.Outputs>
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
        public IReadOnlyCollection<FileInfo> Assets { get; set; } = Array.Empty<FileInfo>();
    }

    /// <inheritdoc/>
    public override async Task<Outputs?> Execute(Inputs inputs)
    {
        Client? clientQuery = await _clients.Get(inputs.Client);
        if (clientQuery is not Client client)
            return Failure(HttpStatusCode.NotFound, "The client does not exist.");
        bool isValid = client.TryValidate(_logger);
        if (!isValid)
            return Failure(HttpStatusCode.BadRequest);
        List<FileInfo> assets = new();
        foreach (WireGuardClient wg in client.WireGuard)
        {
            string fileName = $"{wg.Name}.conf";
            string config = await _factory.Create(wg);
            assets.Add(AssetHelpers.Create(fileName, config));
            // TODO: Make save optional
            if (!await _provider.Put(client.Name, fileName, config))
                return Failure(HttpStatusCode.InternalServerError, "Failed to write the wireguard config file.");
            string svg = _qr.GenerateSvg(config);
            if (string.IsNullOrEmpty(svg))
            {
                _logger.LogWarning("Failed to generate the QR code.");
                continue;
            }
            assets.Add(AssetHelpers.Create(fileName, svg, "image/svg+xml"));
            // TODO: Make save optional
            if (! await _provider.Put(client.Name, fileName + ".svg", svg))
                return Failure(HttpStatusCode.InternalServerError, "Failed to write the QR code file.");
        }
        return Success(assets);
    }

    private Outputs Success(IReadOnlyCollection<FileInfo> assets)
    {
        return new()
        {
            Status = new(HttpStatusCode.OK),
            Assets = assets
        };
    }

    private Outputs Failure(HttpStatusCode statusCode, string? error = null)
    {
        if (!string.IsNullOrEmpty(error))
            _logger.LogError(error);
        return new()
        {
            Status = new(statusCode)
        };
    }
}
