using Orbit.Provision;
using Orbit.Schema;
using StudioLE.Extensions.System;
using StudioLE.Patterns;

namespace Orbit.Generation;

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
