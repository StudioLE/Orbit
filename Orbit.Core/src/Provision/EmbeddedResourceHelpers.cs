using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace Orbit.Provision;

/// <summary>
/// Methods to help with <see cref="EmbeddedFileProvider"/>.
/// </summary>
public static class EmbeddedResourceHelpers
{
    /// <summary>
    /// Get the context of an embedded text file.
    /// </summary>
    public static string GetText(string path)
    {
        IFileInfo templateFile = GetFile(path);
        using Stream readStream = templateFile.CreateReadStream();
        using StreamReader reader = new(readStream);
        return reader
            .ReadToEnd()
            .TrimEnd('\n');
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
