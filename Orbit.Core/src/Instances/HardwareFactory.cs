using Orbit.Schema;
using Orbit.Utils;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Instances;

/// <summary>
/// A factory for creating <see cref="Hardware"/> with default values.
/// </summary>
public class HardwareFactory : IFactory<Hardware, Hardware>
{
    private const string DefaultType = "G2";
    private const Platform DefaultPlatform = Platform.Virtual;
    private const int DefaultDisk = 20;

    /// <inheritdoc/>
    public Hardware Create(Hardware hardware)
    {
        if (hardware.Platform.IsDefault())
            hardware.Platform = DefaultPlatform;
        if (hardware.Disk.IsDefault())
            hardware.Disk = DefaultDisk;
        if (hardware.Type.IsDefault())
            hardware.Type = DefaultType;
        if (hardware.Cpus.IsDefault())
            hardware.Cpus = DefaultCpus(hardware);
        if (hardware.Memory.IsDefault())
            hardware.Memory = DefaultMemory(hardware);
        return hardware;
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
