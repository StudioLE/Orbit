using System.ComponentModel.DataAnnotations;
using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the server hosting an <see cref="Instance"/>.
/// </summary>
public sealed class Server : IEntity, IHasValidationAttributes
{
    private const string DefaultName = "server";
    private const int DefaultNumber = 1;

    /// <summary>
    /// The name of the server.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the server.
    /// </summary>
    [Range(1,64)]
    public int Number { get; set; }

    /// <summary>
    /// The IP address or domain name that resolves the server.
    /// </summary>
    [Required]
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// The SSH connection details for the server.
    /// </summary>
    [ValidateComplexType]
    public SshConnection Ssh { get; set; } = new();

    /// <summary>
    /// The SSH connection details for the server.
    /// </summary>
    [ValidateComplexType]
    public WireGuard WireGuard { get; set; } = new();

    // TODO: Replace with ServerFactory
    public void Review(EntityProvider provider)
    {
        if (!Name.IsNullOrEmpty())
        {
            Server? server = provider.Server.Get(Name);
            if (server is not null)
                Number = server.Number;
        }

        if (Number == default)
        {
            // TODO: Change to GetAllClusterIds
            int[] numbers = provider
                .Server
                .GetAll()
                .Select(x => x.Number)
                .ToArray();
            int finalNumber = numbers.Any()
                    ? numbers.Max()
                    : DefaultNumber - 1;
            Number = finalNumber  + 1;
        }

        // TODO: GetName from host file!
        if (Name.IsNullOrEmpty())
            Name = $"{DefaultName}-{Number:00}";
    }
}
