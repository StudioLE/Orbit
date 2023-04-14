using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;

namespace Orbit.Core.Schema;

public sealed class Network
{
    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Address { get; set; } = string.Empty;

    [IPSchema]
    // ReSharper disable once InconsistentNaming
    public string Gateway { get; set; } = string.Empty;

    public void Review(Instance instance)
    {
        if (Address.IsNullOrEmpty())
            Address = $"10.0.{instance.Cluster}.{instance.Number}";
        if (Gateway.IsNullOrEmpty())
            Gateway = "10.0.0.1";
    }
}
