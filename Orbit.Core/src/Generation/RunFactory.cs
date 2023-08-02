using Orbit.Core.Provision;
using Orbit.Core.Schema;
using StudioLE.Core.Patterns;
using StudioLE.Core.System;

namespace Orbit.Core.Generation;

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
                curl -fsS https://install.studiole.uk/src/{package} | sudo bash

                """;
    }
}
