using System.Text;
using Microsoft.Extensions.Logging;
using Orbit.Utils.CommandLine;

namespace Orbit.WireGuard;

/// <inheritdoc cref="IQREncodeFacade"/>
// ReSharper disable once InconsistentNaming
public class QREncodeFacade : IQREncodeFacade
{
    private readonly ILogger<QREncodeFacade> _logger;

    /// <summary>
    /// The DI constructor for <see cref="QREncodeFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public QREncodeFacade(ILogger<QREncodeFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GenerateSvg(string source)
    {
        StringBuilder output = new();
        Cli cli = new()
        {
            Logger = _logger,
            OnOutput = line =>
            {
                if (!string.IsNullOrEmpty(line))
                    output.AppendLine(line);
            }
        };
        int _ = cli.Execute("qrencode", "-t svg", source);
        return output.ToString();
    }
}
