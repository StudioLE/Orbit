using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Tests.Resources;
using Orbit.Schema;
using StudioLE.Diagnostics;
using StudioLE.Diagnostics.NUnit;
using StudioLE.Serialization;
using StudioLE.Verify.Serialization;

namespace Orbit.Core.Tests;

internal sealed class TestHostTests
{
    private readonly IContext _context = new NUnitContext();
    private ISerializer _serializer = null!;

    [SetUp]
    public async Task SetUp()
    {
        IHost host = await TestHelpers.CreateTestHost();
        _serializer = host.Services.GetRequiredService<ISerializer>();
    }

    [Test]
    [Category("Misc")]
    public async Task TestHost_Server()
    {
        // Arrange
        // Act
        Server server = TestHelpers.GetExampleServer();

        // Assert
        await _context.VerifyAsSerialized(server, _serializer);
    }

    [Test]
    [Category("Misc")]
    public async Task TestHost_Instance()
    {
        // Arrange
        // Act
        Instance instance = TestHelpers.GetExampleInstance();

        // Assert
        await _context.VerifyAsSerialized(instance, _serializer);
    }

    [Test]
    [Category("Misc")]
    public async Task TestHost_Client()
    {
        // Arrange
        // Act
        Client client = TestHelpers.GetExampleClient();

        // Assert
        await _context.VerifyAsSerialized(client, _serializer);
    }
}
