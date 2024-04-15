using Orbit.Schema;

namespace Orbit.Provision;

// TODO: Replace with InstanceProvider
/// <summary>
///
/// </summary>
public static class InstanceProviderHelpers
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <param name="instance"></param>
    /// <returns></returns>
    public static bool Put(this IEntityProvider<Instance> provider, Instance instance)
    {
        return provider.Put(instance.Name, instance);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public static IEnumerable<Instance> GetAll(this IEntityProvider<Instance> provider)
    {
        return provider.GetIndex()
            .Select(x => provider.Get(new InstanceId(x)))
            .OfType<Instance>();
    }
}
