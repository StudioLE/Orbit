using System.Text;
using Microsoft.Extensions.Logging;

namespace Orbit.Shell;

/// <inheritdoc cref="IQREncodeFacade"/>
// ReSharper disable once InconsistentNaming
public class QREncodeFacade : ShellCommand, IQREncodeFacade
{
    private StringBuilder _output = null!;

    /// <summary>
    /// The DI constructor for <see cref="QREncodeFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public QREncodeFacade(ILogger<QREncodeFacade> logger) : base(logger)
    {
    }

    /// <inheritdoc/>
    public async Task<string?> GenerateSvg(string source)
    {
        _output = new();
        await Execute("qrencode", "-t svg", source);
        return _output.ToString();
    }

    protected override void OnOutput(string? output)
    {
        if (output is not null)
            _output.AppendLine(output);
    }
}
