using System.Reflection;
using Microsoft.Extensions.FileProviders;
using YamlDotNet.RepresentationModel;

namespace Orbit.Provision;

/// <summary>
/// Methods to help with <see cref="EmbeddedFileProvider"/>.
/// </summary>
public static class EmbeddedResourceHelpers
{
    /// <summary>
    /// Get the content of an embedded yaml file as a <see cref="YamlNode"/>.
    /// </summary>
    public static YamlNode GetYaml(string path)
    {
        IFileInfo templateFile = GetFile(path);
        using Stream readStream = templateFile.CreateReadStream();
        using StreamReader reader = new(readStream);
        YamlStream yamlStream = new();
        yamlStream.Load(reader);
        YamlDocument document = yamlStream.Documents.First();
        return document.RootNode;
    }

    /// <summary>
    /// Get the context of an embedded text file.
    /// </summary>
    public static string GetText(string path)
    {
        IFileInfo templateFile = GetFile(path);
        using Stream readStream = templateFile.CreateReadStream();
        using StreamReader reader = new(readStream);
        return reader.ReadToEnd();
    }

    /// <summary>
    /// Get the embedded resource file.
    /// </summary>
    private static IFileInfo GetFile(string path)
    {
        Assembly assembly = typeof(EmbeddedResourceHelpers).Assembly;
        EmbeddedFileProvider provider = new(assembly, "Orbit");
        IFileInfo file = provider.GetFileInfo(path);
        if (!file.Exists)
            throw new($"EmbeddedResource does not exist: {path}");
        return file;
    }
}
