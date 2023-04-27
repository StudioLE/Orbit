using System.ComponentModel.DataAnnotations;
using Orbit.Core.Utils.DataAnnotations;

namespace Orbit.Core.Schema;

public class SshConnection : IHasValidationAttributes
{
    [Required]
    [Range(0, 65536)]
    public int Port { get; set; } = 22;

    [Required]
    public string User { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PrivateKeyFile { get; set; } = string.Empty;
}
