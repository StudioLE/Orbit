using StudioLE.Orbit.Provision;
using StudioLE.Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace StudioLE.Orbit.CloudInit;

/// <summary>
/// Create a bash script to install the packages <see cref="Instance.Install"/> on the <see cref="Instance"/>.
/// </summary>
public class InstallFactory : IFactory<Instance, string>
{
    /// <inheritdoc/>
    public string Create(Instance instance)
    {
        string template = EmbeddedResourceHelpers.GetText("Resources/Templates/bash-template");
        return instance
            .Install
            .Select(Create)
            .Prepend(template)
            .Join();
    }

    private static string Create(string package)
    {
        return $"""

            log "Installing {package}"
            curl -fsS https://install.studiole.uk/{package} | sudo bash
            """;
    }
}
