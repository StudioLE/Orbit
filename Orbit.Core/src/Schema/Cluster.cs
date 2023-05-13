using System.ComponentModel.DataAnnotations;
using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

/// <summary>
/// The schema for a cluster of <see cref="Instance"/>.
/// </summary>
public sealed class Cluster : IEntity, IHasValidationAttributes
{
    private const string DefaultName = "cluster";
    private const int DefaultNumber = 1;

    /// <summary>
    /// The name of the cluster.
    /// </summary>
    [NameSchema]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The number of the cluster.
    /// </summary>
    [Range(1,64)]
    public int Number { get; set; }

    public void Review(EntityProvider provider)
    {
        // TODO: Replace with ClusterFactory
        if (!Name.IsNullOrEmpty())
        {
            Cluster? cluster = provider.Cluster.Get(Name);
            if (cluster is not null)
                Number = cluster.Number;
        }

        if (Number == default)
        {
            int[] numbers = provider
                .Cluster
                .GetAll()
                .Select(x => x.Number)
                .ToArray();
            int finalNumber = numbers.Any()
                    ? numbers.Max()
                    : DefaultNumber - 1;
            Number = finalNumber + 1;
        }

        // TODO: GetName from cluster file!
        if (Name.IsNullOrEmpty())
            Name = $"{DefaultName}-{Number:00}";
    }
}
