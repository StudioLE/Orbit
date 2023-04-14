using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;
using Orbit.Core.Utils;

namespace Orbit.Core.Schema;

public class Hardware
{
    private const string DefaultType = "G1";
    private const Platform DefaultPlatform = Platform.Virtual;
    private const int DefaultDisk = 20;

    [EnumDataType(typeof(Platform))]
    public Platform Platform { get; set; }

    [TypeSchema]
    public string Type { get; set; } = string.Empty;

    [Range(1, 64)]
    public int Cpus { get; set; }

    [Range(1, 32)]
    public int Memory { get; set; }

    [Range(10, 1024)]
    public int Disk { get; set; }

    public void Review()
    {
        if (Platform == default)
            Platform = DefaultPlatform;

        if (Disk == default)
            Disk = DefaultDisk;

        if (Type.IsNullOrEmpty())
            Type = DefaultType;

        if (Cpus == default)
        {
            string cpusStr = Type.Substring(1);
            Cpus = int.Parse(cpusStr);
        }

        if (Memory == default)
        {
            char category = Type.First();
            int multiplier = GetTypeMultiplier(category);
            Memory = Cpus * multiplier;
        }
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
