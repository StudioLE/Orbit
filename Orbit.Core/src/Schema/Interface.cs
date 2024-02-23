using Orbit.Schema.DataAnnotations;
using Orbit.Utils.DataAnnotations;

namespace Orbit.Schema;

/// <summary>
/// The schema for the network interface configuration of an <see cref="Instance"/>.
/// </summary>
public readonly record struct Interface() : IHasValidationAttributes
{
    /// <summary>
    /// The name of the interface.
    /// </summary>
    [NameSchema]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The name of the server the interface connects to.
    /// </summary>
    [NameSchema]
    public string Server { get; init; } = string.Empty;

    /// <summary>
    /// The type of the network.
    /// </summary>
    public NetworkType Type { get; init; } = NetworkType.Unknown;

    /// <summary>
    /// The mac address of the interface.
    /// </summary>
    public string MacAddress { get; init; } = string.Empty;

    /// <summary>
    /// The IPv4 or IPv6 addresses of the interface.
    /// </summary>
    public string[] Addresses { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The gateway address of the interface.
    /// </summary>
    public string[] Gateways { get; init; } = Array.Empty<string>();


    /// <summary>
    /// The subnets of the interface.
    /// </summary>
    public string[] Subnets { get; init; } = Array.Empty<string>();

    /// <summary>
    /// The dns resolver addresses of the interface.
    /// </summary>
    public string[] Dns { get; init; } = Array.Empty<string>();
}

public enum NetworkType
{
    Unknown,
    Bridge,
    WireGuard,
    RoutedNic,
    Nic
}
