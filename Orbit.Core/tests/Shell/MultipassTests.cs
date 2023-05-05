using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using StudioLE.Core.System;

namespace Orbit.Core.Tests.Shell;

internal sealed class MultipassTests
{
    private readonly InstanceFactory _instanceFactory;
    private readonly Server _server;
    private readonly Multipass _multipass;

    public MultipassTests()
    {
        #if DEBUG
        Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
        #endif
        IHost host = TestHelpers.CreateTestHost(services => services
            .AddSingleton<EntityProvider>());
        _instanceFactory = host.Services.GetRequiredService<InstanceFactory>();
        EntityProvider provider = host.Services.GetRequiredService<EntityProvider>();
        _multipass = host.Services.GetRequiredService<Multipass>();
        _server = provider
                            .Server
                            .GetAll()
                            .FirstOrDefault()
                        ?? throw new("Expected a server.");
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

    [Test]
    [Explicit("Requires SSH")]
    public void Multipass_Launch()
    {
        // Arrange
        Instance instance = _instanceFactory.Create(new());
        List<string> lines = new();

        // Act
        bool result = _multipass.Launch(instance);
        string output = lines.Join();

        // Assert
        Assert.That(output, Is.Not.Empty);
        Assert.That(result);
    }
#endif
}
