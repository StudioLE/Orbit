using Cascade.Workflows;
using Cascade.Workflows.CommandLine;
using Microsoft.Extensions.Logging;
using Orbit.Schema.DataAnnotations;

namespace Orbit.Generation;

/// <summary>
/// An <see cref="IActivity"/> to generate the server configuration for an instance or client.
/// </summary>
public class GenerateServerConfiguration : IActivity<GenerateServerConfiguration.Inputs, GenerateServerConfiguration.Outputs>
{
    public const string FileName = "server-config.yml";
    private readonly ILogger<GenerateServerConfiguration> _logger;
    private readonly GenerateServerConfigurationForInstance _forInstance;
    private readonly GenerateServerConfigurationForClient _forClient;
    private readonly CommandContext _context;

    /// <summary>
    /// DI constructor for <see cref="GenerateServerConfiguration"/>.
    /// </summary>
    public GenerateServerConfiguration(
        ILogger<GenerateServerConfiguration> logger,
        GenerateServerConfigurationForInstance forInstance,
        GenerateServerConfigurationForClient forClient,
        CommandContext context)
    {
        _logger = logger;
        _forInstance = forInstance;
        _forClient = forClient;
        _context = context;
    }

    /// <summary>
    /// The inputs for <see cref="GenerateServerConfiguration"/>.
    /// </summary>
    public class Inputs
    {
        /// <summary>
        /// The name of the instance to generate from.
        /// </summary>
        [NameSchema]
        public string Instance { get; set; } = string.Empty;

        /// <summary>
        /// The name of the client to generate from.
        /// </summary>
        [NameSchema]
        public string Client { get; set; } = string.Empty;
    }

    /// <summary>
    /// The outputs of <see cref="GenerateServerConfiguration"/>.
    /// </summary>
    public class Outputs
    {
    }

    /// <inheritdoc/>
    public Task<Outputs> Execute(Inputs inputs)
    {
        if (!string.IsNullOrEmpty(inputs.Instance) && !string.IsNullOrEmpty(inputs.Client))
        {
            _logger.LogError("Cannot generate server configuration for both an instance and a client.");
            _context.ExitCode = 1;
            return Task.FromResult(new Outputs());
        }
        if (!string.IsNullOrEmpty(inputs.Instance))
        {
            GenerateServerConfigurationForInstance.Inputs forInstanceInputs = new()
            {
                Instance = inputs.Instance
            };
            return _forInstance.Execute(forInstanceInputs);
        }
        if (!string.IsNullOrEmpty(inputs.Client))
        {
            GenerateServerConfigurationForClient.Inputs forClientInputs = new()
            {
                Client = inputs.Client
            };
            return _forClient.Execute(forClientInputs);
        }
        _logger.LogError("Failed to generate server configuration");
        _context.ExitCode = 1;
        return Task.FromResult(new Outputs());
    }
}
