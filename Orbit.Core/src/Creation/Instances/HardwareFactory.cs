using Orbit.Schema;
using Orbit.Utils;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Creation.Instances;

/// <summary>
/// A factory for creating <see cref="Hardware"/> with default values.
/// </summary>
public class HardwareFactory : IFactory<Hardware, Hardware>
{
    private const string DefaultType = "G2";
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
        string cpus = hardware.Type.Substring(1);
        return int.Parse(cpus);
    }

    private static int DefaultMemory(Hardware hardware)
    {
        char category = hardware.Type.First();
        double multiplier = GetTypeMultiplier(category);
        double memory = hardware.Cpus * multiplier;
        return memory.CeilingToInt();
    }

    private static double GetTypeMultiplier(char category)
    {
        return category switch
        {
            'C' => 0.5,
            'G' => 1,
            'M' => 2,
            _ => throw new("Invalid type")
        };
    }
}
