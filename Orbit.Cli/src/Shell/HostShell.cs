using Microsoft.Extensions.Logging;
using Renci.SshNet;

namespace Orbit.Cli.Shell;

public class HostShell : IDisposable
{
    private readonly ILogger<HostShell> _logger;
    private readonly SshClient _ssh;

    public HostShell(ILogger<HostShell> logger, HostShellOptions options)
    {
        _logger = logger;
        _ssh = CreateClient(options);
    }

    public void Connect()
    {
        _logger.LogInformation($"{nameof(Connect)}() called.");
         _ssh.Connect();
    }

    public SshCommand Execute(string command, params string[] arguments)
    {
        string[] components = Array.Empty<string>()
            .Prepend(command)
            .Concat(arguments.Select(argument => $"\"{argument}\""))
            .ToArray();
        string exec = string.Join(' ', components);
        return _ssh.RunCommand(exec);
    }

    public void Disconnect()
    {
        _ssh.Disconnect();
    }

    public void Test()
    {
        SshCommand cmd = Execute("ls /root");
        _logger.LogInformation("Exit Status: {exitStatus}", cmd.ExitStatus);
        _logger.LogInformation("Output: {exitStatus}", cmd.Result);
        _logger.LogInformation("Error: {exitStatus}", cmd.Error);
    }

    public void Dispose()
    {
        _ssh.Dispose();
    }

    private SshClient CreateClient(HostShellOptions options)
    {
        ConnectionInfo connection = CreateConnection(_logger, options);
        return new(connection);
    }

    private static ConnectionInfo CreateConnection(ILogger<HostShell> logger, HostShellOptions options)
    {
        IReadOnlyCollection<string> errors = options.Validate();
        if (errors.Any())
        {
            logger.LogError(string.Join(Environment.NewLine, errors));
            throw new("Invalid shell configuration");
        }
        List<AuthenticationMethod> auth = new();
        if (!string.IsNullOrWhiteSpace(options.Password))
            auth.Add(new PasswordAuthenticationMethod(options.Username, options.Password));
        if (!string.IsNullOrWhiteSpace(options.PrivateKeyPath)) {}
        auth.Add(new PrivateKeyAuthenticationMethod(options.Username, new []{ new PrivateKeyFile(options.PrivateKeyPath) }));
        return new(options.Address, options.Port, options.Username, auth.ToArray());
    }
}
