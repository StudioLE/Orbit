using System.Reflection;
using Microsoft.Extensions.FileProviders;
using YamlDotNet.RepresentationModel;

namespace Orbit.Core;

public class EmbeddedResourceHelpers
{
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

    public static string GetText(string path)
    {
        IFileInfo templateFile = GetFile(path);
        using Stream readStream = templateFile.CreateReadStream();
        using StreamReader reader = new(readStream);
        return reader.ReadToEnd();
    }

    private static IFileInfo GetFile(string path)
    {
        Assembly assembly = typeof(EmbeddedResourceHelpers).Assembly;
        EmbeddedFileProvider provider = new(assembly);
        IFileInfo file = provider.GetFileInfo(path);
        if (!file.Exists)
            throw new($"EmbeddedResource does not exist: {path}");
        return file;
    }
}
