namespace Orbit.Core.Shell;

/// <summary>
/// A facade to access the WireGuard CLI.
/// </summary>
public interface IWireGuardFacade
{
    /// <summary>
    /// Generate a new private key.
    /// </summary>
    string? GeneratePrivateKey();

    /// <summary>
    /// Generate a public key for the specified <paramref name="privateKey"/>.
    /// </summary>
    string? GeneratePublicKey(string privateKey);

    /// <summary>
    /// Generate a new pre-shared key.
    /// </summary>
    string? GeneratePreSharedKey();
}
