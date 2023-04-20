using System.ComponentModel.DataAnnotations;
using Orbit.Core.Providers;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

public interface IEntity
{
    [NameSchema]
    public string Name { get; }

    [Range(1,64)]
    public int Number { get; }

    public void Review(EntityProvider provider);
}
