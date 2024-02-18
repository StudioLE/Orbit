using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Hosting;
using Orbit.Shell;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.Shell;

// ReSharper disable once InconsistentNaming
internal sealed class QREncodeFacadeTests
{
    private readonly IContext _context = new NUnitContext();
    private readonly QREncodeFacade _qr;
    private readonly IReadOnlyCollection<LogEntry> _logs;

    public QREncodeFacadeTests()
    {
        IHost host = Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices(services => services
                .AddOrbitServices())
            .Build();
        _logs = host.Services.GetCachedLogs();
        _qr = host.Services.GetRequiredService<QREncodeFacade>();
    }

    [Test]
    [Category("Misc")]
    public async Task QREncodeFacade_GenerateSvg()
    {
        // Arrange
        const string source = "Hello, world!";

        // Act
        string? output = await _qr.GenerateSvg(source);

        // Assert
        if (_logs.Any(log => log is
            {
                LogLevel: LogLevel.Error,
                Message: "Failed to start qrencode. Are you sure it's installed?"
            }))
        {
            Assert.Inconclusive();
            return;
        }
        Assert.That(output, Is.Not.Null);
        Assert.That(output, Is.Not.Empty);
        await _context.Verify(output!);
    }
}
