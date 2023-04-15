using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using YamlDotNet.Serialization;

namespace Orbit.Core.Schema;

public sealed class Instance
{
    private const int DefaultInstanceNumber = 1;
    private const int DefaultClusterNumber = 1;
    private const int DefaultHost = 1;
    private const string DefaultRole = "node";

    [NameSchema]
    public string Name { get; set; } = string.Empty;

    [IdSchema]
    public string Id => $"{Cluster:00}-{Number:00}";

    [Range(1,64)]
    public int Number { get; set; }

    [Range(1,64)]
    public int Cluster { get; set; }

    [Range(1,64)]
    public int Host { get; set; }

    [NameSchema]
    public string Role { get; set; } = string.Empty;

    [ValidateComplexType]
    public Network Network { get; set; } = new();

    // ReSharper disable once InconsistentNaming
    [YamlMember(Alias = "os")]
    [ValidateComplexType]
    public OS OS { get; set; } = new();

    [ValidateComplexType]
    public Hardware Hardware { get; set; } = new();

    public void Review()
    {
        Hardware.Review();
        OS.Review();

        if (Host == default)
            Host = DefaultHost;

        if (Cluster == default)
        {
            int[] clusters = InstanceApi
                .GetIds()
                .Select(IdHelpers.GetClusterNumber)
                .ToArray();
            int lastClusterNumber= clusters.Any()
                    ? clusters.Max()
                    : DefaultClusterNumber - 1;
            Cluster = lastClusterNumber + 1;
        }

        if (Number == default)
        {
            int[] instances = InstanceApi
                .GetIds()
                .Where(id => IdHelpers.GetClusterNumber(id) == Cluster)
                .Select(IdHelpers.GetInstanceNumber)
                .ToArray();
            int lastInstanceNumber = instances.Any()
                ? instances.Max()
                : DefaultInstanceNumber - 1;
            Number = lastInstanceNumber + 1;
        }

        if (Role.IsNullOrEmpty())
            Role = DefaultRole;

        if (Name.IsNullOrEmpty())
            Name = Id;

        Network.Review(this);
    }
}
