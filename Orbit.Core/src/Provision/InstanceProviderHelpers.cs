using Orbit.Schema;

namespace Orbit.Provision;

public static class InstanceProviderHelpers
{
    public static bool Put(this IEntityProvider<Instance> provider, Instance instance)
    {
        return provider.Put(instance.Name, instance);
    }

    public static IEnumerable<Instance> GetAll(this IEntityProvider<Instance> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new InstanceId(x)))
            .OfType<Instance>();
    }
}
