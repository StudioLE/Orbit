using Microsoft.Extensions.Configuration;

namespace Orbit.Cli.Shell;

public class HostShellOptions
{
    public const string MarkerKey = "Host";

    public string Address { get; set; } = string.Empty;

    public int Port { get; set; } = 22;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PrivateKeyPath { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Validate()
    {
        List<string> errors = new();
        if (string.IsNullOrWhiteSpace(Address))
            errors.Add("Host must be specified.");
        if (Port is < 0 or > 65536)
            errors.Add("Port must be specified.");
        if (string.IsNullOrWhiteSpace(Username))
            errors.Add("Username must be specified.");
        if (string.IsNullOrWhiteSpace(Password) && string.IsNullOrWhiteSpace(PrivateKeyPath))
            errors.Add("Password or private key path must be specified.");
        return errors;
    }

    public HostShellOptions()
    {
    }

    public HostShellOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
    }
}
