using FileInfo = StudioLE.Storage.Files.FileInfo;

namespace Orbit.Assets;

/// <summary>
/// Methods to help with assets.
/// </summary>
public static class AssetHelpers
{
    /// <summary>
    /// Create a data asset.
    /// </summary>
    /// <returns>
    /// A <see cref="FileInfo"/> representing the data asset.
    /// </returns>
    public static FileInfo CreateFromYaml(string yaml)
    {
        return new()
        {
            MediaType = "text/x-yaml",
            // TODO: Convert output to data URI
            Location = yaml
        };
    }

    /// <summary>
    /// Create a data asset.
    /// </summary>
    /// <returns>
    /// A <see cref="FileInfo"/> representing the data asset.
    /// </returns>
    public static FileInfo Create(string fileName, string content, string mediaType = "text/plain")
    {
        return new()
        {
            Name = fileName,
            MediaType = mediaType,
            // TODO: Convert output to data URI
            Location = content
        };
    }

}
