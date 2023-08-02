using Orbit.Core.Shell;

namespace Orbit.Core.Tests.Resources;

public class MockWireGuardFacade : IWireGuardFacade
{
    public string GeneratePrivateKey()
    {
        return MockConstants.PrivateKey;
    }

    public string GeneratePublicKey(string privateKey)
    {
        if (privateKey != MockConstants.PrivateKey)
            throw new("Expected the private key: " + MockConstants.PrivateKey);
        return MockConstants.PublicKey;
    }

    /// <inheritdoc />
    public string GeneratePreSharedKey()
    {
        return MockConstants.PreSharedKey;
    }
}
