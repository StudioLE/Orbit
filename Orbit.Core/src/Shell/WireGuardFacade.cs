using System.Text;
using Microsoft.Extensions.Logging;
using Orbit.Utils.CommandLine;

namespace Orbit.Shell;

/// <inheritdoc cref="IWireGuardFacade"/>
public class WireGuardFacade : IWireGuardFacade
{
    private readonly ILogger<WireGuardFacade> _logger;

    /// <summary>
    /// The DI constructor for <see cref="WireGuardFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public WireGuardFacade(ILogger<WireGuardFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GeneratePrivateKey()
    {
        StringBuilder output = new();
        Cli cli = new()
        {
            Logger = _logger,
            OnOutput = line =>
            {
                if (!string.IsNullOrEmpty(line))
                    output.Append(line);
            }
        };
        // ReSharper disable once StringLiteralTypo
        int _ = cli.Execute("wg", "genkey");
        return output.ToString();
    }

    /// <inheritdoc/>
    public string GeneratePublicKey(string privateKey)
    {
        StringBuilder output = new();
        Cli cli = new()
        {
            Logger = _logger,
            OnOutput = line =>
            {
                if (!string.IsNullOrEmpty(line))
                    output.Append(line);
            }
        };
        // ReSharper disable once StringLiteralTypo
        int _ = cli.Execute("wg", "pubkey", privateKey);
        return output.ToString();
    }

    /// <inheritdoc/>
    public string GeneratePreSharedKey()
    {
        StringBuilder output = new();
        Cli cli = new()
        {
            Logger = _logger,
            OnOutput = line =>
            {
                if (!string.IsNullOrEmpty(line))
                    output.Append(line);
            }
        };
        // ReSharper disable once StringLiteralTypo
        int _ = cli.Execute("wg", "genpsk");
        return output.ToString();
    }
}
