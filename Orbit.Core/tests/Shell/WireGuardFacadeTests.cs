using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.Logging;

namespace Orbit.Core.Tests.Shell;

internal sealed class WireGuardFacadeTests
{
    private readonly WireGuardFacade _wg;

    public WireGuardFacadeTests()
    {
        ILogger<WireGuardFacade> logger = LoggingHelpers.CreateConsoleLogger<WireGuardFacade>();
        _wg = new(logger);
    }

    #if DEBUG

    [Test]
    [Category("Misc")]
    public void WireGuardFacade_GeneratePrivateKey()
    {
        // Arrange
        // Act
        string? privateKey = _wg.GeneratePrivateKey();

        // Assert
        Assert.That(privateKey, Is.Not.Null);
        Assert.That(privateKey, Is.Not.Empty);
        Assert.That(privateKey?.Length, Is.EqualTo(44));
        Assert.That(privateKey, Does.Match(Base64SchemaAttribute.Base64Regex));
    }

    [Test]
    [Category("Misc")]
    public void WireGuardFacade_GeneratePublicKey()
    {
        // Arrange
        const string privateKey = "eKDaAiGsNP5b0iygA0MaPl/3WPhZhNOF0M1hdF+UTV0=";

        // Act
        string? publicKey = _wg.GeneratePublicKey(privateKey);

        // Assert
        Assert.That(publicKey, Is.EqualTo("7vG3tXmJWsEZDYt84Ev3fPOteRkkuDLaTfeSiLnPK2s="));
    }

    [Test]
    [Category("Misc")]
    public void WireGuardFacade_GeneratePublicAndPrivate()
    {
        // Arrange
        // Act
        string? privateKey = _wg.GeneratePrivateKey();
        string? publicKey = _wg.GeneratePublicKey(privateKey!);

        // Assert
        Assert.That(privateKey, Is.Not.Null);
        Assert.That(privateKey, Is.Not.Empty);
        Assert.That(privateKey?.Length, Is.EqualTo(44));
        Assert.That(privateKey, Does.Match(Base64SchemaAttribute.Base64Regex));

        Assert.That(publicKey, Is.Not.Null);
        Assert.That(publicKey, Is.Not.Empty);
        Assert.That(publicKey?.Length, Is.EqualTo(44));
        Assert.That(publicKey, Does.Match(Base64SchemaAttribute.Base64Regex));
    }

    #endif
}
