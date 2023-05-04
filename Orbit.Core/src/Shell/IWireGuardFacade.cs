namespace Orbit.Core.Shell;

public interface IWireGuardFacade
{
    string? GeneratePrivateKey();

    string? GeneratePublicKey(string privateKey);
}
