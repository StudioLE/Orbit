using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Orbit.Core.Activities;

public class Launch
{
    private readonly ILogger<Launch> _logger;
    private readonly Multipass _multipass;

    public Launch()
    {
        using IHost host = Host
            .CreateDefaultBuilder()
            .RegisterLaunchServices()
            .Build();
        _logger = host.Services.GetRequiredService<ILogger<Launch>>();
        _multipass = host.Services.GetRequiredService<Multipass>();
    }

    public Launch(ILogger<Launch> logger, Multipass multipass)
    {
        _logger = logger;
        _multipass = multipass;
    }

    public bool Execute(string id)
    {
        _multipass.Launch(id, Console.WriteLine);
        return true;
    }
}
