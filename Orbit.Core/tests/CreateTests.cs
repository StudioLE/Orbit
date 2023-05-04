using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NUnit.Framework;
using Orbit.Core.Activities;
using Orbit.Core.Hosting;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Utils.Logging.TestLogger;
using StudioLE.Verify;
using StudioLE.Verify.NUnit;
using Host = Microsoft.Extensions.Hosting.Host;

namespace Orbit.Core.Tests;

internal sealed class CreateTests
{
    private readonly Verify _verify = new(new NUnitVerifyContext());
    private readonly IServiceProvider _services;

    public CreateTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        _services = Host
            .CreateDefaultBuilder()
            .UseTestLoggingProviders()
            .ConfigureServices(services => services
                .AddOrbitServices()
                .AddTestEntityProvider())
            .Build()
            .Services;
    }

    [Test]
    public async Task Create_Execute_Default()
    {
        // Arrange
        TestLogger logger = TestLogger.GetInstance();
        Instance sourceInstance = new()
        {
            WireGuard =
            {
                PrivateKey = "8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=",
                PublicKey = "Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE="
            }
        };
        Create create = _services.GetRequiredService<Create>();

        // Act
        Instance? createdInstance = await create.Execute(sourceInstance);

        // Assert
        if (createdInstance is null)
            Assert.Fail();
        else
            await _verify.AsYaml(createdInstance);

        Assert.That(logger.Logs.Count, Is.EqualTo(1));
        Assert.That(logger.Logs.ElementAt(0).Message, Is.EqualTo($"Created instance {createdInstance!.Name}"));
        EntityProvider provider = _services.GetRequiredService<EntityProvider>();
        Instance storedInstance = provider.Instance.Get(createdInstance.Cluster, createdInstance.Name) ?? throw new("Failed to get instance.");
        await _verify.AsYaml(storedInstance, createdInstance);
        string? networkConfig = provider.Instance.GetResource(createdInstance.Cluster, createdInstance.Name, "network-config.yml");
        string? userConfig = provider.Instance.GetResource(createdInstance.Cluster, createdInstance.Name, "user-config.yml");
        Assert.That(networkConfig, Is.Not.Null);
        Assert.That(userConfig, Is.Not.Null);
    }
}
