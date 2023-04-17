using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using Orbit.Core.Schema;
using StudioLE.Core.System;

namespace Orbit.Core.Tests;

internal sealed class MultipassTests
{
    private readonly Multipass _multipass;
    private readonly InstanceProvider _provider = InstanceProvider.CreateTemp();

    public MultipassTests()
    {
        string[] args = { "--environment", "Development" };
        using IHost host = Host
            .CreateDefaultBuilder(args)
            .RegisterLaunchServices()
            .Build();
        _multipass = host.Services.GetRequiredService<Multipass>();
    }

    [Test]
    [Explicit("Requires SSH")]
    public void Multipass_List()
    {
        // Arrange
        // Act
        JObject? json = _multipass.List();

        // Preview
        if(json is not null)
            Console.WriteLine(json.ToString());

        // Assert
        if(json is null)
            Assert.Fail();
        else
            Assert.Pass();
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
        bool result = _multipass.Launch(instance, lines.Add);
        string output = lines.Join();

        // Assert
        Assert.That(output, Is.Not.Empty);
        Assert.That(result);
    }
}
