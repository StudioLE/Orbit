using Microsoft.Extensions.Logging;
using StudioLE.Orbit.WireGuard;

namespace StudioLE.Orbit.Core.Tests.Resources;

/// <inheritdoc cref="IQREncodeFacade"/>
// ReSharper disable once InconsistentNaming
public class MockQREncodeFacade : IQREncodeFacade
{
    private readonly ILogger<MockQREncodeFacade> _logger;

    /// <summary>
    /// The DI constructor for <see cref="MockQREncodeFacade"/>.
    /// </summary>
    /// <param name="logger"></param>
    public MockQREncodeFacade(ILogger<MockQREncodeFacade> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public string GenerateSvg(string source)
    {
        return """
            <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
            <!-- Created with qrencode 4.1.1 (https://fukuchi.org/works/qrencode/index.html) -->
            <svg width="8.15cm" height="8.15cm" viewBox="0 0 77 77" preserveAspectRatio="none" version="1.1" xmlns="http://www.w3.org/2000/svg">
            	<g id="QRcode">
            		<rect x="0" y="0" width="77" height="77" fill="#ffffff"/>
            		<g id="Pattern" transform="translate(4,4)">
            			<rect x="0" y="0" width="1" height="1" fill="#000000"/>
            		</g>
            	</g>
            </svg>
            """;
    }
}
