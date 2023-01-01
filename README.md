![Vernuntii Logo](res/logo.svg)

[:running: **Quick start guide**](#quick-start-guide) &nbsp; | &nbsp; [:bulb: How it works](#how-it-works) &nbsp; | &nbsp; [ :scroll: Chat on gitter](https://gitter.im/vernuntii/vernuntii?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

Vernuntii (latin for versionable messages) is your tool to get rid of manually versioning your software.

<!-- omit in toc -->
### Vernuntii key facts on a glance

- Calculates version with [adaptivity][self-version-adaptivity] in mind
- Highly customizable via [configuration file][configuration-file]
  - Either json or yaml
- Many inbuilt [versioning modes][configuration-versioning-modes]
  - Possiblity to override increment mode, message convention and more
- Mighty git plugin
  - Reads commits and tags, so it:
    - Searches for latest commit version
    - Uses commit messages as message stream
  - Enables branch-based configurations
  - Fails fast if using a shallow repository
- Two layer inbuilt cache mechanism
- SUPER FAST due to R2R-compilation
- Predestined to run concurrently
- Extendable through plugins (TBD)

<!-- omit in toc -->
## Quick start guide

#### Prerequisites

- .NET 6.0 Runtime
- Git
- A repository with at least one commit :smiley:

> Please make sure the repository is not shallowed!

#### Use case #1

Using only [MSBuild Integration][msbuild-nuget-package-docs] because

- You are author of one or more projects whose resulting packages need to be versioned by Vernuntii
- You do not need to coordinate the versioning "from above" like Nuke, Cake or any ontinuous integration platform
- After publishing the packages with the calculated version from Vernuntii you are willing to create manually the git tag

#### Use case #2

Using [MSBuild Integration][msbuild-nuget-package-docs] with [GitHub actions][self-github-actions] because

- You would like to automate the git tag creation
- You want to define a GitHub workflow with the intention to publish versionized packages:
  1. You define a step that uses [`vernuntii/actions/execute@main`][self-github-actions] to have access to the calculated next version
  3. You define a step that pushes the versionized packages to your package registry (e.g. NuGet)
  4. You define a step that creates a git tag with the next version and pushes it back to your repository

> This repository uses such workflow: [.github/workflows/build-test-pack-publish.yml](.github/workflows/build-test-pack-publish.yml)

#### Use case #3

Using only [.NET CLI package](#net-cli-package) because

- You want to have access to the "vernuntii"-binary
- You need full control and want to realize custom workflows
- You want to take a look at --help :upside_down_face:

#### **Use case #?**

Your use case is not described above? Open an issue and tell me about it.

<!-- omit in toc -->
## How it works

So we first assume a configuration file that is empty or not even existing. In this case defaults are applied. They roughly look like this:

```yaml
# /vernuntii.yml
VersioningMode: ContinuousDelivery # What's that? I'll explain.
```

We also assume we are in the branch `main`. Now we move on to the logic.

1. If the current commit has one or more lightweight git tags with a SemVer-compatible version string as tag name then the **latest** version of these versions is used and the next bullet points are skipped.
2. If the current commit *does not* have one SemVer-compatible version then the next version is about to be calculated, so we take a look to `git log` and go from current commit backwards and search for the latest version.
3. If the latest version has been found, then this version serves as start version, otherwise the default version `0.1.0-main.0` is assumed and next bullet points are skipped.
4. If a pre-release is given then as `ContinuousDelivery` indicates, we assume a human triggered release workflow or in other words: we only need the next version when the program is in an actual delivery state. So we once <ins>increment height by one</ins>. If the height was `0`, then it won't exceed `1` in this calculation. Why height and not patch? Because of the word "Continuous". Yes, the program in a delivery state and we just want ontinuously calculate the next version but without soil the version core, so we increment the height.
5. If a pre-release is not given then as `ContinuousDelivery` indicates, we once <ins>increment the patch by one</ins>. If the patch was `0`, then it won't exceed `1` in this calculation. Why patch? Because a change in version core signalizes a *new release*. Why only patch? First of all, there are different [versioning modes][configuration-versioning-modes] but `ContinuousDelivery` is one of the more feasible workflows that are easy introducable, so if you want for example introduce a minor or major release, you would have to adjust the versioning preset to apply <ins>your convention</ins> that affects not only patch anymore or you create a tag that reflects the next minor or major release and [vernuntii will just consider it][self-version-adaptivity] in next version calculation.

<!-- omit in toc -->
# Table of contents

- [Version adaptivity](#version-adaptivity)
- [Vernuntii installers](#vernuntii-installers)
  - [.NET CLI package](#net-cli-package)
- [Vernuntii integrations](#vernuntii-integrations)
  - [MSBuild package](#msbuild-package)
  - [GitHub actions](#github-actions)
- [Development](#development)
  - [Getting started](#getting-started)
    - [Minimum requirements](#minimum-requirements)
  - [Vernuntii.SemVer.Parser](#vernuntiisemverparser)
  - [Issues I am working on](#issues-i-am-working-on)
- [License](#license)

## Version adaptivity

Vernuntii wants to simplify semantic versioning, so it tries to adapt as much as possible and let the user control how the next version should be calculated.

<!-- omit in toc -->
### Version prefix

The version prefix refers to the prefix before the version core e.g. `v0.1.0`. The current behaviour is to adapt if existing the prefix `v` of the latest version.

- If latest is
  - `v0.1.0` -> prefix `v` is adapted
  - `0.1.0` -> no prefix is adapted

> Currently none or `v` prefix is adapted. You cannot set initial or explicit version prefix yet. When you want an initial prefix then it is recommended to set the start version via git tag or configuration file.

<!-- omit in toc -->
### Version core

The version core refers to `<major>.<minor>.<patch>`. When Vernuntii searches for the latest version. It does not care what the latest version core is. If we assume

```
771c4ea (HEAD -> main) second commit
8f24ad3 (tag: 0.1.0) initial commit
```

then the latest version would be `0.1.0`. If we decide to change the version by ourself by tagging HEAD with `0.5.0` for whatever reason

```
771c4ea (HEAD -> main, tag: 0.5.0) second commit
8f24ad3 (tag: 0.1.0) initial commit
```

then the latest version would be `0.5.0`.

<!-- omit in toc -->
### Drawback

One drawback of being adaptive is that the latest or next version is not deterministic when you would remove all tags, but to be fair: I am not aware of any versioning tool that gurantees full determinism in calculating the next version in case you remove all tags.

## Vernuntii installers

A Vernuntii installer is another term for getting the bare metal binaries to execute Vernuntii directly. For example the .NET CLI package is used in [GitHub actions][self-github-actions] integration.

### .NET CLI package

[![Nuget][globaltool-nuget-package-badge]][globaltool-nuget-package]

```
dotnet tool install --global Vernuntii.Console.GlobalTool --version 0.1.0-alpha.0

# local
dotnet new tool-manifest # if you are setting up this repo
dotnet tool install --local Vernuntii.Console.GlobalTool --version 0.1.0-alpha.0
```

## Vernuntii integrations

A Vernuntii integration is a facility that uses Vernuntii internally and allows cool workflows.

### MSBuild package

[![Nuget][msbuild-nuget-package-badge]][msbuild-nuget-package]

The MSBuild package is called `Vernuntii.Console.MSBuild` and installable over NuGet store or by adding these lines to your project:

```
<PackageReference Include="Vernuntii.Console.GlobalTool" Version="0.1.0-alpha.0">
  <PrivateAssets>all</PrivateAssets>
  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
</PackageReference>
```

When installed it sets MSBuild-specific properties:

- `<Version>$(Vernuntii_SemanticVersion)</Version>`
- `<VersionPrefix>$(Vernuntii_VersionCore)</VersionPrefix>`
- `<VersionSuffix>$(Vernuntii_PreRelease)$(Vernuntii_PlusBuild)</VersionSuffix>`
- `<PackageVersion>$(Vernuntii_SemanticVersion)</PackageVersion>`
- `<AssemblyVersion>$(Vernuntii_VersionCore).0</AssemblyVersion>` (if not previously defined)
- `<InformationalVersion>$(Vernuntii_VersionCore)$(Vernuntii_HyphenPreRelease)+$(Vernuntii_BranchName)</InformationalVersion>` (if not previously defined)
- `<FileVersion>$(Vernuntii_VersionCore).0</FileVersion>` (if not previously defined)

The `Vernuntii_*`-properties are provided by an internal MSBuild-task that calls the Vernuntii global tool.

From the following set of **optional properties** you can choose to change the behaviour of the MSBuild package:

- `<DisableVernuntii/>`
  - Disables Vernuntii
- `<VernuntiiMSBuildIntegrationAssemblyFile/>`
- `<VernuntiiConsoleExecutableFile/>`
- `<VernuntiiVerbosity/>`
  - Allowed value: `Debug`, `Error`, `Fatal` (implicit default), `Information`, `Verbose`, `Warning`
- `<VernuntiiConfigPath/>`
  - Path to [configuration file][configuration-file]
- `<VernuntiiCacheId/>`
  - The cache id (default is `SHORT_LIVING_CACHE`)
- `<VernuntiiCacheCreationRetentionTime/>`
  - The retention time after what time since creation the cache should be renewed
- `<VernuntiiCacheLastAccessRetentionTime/>`
  - The retention time after what time of last access the cache should be renewed
- `<VernuntiiEmptyCaches/>`
  - `true` empties the cache before applying any rules of retention time
- `<ExecuteVernuntiiTaskDependsOn/>`
  - MSBuild-targets to depend on when calling the `ExecuteVernuntiiTask`-MSBuild-target.
- `<ExecuteVernuntiiTaskBeforeTargets/>`
  - Prepends MSBuild-targets to the `BeforeTargets` of `ExecuteVernuntiiTask`-MSBuild-target.
- `<UpdateVersionPropsFromVernuntiiTask/>`
  - `false` means the MSBuild-specific properties (`Version`, `VersionPrefix`, ...) are not set anymore but  `Vernuntii_*`-properties are still available

### GitHub actions

The following [GitHub actions][vernuntii-actions] are available.

- `vernuntii/actions/install/dotnet-tool@main`
  - Using this GitHub action makes the global command "vernuntii" available
- `vernuntii/actions/install/msbuild-import@main`
  - Enables "Vernuntii"-.targets file in subsequent calls of MSBuild
- `vernuntii/actions/execute@main`
  - Executes the "vernuntii"-binary

> :warning: Ensure to not fetch the repository shallowed by simply specifying:
> ```
> - uses: actions/checkout@v3
>   with:
>     fetch-depth: 0
> ```

## Development

### Getting started

The project is out of the box compilable and you don't have to initialize anything before. Only consider the [Minimum requirements](#minimum-requirements).

#### Minimum requirements

- Windows (OS) for cross-compiling
- Visual Studio 2022 (optional)
- .NET 6.0 SDK

### Vernuntii.SemVer.Parser

[![Nuget][vernuntii-semver-parser-nuget-badge]][vernuntii-semver-parser-nuget]

Vernuntii uses [Vernuntii.SemVer.Parser][vernuntii-semver-parser-nuget] to parse your version strings. If you want to use it too, then check out the [README.md][vernuntii-semver-parser-readme] for more details.

### Issues I am working on

You can follow the tasks in this [GitHub project](https://github.com/orgs/vernuntii/projects/2/views/1).

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

[self-version-adaptivity]: #version-adaptivity
[self-github-actions]: #github-actions
[msbuild-nuget-package]: https://www.nuget.org/packages/Vernuntii.Console.MSBuild
[msbuild-nuget-package-badge]: https://img.shields.io/nuget/v/Vernuntii.Console.MSBuild
[msbuild-nuget-package-docs]: #msbuild-package
[globaltool-nuget-package]: https://www.nuget.org/packages/Vernuntii.Console.GlobalTool
[globaltool-nuget-package-badge]: https://img.shields.io/nuget/v/Vernuntii.Console.GlobalTool
[vernuntii-actions]: https://github.com/vernuntii/actions
[configuration-file]: ./docs/configuration-file.md
[configuration-versioning-modes]: ./docs/configuration-file.md#git-plugin--branches--versioning-mode
[semver-nuget-package]: https://www.nuget.org/packages/Vernuntii.SemVer
[semver-parser-nuget-package]: https://www.nuget.org/packages/Vernuntii.SemVer.Parser
[vernuntii-semver-parser-readme]: ./src/Vernuntii.SemVer.Parser
[vernuntii-semver-parser-nuget]: https://www.nuget.org/packages/Vernuntii.SemVer.Parser
[vernuntii-semver-parser-nuget-badge]: https://img.shields.io/nuget/v/Vernuntii.SemVer.Parser