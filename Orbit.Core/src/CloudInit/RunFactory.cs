using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.CloudInit;

/// <summary>
/// Create a bash script to run the packages <see cref="Instance.Run"/> on the <see cref="Instance"/>.
/// </summary>
public class RunFactory : IFactory<Instance, string>
{
    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        string template = EmbeddedResourceHelpers.GetText("Resources/Templates/bash-template");
        return instance
            .Run
            .Select(x => Create(x, instance))
            .Prepend(template)
            .Join();
    }

    private static string Create(string package, Instance instance)
    {
        return instance
            .Install
            .Contains(package)
            ? $"""

            log "Running local {package}"
            {package}
            """
            : $"""

            log "Running remote {package}"
            curl -fsS https://bash.studiole.uk/{package} | sudo bash
            """;
    }
}
