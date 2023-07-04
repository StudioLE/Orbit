using Orbit.Core.Shell;

namespace Orbit.Core.Tests.Resources;

public class MockWireGuardFacade : IWireGuardFacade
{
    public const string PrivateKey = "8Dh1P7/6fm9C/wHYzDrEhnyKmFgzL6yH6WuslXPLbVQ=";
    public const string PublicKey = "Rc9kAH9gclSHur2vbbmIj3pvWizuxB5ly1Drv0tRXRE=";
    public const string ExternalIPv4 = "203.0.113.1";
    public const string ExternalIPv6 = "2001:db8::";
    public const int Port = 51820;

    public string GeneratePrivateKey()
    {
        return PrivateKey;
    }

    public string GeneratePublicKey(string privateKey)
    {
        if (privateKey != PrivateKey)
            throw new("Expected the private key: " + PrivateKey);
        return PublicKey;
    }
}
