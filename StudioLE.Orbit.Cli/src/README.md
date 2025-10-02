## About

A CLI tool providing infrastructure-as-code and git-ops methods for creating and managing virtual machines with [LXD](https://canonical.com/lxd).

## How to Use

Install as a [.NET Core Global Tool](https://learn.microsoft.com/en-us/dotnet/core/tools/global-tools-how-to-use):

```bash
dotnet tool install StudioLE.Orbit --global --prerelease
```

Run the tool

```bash
Orbit --help
```

Refer to [launchsettings.json](Properties/launchSettings.json) for examples of executing each method.

## How it Works

Each CLI command is defined in the [Orbit.Core library](../../StudioLE.Orbit.Core/src) as a [Tectonic activity](https://github.com/StudioLE/Tectonic/tree/main/Tectonic.Abstractions/src), the [Program.cs](Program.cs) registers each command to be called be executed with [Tectonic.Extensions.Cli](https://github.com/StudioLE/Tectonic/tree/main/Tectonic.Extensions.CommandLine/src).
