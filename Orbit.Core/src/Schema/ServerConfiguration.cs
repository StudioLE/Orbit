using System.Collections;

namespace Orbit.Schema;

/// <summary>
/// A set of shell commands to configure a <see cref="Server"/>.
/// </summary>
public readonly record struct ServerConfiguration : IReadOnlyDictionary<string, ShellCommand[]>
{
    private readonly IReadOnlyDictionary<string, ShellCommand[]> _readOnlyDictionaryImplementation;

    /// <summary>
    /// Create a new instance of <see cref="ServerConfiguration"/>.
    /// </summary>
    public ServerConfiguration(IReadOnlyDictionary<string, ShellCommand[]> readOnlyDictionaryImplementation)
    {
        _readOnlyDictionaryImplementation = readOnlyDictionaryImplementation;
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<string, ShellCommand[]>> GetEnumerator()
    {
        return _readOnlyDictionaryImplementation.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_readOnlyDictionaryImplementation).GetEnumerator();
    }

    /// <inheritdoc />
    public int Count => _readOnlyDictionaryImplementation.Count;

    /// <inheritdoc />
    public bool ContainsKey(string key)
    {
        return _readOnlyDictionaryImplementation.ContainsKey(key);
    }

    /// <inheritdoc />
    public bool TryGetValue(string key, out ShellCommand[] value)
    {
        return _readOnlyDictionaryImplementation.TryGetValue(key, out value!);
    }

    /// <inheritdoc />
    public ShellCommand[] this[string key] => _readOnlyDictionaryImplementation[key];

    /// <inheritdoc />
    public IEnumerable<string> Keys => _readOnlyDictionaryImplementation.Keys;

    /// <inheritdoc />
    public IEnumerable<ShellCommand[]> Values => _readOnlyDictionaryImplementation.Values;
}
