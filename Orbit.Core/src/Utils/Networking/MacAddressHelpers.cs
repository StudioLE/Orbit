namespace Orbit.Utils.Networking;

/// <summary>
/// Methods to help with MacAddress.
/// </summary>
public static class MacAddressHelpers
{
    /// <summary>
    /// Generate a deterministic MAC address from seed values.
    /// </summary>
    /// <param name="type">The context seed.</param>
    /// <param name="server">The server number.</param>
    /// <param name="entity">The entity number.</param>
    /// <returns>A MAC Address.</returns>
    public static MacAddress Generate(int type, int server, int entity)
    {
        byte a = (byte)type;
        byte b = (byte)server;
        byte c = (byte)entity;
        return new([0x2, 0x0, 0x0, a, b, c]);
    }

    /// <summary>
    /// Generate a deterministic MAC address from seed values.
    /// </summary>
    /// <param name="type">The context seed.</param>
    /// <param name="server">The server number.</param>
    /// <param name="entity">The entity number.</param>
    /// <returns>A MAC Address.</returns>
    public static MacAddress GenerateRandom(int type, int server, int entity)
    {
        int seed = type * 1_000_000
                   + server * 1000
                   + entity;
        return Generate(seed);
    }

    private static MacAddress Generate(int seed)
    {
        byte[] bytes = new byte[6];
        Random random = new(seed);
        random.NextBytes(bytes);
        bytes[0] = (byte)(bytes[0] & 0xFC); // zeroing last 2 bits to make it unicast and locally administered
        return new(bytes);
    }
}
