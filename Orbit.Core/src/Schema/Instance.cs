using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;
using YamlDotNet.Serialization;

namespace Orbit.Core.Schema;

public sealed class Instance : IEntity, IHasValidationAttributes
{
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    [Range(1, 64)]
    public int Number { get; set; }

    [NameSchema]
    public string Role { get; set; } = string.Empty;

    [NameSchema]
    public string Cluster { get; set; } = string.Empty;

    [Required]
    [NameSchema]
    public string Server { get; set; } = string.Empty;

    [ValidateComplexType]
    public Network Network { get; set; } = new();

    // ReSharper disable once InconsistentNaming
    [YamlMember(Alias = "os")]
    [ValidateComplexType]
    public OS OS { get; set; } = new();

    [ValidateComplexType]
    public Hardware Hardware { get; set; } = new();

    [ValidateComplexType]
    [YamlMember(Alias = "wireguard")]
    public WireGuard WireGuard { get; set; } = new();
}
