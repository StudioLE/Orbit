namespace Orbit.Shell;

/// <summary>
/// A facade to access the QREncode CLI.
/// </summary>
// ReSharper disable once InconsistentNaming
public interface IQREncodeFacade
{
    /// <summary>
    /// Generate an Svg
    /// </summary>
    public string GenerateSvg(string source);
}
