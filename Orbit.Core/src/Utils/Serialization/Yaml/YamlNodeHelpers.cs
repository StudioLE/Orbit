using YamlDotNet.RepresentationModel;

namespace Orbit.Core.Utils.Serialization.Yaml;

/// <summary>
/// Methods to help with <see cref="YamlNode"/>.
/// </summary>
public static class YamlNodeHelpers
{
    /// <summary>
    /// Set the value of a <see cref="YamlMappingNode"/>.
    /// </summary>
    public static void SetValue(this YamlMappingNode yaml, string key, YamlNode value, bool overwrite = true)
    {
        if (!yaml.Children.ContainsKey(key))
        {
            yaml.Add(key, value);
            return;
        }
        if (overwrite)
            return;
        yaml.Children[key] = value;
    }

    /// <summary>
    /// Get the value of a <see cref="YamlMappingNode"/>.
    /// </summary>
    public static T? GetValue<T>(this YamlMappingNode yaml, string key)
        where T : YamlNode
    {
        if (yaml.Children.TryGetValue(key, out YamlNode value))
            return value as T;
        return null;
    }

    /// <summary>
    /// Add a range of values to YamlSequenceNode.
    /// </summary>
    public static void AddRange(this YamlSequenceNode yaml, IEnumerable<YamlNode> nodes)
    {
        foreach (YamlNode node in nodes)
            yaml.Add(node);
    }
}
