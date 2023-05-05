using Orbit.Core.Schema;
using Renci.SshNet;

namespace Orbit.Core.Shell;

public static class ConnectionHelpers
{
    public static ConnectionInfo CreateConnection(this Server server)
    {
        List<AuthenticationMethod> auth = new();
        if (!string.IsNullOrWhiteSpace(server.Ssh.Password))
            auth.Add(new PasswordAuthenticationMethod(server.Ssh.User, server.Ssh.Password));
        if (!string.IsNullOrWhiteSpace(server.Ssh.PrivateKeyFile))
            auth.Add(new PrivateKeyAuthenticationMethod(server.Ssh.User, new PrivateKeyFile(server.Ssh.PrivateKeyFile)));
        return new(server.Address, server.Ssh.Port, server.Ssh.User, auth.ToArray());
    }
}
