using Orbit.Core.Schema;

namespace Orbit.Core.Provision;

public static class InstanceProviderHelpers
{
    public static bool Put(this IEntityProvider<Instance> provider, Instance instance)
    {
        return provider.Put(new InstanceId(instance.Name), instance);
    }

    public static IEnumerable<Instance> GetAll(this IEntityProvider<Instance> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new InstanceId(x)))
            .OfType<Instance>();
    }
}
