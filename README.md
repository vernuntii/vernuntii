![Vernuntii Logo](res/logo.svg)
<!-- [![Nuget](https://img.shields.io/nuget/v/Vernuntii)][NuGet Package] -->

[:running: Quick start guide](#quick-start-guide) &nbsp; 
<!-- | &nbsp; [:package: NuGet Package][NuGet Package] -->

Vernuntii (transl. versionable messages) is a tool for calculating the next semantic version. The tool has the capability to iterate a stream of (commit) messages and decide upon versioning mode to increment major, minor, patch or height. When using the git plugin the pre-release is derived from branch and highly customizable. The version prefix (e.g. v) is either inherited (default), (initially) set or explicitly removed depending on configuration. Each branch is separatly configurable. The most important fact is that this tool is single branch scoped like MinVer or Verlite, so simply said it reads all commits you see in git log.

### Key facts

- Git plugin
  - Searches for latest commit version
  - Uses commit messages as message stream
  - Enables branch-based configuration
- Requires configuration file
  - Either json or yaml
- Wide range of versioning mode presets
  - E.g. Continous Delivery, Continous Deployment, Manual or
  - E.g. Conventional Commits Delivery or Conventional Commits Deployment
  - With possiblity to override parts of preset
- Wide range of message conventions
  - E.g. Continous, Manual or Conventional Commits
- Inbuilt cache mechanism
- Concurrency support
  
### Vernuntii installers

A Vernuntii installer is another term for getting the bare metal binaries to execute Vernuntii directly. For example the .NET CLI package is used in GitHub Actions integration.

#### .NET CLI package

```
dotnet tool install --global Vernuntii.Console.GlobalTool --version 0.1.0

# local
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local Vernuntii.Console.GlobalTool --version 0.1.0
```

### Vernuntii integrations

A Vernuntii integration is a facility that uses Vernuntii internally and allows cool workflows.

#### MSBuild package

#### GitHub Actions

## Quick start guide

> :warning: This README is under construction. Please take a seat or take part to the discussions: https://github.com/vernuntii/vernuntii/discussions

<!-- # Command-line interface

Vernuntii provides a console application for outputting the next semantic version of a branch on basis of a message convention like Conventional Commits. -->

<!-- [:package: Download .NET CLI][NuGet Package .NET CLI] &nbsp; | &nbsp; [:package: GitHub Actions][GitHub Actions] -->

[NuGet Package]: https://www.nuget.org/packages/Vernuntii
[NuGet Package .NET CLI]: https://www.nuget.org/packages/GitVersion.Tool/#dotnet-cli-local
[GitHub Actions]: https://github.com/vernuntii/actions