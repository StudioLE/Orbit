using System.ComponentModel.DataAnnotations;
using Orbit.Core.Schema.DataAnnotations;

namespace Orbit.Core.Schema;

public class Hardware
{
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
}
