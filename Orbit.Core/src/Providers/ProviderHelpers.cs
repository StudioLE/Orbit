using Microsoft.Extensions.FileProviders;
using YamlDotNet.Serialization;

namespace Orbit.Core.Providers;

/// <summary>
/// Methods to help with <see cref="IFileInfo"/>.
/// </summary>
public static class ProviderHelpers
{
    /// <summary>
    /// Deserialize <typeparamref name="T"/> from a YAML file.
    /// </summary>
    /// <returns>Returns the deserialized <typeparamref name="T"/> or <see langword="null"/>.</returns>
    public static T? ReadYamlFileAs<T>(this IFileInfo file) where T : class
    {
        if (!file.Exists)
            return null;
        Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        IDeserializer deserializer = Yaml.Deserializer();
        return deserializer.Deserialize<T>(reader);
    }

    /// <summary>
    /// Read a text file.
    /// </summary>
    /// <returns>The file content or <see langword="null"/>.</returns>
    public static string? ReadTextFile(this IFileInfo file)
    {
        if (!file.Exists)
            return null;
        Stream stream = file.CreateReadStream();
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Write <paramref name="obj"/> as YAML to the file.
    /// </summary>
    /// <returns><see langword="true"/> if successful.</returns>
    public static bool WriteYamlFile(this IFileInfo file, object obj, string? prefixYaml = null)
    {
        if(file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        if (!string.IsNullOrEmpty(prefixYaml))
            writer.WriteLine(prefixYaml);
        ISerializer serializer = Yaml.Serializer();
        serializer.Serialize(writer, obj);
        return true;
    }


    /// <summary>
    /// Write <paramref name="content"/> to the file.
    /// </summary>
    /// <returns><see langword="true"/> if successful.</returns>
    public static bool WriteTextFile(this IFileInfo file, string content)
    {
        if(file.Exists)
            return false;
        FileInfo physicalFile = new(file.PhysicalPath ?? throw new("Not a physical path"));
        DirectoryInfo directory = physicalFile.Directory ?? throw new("Directory was unexpectedly null.");
        if(!directory.Exists)
            directory.Create();
        if(file.Exists)
            return false;
        using StreamWriter writer = physicalFile.CreateText();
        writer.Write(content);
        return true;
    }
}
