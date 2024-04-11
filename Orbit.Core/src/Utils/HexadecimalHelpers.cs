using System.Globalization;

namespace Orbit.Utils;

/// <summary>
/// Methods to help with hexadecimal values.
/// </summary>
public static class HexadecimalHelpers
{
    /// <summary>
    /// Convert a hexadecimal string to a ushort.
    /// </summary>
    /// <param name="hexadecimal"></param>
    /// <returns>A <see cref="ushort"/> or <see langword="null"/> if the hexadecimal is invalid.</returns>
    public static ushort? ToUShort(string hexadecimal)
    {
        if(hexadecimal.Length > 4)
            return null;
        if(!int.TryParse(hexadecimal, NumberStyles.HexNumber, null, out int hexAsInt))
            return null;
        ushort hexAsUShort = (ushort)hexAsInt;
        return hexAsUShort;
    }
}
