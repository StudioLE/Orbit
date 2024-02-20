using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Hosting;
using Orbit.WireGuard;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Extensions.Logging.Cache;
using StudioLE.Verify;

namespace Orbit.Core.Tests.WireGuard;

// ReSharper disable once InconsistentNaming
internal sealed class QREncodeFacadeTests
{
    public enum StringContent
    {
        HelloWorld,
        WireGuardConfig
    }

    private const string WireGuardConfig = """
        [Interface]
        ListenPort = 61006
        PrivateKey = 8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=
        Address = 10.1.6.9
        Address = fc00::1:6:9
        DNS = 10.1.6.2
        DNS = fc00::1:6:2

        [Peer]
        PublicKey = Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE=
        PreSharedKey = C/quZemAL04qz/eC+WIoelwa+H0oZSiYDSyHNvMVpsY=
        AllowedIPs = 10.1.6.0/24
        AllowedIPs = fc00::1:6:0/112
        Endpoint = 10.0.6.1:61006
        PersistentKeepAlive = 25
        """;
    private readonly IContext _context = new NUnitContext();
    private readonly IQREncodeFacade _qr;
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
        _qr = host.Services.GetRequiredService<IQREncodeFacade>();
    }

    [Test]
    [Category("Misc")]
    [Explicit("Requires QREncode")]
    public async Task QREncodeFacade_GenerateSvg([Values] StringContent stringContent)
    {
        // Arrange
        string source = stringContent switch
        {
            StringContent.HelloWorld => "Hello, world!",
            StringContent.WireGuardConfig => WireGuardConfig,
            _ => throw new ArgumentOutOfRangeException(nameof(stringContent), stringContent, null)
        };

        // Act
        string output = _qr.GenerateSvg(source);

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
        await _context.Verify(output);
    }
}
