using Orbit.Core.Provision;
using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

/// <summary>
/// A factory for creating <see cref="Network"/> with default values.
/// </summary>
public class NetworkFactory : IFactory<Network, Network>
{
    private const string DefaultName = "network";
    private const int DefaultNumberValue = 1;
    private readonly IEntityProvider<Network> _networks;

    public NetworkFactory(IEntityProvider<Network> networks)
    {
        _networks = networks;
    }

    /// <inheritdoc />
    public Network Create(Network source)
    {
        Network result = new();

        result.Server = source.Server.IsNullOrEmpty()
            ? throw new("Can't create a network without a server.")
            : source.Server;

        result.Number = source.Number == default
            ? DefaultNumber()
            : source.Number;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"{DefaultName}-{result.Number:00}"
            : source.Name;

        return result;
    }

    private int DefaultNumber()
    {
        int[] numbers = _networks
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultNumberValue - 1;
        return finalNumber  + 1;
    }
}
