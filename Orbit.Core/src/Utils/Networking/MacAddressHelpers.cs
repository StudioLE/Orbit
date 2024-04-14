using StudioLE.Extensions.System;

namespace Orbit.Utils.Networking;

public static class MacAddressHelpers
{
    public static string Generate(int type, int server, int entity)
    {
        int seed = type * 1_000_000
                   + server * 1000
                   + entity;
        return Generate(seed);
    }

    private static string Generate(int seed)
    {
        byte[] bytes = new byte[6];
        Random random = new(seed);
        random.NextBytes(bytes);
        bytes[0] = (byte)(bytes[0] & 0xFC); // zeroing last 2 bits to make it unicast and locally administered
        return bytes
            .Select(b => b.ToString("X2"))
            .Join(":");
    }
}
