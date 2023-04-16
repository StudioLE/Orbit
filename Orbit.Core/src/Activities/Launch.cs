using Microsoft.Extensions.Logging;

namespace Orbit.Core.Activities;

public class Launch
{
    private readonly ILogger<Launch> _logger;
    private readonly Multipass _multipass;

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
