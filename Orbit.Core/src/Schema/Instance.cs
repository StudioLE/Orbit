using System.ComponentModel.DataAnnotations;
using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;
using YamlDotNet.Serialization;

namespace Orbit.Core.Schema;

public sealed class Instance : IEntity, IHasValidationAttributes
{
    private const int DefaultInstanceNumber = 1;
    private const string DefaultRole = "node";

    [NameSchema]
    public string Name { get; set; } = string.Empty;

    [Range(1,64)]
    public int Number { get; set; }

    [NameSchema]
    public string Role { get; set; } = string.Empty;

    [NameSchema]
    public string Cluster { get; set; } = string.Empty;

    [Required]
    [NameSchema]
    public string Host { get; set; } = string.Empty;

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

    public void Review(EntityProvider provider)
    {
        if (Host.IsNullOrEmpty())
            Host = provider
                       .Host
                       .GetAllNames()
                       .FirstOrDefault()
                   ?? throw new("Host must be set if more than one exist.");


        if (Cluster.IsNullOrEmpty())
            throw new Exception("Cluster not set");

        WireGuard.Review();
        Hardware.Review();
        OS.Review();

        if (Number == default)
        {
            int[] numbers = provider
                .Instance
                .GetAllInCluster(Cluster)
                .Select(x => x.Number)
                .ToArray();
            int finalNumber = numbers.Any()
                ? numbers.Max()
                : DefaultInstanceNumber - 1;
            Number = finalNumber + 1;
        }

        if (Role.IsNullOrEmpty())
            Role = DefaultRole;

        if (Name.IsNullOrEmpty())
            Name = $"{Cluster}-{Number:00}";

        Network.Review(this, provider);
    }
}
