using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;
using Orbit.Utils.Networking;

namespace Orbit.Schema;

/// <summary>
/// The schema for the network interface configuration of an <see cref="Instance"/>.
/// </summary>
public record struct Interface() : IHasValidationAttributes
{
    /// <summary>
    /// The name of the interface.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The name of the server the interface connects to.
    /// </summary>
    [NameSchema]
    public ServerId Server { get; set; } = new();

    /// <summary>
    /// The type of the network.
    /// </summary>
    public NetworkType Type { get; set; } = NetworkType.Unknown;

    /// <summary>
    /// The mac address of the interface.
    /// </summary>
    public MacAddress MacAddress { get; set; } = new();

    /// <summary>
    /// The IPv4 or IPv6 addresses of the interface.
    /// </summary>
    public string[] Addresses { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The gateway address of the interface.
    /// </summary>
    public string[] Gateways { get; set; } = Array.Empty<string>();


    /// <summary>
    /// The subnets of the interface.
    /// </summary>
    public string[] Subnets { get; set; } = Array.Empty<string>();

    /// <summary>
    /// The dns resolver addresses of the interface.
    /// </summary>
    public string[] Dns { get; set; } = Array.Empty<string>();
}

/// <summary>
/// The type of network.
/// </summary>
public enum NetworkType
{
    /// <summary>
    /// Unknown
    /// </summary>
    Unknown = -1,

    /// <summary>
    /// A bridge network.
    /// </summary>
    Bridge = 0,

    /// <summary>
    /// A WireGuard network.
    /// </summary>
    WireGuard = 1,

    /// <summary>
    /// A routed nic network.
    /// </summary>
    RoutedNic = 2,

    /// <summary>
    /// A nic network.
    /// </summary>
    Nic = 3
}
