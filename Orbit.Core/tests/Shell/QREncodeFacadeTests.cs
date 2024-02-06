using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Shell;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Extensions.Logging.Console;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Shell;

// ReSharper disable once InconsistentNaming
internal sealed class QREncodeFacadeTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly QREncodeFacade _qr;

    public QREncodeFacadeTests()
    {
        ILogger<QREncodeFacade> logger = LoggerFactory.Create(builder => builder
                .AddBasicConsole()
                .AddCache())
            .CreateLogger<QREncodeFacade>();
        _qr = new(logger);
    }

    #if DEBUG

    [Test]
    [Category("Misc")]
    public async Task QREncodeFacade_GenerateSvg()
    {
        // Arrange
        const string source = "Hello, world!";

        // Act
        string? output = _qr.GenerateSvg(source);

        // Assert
        Assert.That(output, Is.Not.Null);
        Assert.That(output, Is.Not.Empty);
        await _context.Verify(output!);
    }

    #endif
}
