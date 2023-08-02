using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

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
