using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Shell;

namespace Orbit.Core.Tests.Shell;

internal sealed class MultipassFacadeTests
{
    private readonly Server _server;
    private readonly Instance _instance;
    private readonly MultipassFacade _multipass;

    public MultipassFacadeTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost(services => services
            .AddSingleton<EntityProvider>());
        EntityProvider provider = host.Services.GetRequiredService<EntityProvider>();
        _multipass = host.Services.GetRequiredService<MultipassFacade>();
        _server = provider
                      .Server
                      .GetAll()
                      .FirstOrDefault()
                  ?? throw new("Expected a server.");
        string cluster = provider
                             .Cluster
                             .GetAllNames()
                             .FirstOrDefault()
                         ?? throw new("Expected a cluster.");
        _instance = provider
                        .Instance
                        .GetAllInCluster(cluster)
                        .FirstOrDefault()
                    ?? throw new("Expected an instance.");
    }

#if DEBUG

    [Test]
    [Explicit("Requires SSH")]
    public void Multipass_List()
    {
        // Arrange
        // Act
        JObject? json = _multipass.List(_server);

        // Preview
        if (json is not null)
            Console.WriteLine(json.ToString());

        // Assert
        Assert.That(json, Is.Not.Null);
    }

    [TestCase]
    [Explicit("Requires SSH")]
    public void Multipass_Launch()
    {
        // Arrange
        // Act
        bool result = _multipass.Launch(_instance);

        // Assert
        Assert.That(result);
    }
#endif
}
