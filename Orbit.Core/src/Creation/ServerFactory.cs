using Orbit.Core.Provision;
using Orbit.Core.Schema;
using Orbit.Core.Utils;
using StudioLE.Patterns;

namespace Orbit.Core.Creation;

/// <summary>
/// A factory for creating <see cref="Server"/> with default values.
/// </summary>
public class ServerFactory : IFactory<Server, Server>
{
    private const string DefaultName = "server";
    private const int DefaultNumberValue = 1;
    private const string DefaultAddress = "localhost";
    private readonly IEntityProvider<Server> _servers;


    public ServerFactory(IEntityProvider<Server> servers)
    {
        _servers = servers;
    }

    /// <inheritdoc />
    public Server Create(Server source)
    {
        Server result = new();

        result.Number = source.Number == default
            ? DefaultNumber()
            : source.Number;

        result.Name = source.Name.IsNullOrEmpty()
            ? $"{DefaultName}-{result.Number:00}"
            : source.Name;

        result.Address = source.Address.IsNullOrEmpty()
            ? DefaultAddress
            : source.Address;

        result.Ssh = source.Ssh;

        return result;
    }

    private int DefaultNumber()
    {
        int[] numbers = _servers
            .GetAll()
            .Select(x => x.Number)
            .ToArray();
        int finalNumber = numbers.Any()
            ? numbers.Max()
            : DefaultNumberValue - 1;
        return finalNumber + 1;
    }
}
