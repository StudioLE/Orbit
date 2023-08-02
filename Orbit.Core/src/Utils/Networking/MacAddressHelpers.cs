using StudioLE.Core.System;

namespace Orbit.Core.Utils.Networking;

public static class MacAddressHelpers
{
    public static string Generate()
    {
        byte[] bytes = new byte[6];
        Random random = new();
        random.NextBytes(bytes);
        bytes[0] = (byte)(bytes[0] & 0xFC); // zeroing last 2 bits to make it unicast and locally administered
        return bytes
            .Select(b => b.ToString("X2"))
            .Join(":");
    }
}
