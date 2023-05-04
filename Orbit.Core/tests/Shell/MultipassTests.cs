using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Shell;
using Orbit.Core.Utils.Logging;
using StudioLE.Core.System;

namespace Orbit.Core.Tests.Shell;

internal sealed class MultipassTests
{
    private readonly Multipass _multipass = new();
    private readonly EntityProvider _provider;

    public MultipassTests()
    {
        ILogger<EntityProvider> logger = LoggingHelpers.CreateConsoleLogger<EntityProvider>();
        _provider = new(new(), logger);
        Host host = _provider
            .Host
            .GetAll()
            .FirstOrDefault() ?? throw new("Expected a host");
        _multipass.Connect(host);
    }

#if DEBUG

    [Test]
    [Explicit("Requires SSH")]
    public void Multipass_List()
    {
        // Arrange

        // Act
        JObject? json = _multipass.List();

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
        List<string> lines = new();
        Instance instance = new();
        instance.Review(_provider);

        // Act
        bool result = _multipass.Launch(instance);
        string output = lines.Join();

        // Assert
        Assert.That(output, Is.Not.Empty);
        Assert.That(result);
    }
#endif
}
