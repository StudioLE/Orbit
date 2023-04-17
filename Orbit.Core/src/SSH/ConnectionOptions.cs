using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Orbit.Core.Utils.DataAnnotations;
using Renci.SshNet;
using StudioLE.Core.System;

namespace Orbit.Core.SSH;

public class ConnectionOptions : IHasValidationAttributes
{
    private const string MarkerKey = "Host";

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(0, 65536)]
    public int Port { get; set; } = 22;

    [Required]
    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string PrivateKeyFile { get; set; } = string.Empty;

    public ConnectionOptions()
    {
    }

    public ConnectionOptions(IConfiguration configuration)
    {
        configuration.GetSection(MarkerKey).Bind(this);
        if(!this.TryValidate(out IReadOnlyCollection<string> errors))
            throw new(errors.Join());
    }

    public ConnectionInfo CreateConnection()
    {
        List<AuthenticationMethod> auth = new();
        if (!string.IsNullOrWhiteSpace(Password))
            auth.Add(new PasswordAuthenticationMethod(Username, Password));
        if (!string.IsNullOrWhiteSpace(PrivateKeyFile))
            auth.Add(new PrivateKeyAuthenticationMethod(Username, new PrivateKeyFile(PrivateKeyFile)));
        return new(Address, Port, Username, auth.ToArray());
    }
}
