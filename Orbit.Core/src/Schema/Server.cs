using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for the server hosting an <see cref="Instance"/>.
/// </summary>
public sealed class Server : IEntity, IHasValidationAttributes
{
    /// <summary>
    /// The name of the server.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the server.
    /// </summary>
    [Range(1, 64)]
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
}
