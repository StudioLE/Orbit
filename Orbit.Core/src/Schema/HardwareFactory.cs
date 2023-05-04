using Orbit.Core.Utils;
using StudioLE.Core.Patterns;

namespace Orbit.Core.Schema;

public class HardwareFactory : IFactory<Hardware, Hardware>
{
    private const string DefaultType = "G1";
    private const Platform DefaultPlatform = Platform.Virtual;
    private const int DefaultDisk = 20;

    /// <inheritdoc/>
    public Hardware Create(Hardware source)
    {
        Hardware result = new();

        result.Platform = source.Platform == default
            ? DefaultPlatform
            : source.Platform;

        result.Disk = source.Disk == default
            ? DefaultDisk
            : source.Disk;

        result.Type = source.Type.IsNullOrEmpty()
            ? DefaultType
            : source.Type;

        result.Cpus = source.Cpus == default
            ? DefaultCpus(result)
            : source.Cpus;

        result.Memory = source.Memory == default
            ? DefaultMemory(result)
            : source.Memory;

        return result;
    }

    private static int DefaultCpus(Hardware hardware)
    {
        string cpusStr = hardware.Type.Substring(1);
        return int.Parse(cpusStr);
    }

    private static int DefaultMemory(Hardware hardware)
    {
        char category = hardware.Type.First();
        int multiplier = GetTypeMultiplier(category);
        return hardware.Cpus * multiplier;
    }

    private static int GetTypeMultiplier(char category)
    {
        return category switch
        {
            'C' => 2,
            'G' => 4,
            'M' => 8,
            _ => throw new("Invalid type")
        };
    }
}
