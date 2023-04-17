using Microsoft.Extensions.Logging;
using Orbit.Core.Schema;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Activities;

public class Launch
{
    private readonly ILogger<Launch> _logger;
    private readonly Multipass _multipass;
    private readonly InstanceProvider _provider;
    private Instance _instance = new();

    public Launch(ILogger<Launch> logger, Multipass multipass, InstanceProvider provider)
    {
        _logger = logger;
        _multipass = multipass;
        _provider = provider;
    }

    public bool Execute(string id)
    {
        if (!GetInstance(id))
            return false;

        if (!_instance.TryValidate(_logger))
            return false;

        if (!LaunchInstance())
            return false;

        return true;
    }

    private bool GetInstance(string id)
    {
        Instance? instance = _provider.Get(id);
        if (instance is null)
        {
            _logger.LogError("The instance does not exist.");
            return false;
        }
        _instance = instance;
        return true;
    }

    private bool LaunchInstance()
    {
        return _multipass.Launch(_instance);
    }
}
