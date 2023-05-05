using System.ComponentModel.DataAnnotations;
using Cascade.Workflows;
using Microsoft.Extensions.Logging;
using Orbit.Core.Providers;
using Orbit.Core.Schema;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Shell;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Activities;

public class Launch : IActivity<Launch.Inputs, Launch.Outputs>
{
    private readonly ILogger<Launch> _logger;
    private readonly MultipassFacade _multipass;
    private readonly EntityProvider _provider;
    private Instance _instance = new();

    public class Inputs
    {
        [Required]
        [NameSchema]
        public string Cluster { get; set; } = string.Empty;

        [Required]
        [NameSchema]
        public string Instance { get; set; } = string.Empty;
    }

    public class Outputs
    {
        public int ExitCode { get; set; }
    }

    public Launch(ILogger<Launch> logger, MultipassFacade multipass, EntityProvider provider)
    {
        _logger = logger;
        _multipass = multipass;
        _provider = provider;
    }

    public Task<Outputs> Execute(Inputs inputs)
    {
        if (!GetInstance(inputs.Cluster, inputs.Instance))
            return Failure();

        if (!_instance.TryValidate(_logger))
            return Failure();

        if (!LaunchInstance())
            return Failure();

        return Success();
    }

    private bool GetInstance(string clusterName, string instanceName)
    {
        Instance? instance = _provider.Instance.Get(clusterName, instanceName);
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

    private static Task<Outputs> Success()
    {
        Outputs outputs = new()
        {
            ExitCode = 0
        };
        return Task.FromResult(outputs);
    }

    private static Task<Outputs> Failure()
    {
        Outputs outputs = new()
        {
            ExitCode = 1
        };
        return Task.FromResult(outputs);
    }
}
