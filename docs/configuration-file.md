<!-- omit in toc -->
# Configuration file

The configuration file is the heart of Vernuntii. By defining properties you change fundamentally the behvaiour of Vernuntii.

## Auto-discovery

The configuration file will be auto-discovered when the file name meets the following rules:

- Has the **file name** `vernuntii`
- Ends with `.json`, `.yaml` or `.yml` (recommended) using their respective file format

Depending on integration the following working directory is assumed where the search for the configuration file is beginning and is going upwards until the root (`C:\` or `/`):

- MSBuild Integration: directory of project that has the MSBuild integration installed
- GitHub Actions (`vernuntii/actions/execute`): current working directory at the time where the action is called

<!-- omit in toc -->
# Table of contents

- [Git plugin → Global properties](#git-plugin--global-properties)
  - [Schema → `<git-directory>`](#schema--git-directory)
    - [Default values](#default-values)
  - [Schema → `<start-version>`](#schema--start-version)
    - [Default values](#default-values-1)
- [Git plugin → Branches](#git-plugin--branches)
  - [Schema → `<if-branch:string>`](#schema--if-branchstring)
    - [Examples](#examples)
  - [Schema → `<branch:string>`](#schema--branchstring)
    - [Default values](#default-values-2)
  - [Schema → `<since-commit:string>`](#schema--since-commitstring)
    - [Default values](#default-values-3)
  - [Schema → `<preset>`](#schema--preset)
  - [Schema → `<pre-release:string>`](#schema--pre-releasestring)
    - [Default values](#default-values-4)
  - [Schema → `<pre-release-escapes:list>`](#schema--pre-release-escapeslist)
    - [Default values](#default-values-5)
  - [Schema → `<search-pre-release:string>`](#schema--search-pre-releasestring)
    - [Default values](#default-values-6)
  - [Schema → `<search-pre-release-escapes:list>`](#schema--search-pre-release-escapeslist)
    - [Default values](#default-values-7)
- [Git plugin → Branches → Versioning mode](#git-plugin--branches--versioning-mode)
  - [Schema → `<preset:string>`](#schema--presetstring)
  - [Schema → `<preset:object>`](#schema--presetobject)
  - [Schema → `<message-convention:string>`](#schema--message-conventionstring)
  - [Schema → `<message-convention:object>`](#schema--message-conventionobject)
  - [Schema → `<message-indicator:string>`](#schema--message-indicatorstring)
  - [Schema → `<message-indicator:object>` → `Regex`](#schema--message-indicatorobject--regex)
    - [Examples](#examples-1)
  - [Schema → `<increment-mode:string>`](#schema--increment-modestring)
  - [Schema → `<increment-flow:string>`](#schema--increment-flowstring)
  - [Schema → `<increment-flow:object>`](#schema--increment-flowobject)
  - [Schema → `<increment-flow-condition:string>`](#schema--increment-flow-conditionstring)
  - [Schema → `<increment-flow-mode:string>`](#schema--increment-flow-modestring)
  - [Examples](#examples-2)

# Git plugin → Global properties

## Schema → `<git-directory>`

```yaml
# The git directory to use
GitDirectory: <git-branch:string> (optional)
```

### Default values

```yaml
GitDirectory: (the location where this configuration file has been found)
```

## Schema → `<start-version>`

```yaml
# The start version when not a single version exist
StartVersion: <start-version:string> (optional)
```

### Default values

```yaml
StartVersion: (latest commit version or if not found "0.1.0-main.0" where "main" is active branch name as long as not configured otherwise)
```

# Git plugin → Branches

The git plugin allows you to define branches where their rules not affect each other. 

When I talk about branches, then I talk always about _branch cases_. A branch case by definition of Vernuntii is a ruleset of how Vernuntii should behave on this or that branch.

There is <ins>always</ins> a **default branch case**. It does apply when a branch case is not found for the current branch you are active right now. The default branch case can be customized at the root of the configuration.

```
# /vernuntii.yml
# Set here the properties for the default branch case!
```

> [`<if-branch>`](#schema--if-branch) is not available in the default branch case.

## Schema → `<if-branch:string>`

```yaml
# The branch case is used if active branch is equals to <if-branch>.
# If current active branch is matching <if-branch> than the settings
# specified in default branch case are used.
IfBranch: <if-branch:string> (required)
```

Available values for `<if-branch:string>`:

- Full branch name or a part of it
  - main (head)
  - heads/main (head)
  - refs/heads/main (head)
  - origin/main (remote)
  - remotes/origin/main (remote)
  - refs/remotes/origin/main (remote)
- `<if-branch>` is recognized as **regular expression** when it begins with '/' and ends with another '/'
  - `/feature-.*/`: all branches starting with `feature-`
  - If multiple branches are selected an exception is thrown, so use this feature with caution
- `HEAD` when you want to match the _detached HEAD_.

### Examples

```
# /vernuntii.yml

Branches:
  - IfBranch: develop # branch case for develop branch
```

## Schema → `<branch:string>`

```yaml
# The branch reading the commits from.
Branch: <branch-name:string> (optional)
```

- If not specified the branch is the one evaluated from `IfBranch`
- It must be a valid branch name (see `<if-branch>`), but
- RegEx is NOT allowed

### Default values

```yaml
Branch: (the branch evaluated from `<if-branch-string>`)
```

## Schema → `<since-commit:string>`

```yaml
# The since-commit where to start reading from.
SinceCommit: <since-commit:string> (optional)
```

### Default values

```yaml
SinceCommit: (latest commit version)
```

## Schema → `<preset>`

The schema for [`<preset>`](#git-plugin--branches--versioning-mode) deserves its own section.

## Schema → `<pre-release:string>`

```yaml
# The pre-release is used for pre-search or post-transformation.
# Used only in pre-search if "SearchPreRelease" is
# null.
PreRelease: <pre-release:string> (optional)
```

### Default values

```yaml
PreRelease: (short name of current branch, e.g. main)
```

## Schema → `<pre-release-escapes:list>`

```yaml
# Pre-release escapes.
PreReleaseEscapes:
  - Pattern: <pattern:string> (optional)
    Replacement: <replacement:string> (optional)
```

- `<pattern>` is recognized as **regular expression** when it begins with '/' and ends with another '/'

### Default values

```yaml
PreReleaseEscapes:
  - Pattern: /[A-Z]/
    Replacement: (lower)
  - Pattern: ///
    Replaceent: '-'
```

## Schema → `<search-pre-release:string>`

```yaml
# The pre-release is used for pre-search.
# If null or empty non-pre-release versions are included in
# search.
# If specified then all non-release AND version with this
# pre-release are included in search.
SearchPreRelease: <search-pre-release:string> (optional)
```

### Default values

```yaml
SearchPreRelease: (if not specified then it inherits <pre-release>)
```

## Schema → `<search-pre-release-escapes:list>`

```yaml
# Search pre-release escapes.
SearchPreReleaseEscapes: (see <pre-release-escapes:list>)
```

- `<pattern:string>` is recognized as **regular expression** when it begins with '/' and ends with another '/'

### Default values

```yaml
SearchPreReleaseEscapes: (if not specified it inherits <pre-release-escapes:list>)
```

# Git plugin → Branches → Versioning mode

The versioning mode allows you to choose one of the many available version strategies to calculate the next version.

```yaml
VersioningMode: <preset:string> or <preset:object> # see schema below
```

You can define `VersioningMode` also in every branch:

```yaml
Branches:
  - IfBranch: <if-branch:string>
    VersioningMode: <preset:string> or <preset:object> # see schema below
```

## Schema → `<preset:string>`

```yaml
# The versioning mode defined by string
VersioningMode: <preset:string> or <preset:object> (optional)
```

Available values for `<preset:string>`:

- `Default`: resolves internally to `ContinuousDelivery`
- `ContinuousDelivery`
  - `MessageConvention`: `Continuous` (see [`<message-convention:string>`][message-convention-string])
  - `IncrementMode`: `Consecutive` (see [`<increment-mode:string>`][increment-mode-string])
  - `IncrementFlow`: `None` (see [`<increment-flow:string>`][increment-flow-string])
- `ContinuousDeployment`
  - `MessageConvention`: `Continuous` (see [`<message-convention:string>`][message-convention-string])
  - `IncrementMode`: `Successive` (see [`<increment-mode:string>`][increment-mode-string])
  - `IncrementFlow`: `None` (see [`<increment-flow:string>`][increment-flow-string])
- `ConventionalCommitsDelivery`
  - `MessageConvention`: `ConventionalCommits` (see [`<message-convention:string>`][message-convention-string])
  - `IncrementMode`: `Consecutive` (see [`<increment-mode:string>`][increment-mode-string])
  - `IncrementFlow`: `None` (see [`<increment-flow:string>`][increment-flow-string])
- `ConventionalCommitsDeployment`
  - `MessageConvention`: `ConventionalCommits` (see [`<message-convention:string>`][message-convention-string])
  - `IncrementMode`: `Successive` (see [`<increment-mode:string>`][increment-mode-string])
  - `IncrementFlow`: `None` (see [`<increment-flow:string>`][increment-flow-string])
- `Manual`
  - `MessageConvention`: `Falsy` (see [`<message-convention:string>`][message-convention-string])
  - `IncrementMode`: `None` (see [`<increment-mode:string>`][increment-mode-string])
  - `IncrementFlow`: `None` (see [`<increment-flow:string>`][increment-flow-string])

If `VersioningMode` is not set its default is:

```yaml
VersioningMode: Default # 'Default' resolves internally to 'ContinuousDelivery'
```

## Schema → `<preset:object>`

```yaml
# More detailed
VersioningMode:
  Preset: <preset:string> (optional)
  MessageConvention: <message-convention:string> or <message-convention:object> (optional if "Preset" is set)
  IncrementMode: <increment-mode:string> (optional if "Preset" is set)
  IncrementFlow: <increment-flow:string> or <increment-flow:object> (optional)
```

If `VersioningMode` is defined as object its default is:

```yaml
VersioningMode:
  Preset: Manual
  MessageConvention: None (inherited from "Preset")
  IncrementMode: None (inherited from "Preset")
  IncrementFlow: None (inherited from "Preset")
```

## Schema → `<message-convention:string>`

```yaml
# More detailed
VersioningMode:
  ...
  MessageConvention: <message-convention:string> (optional if <preset> is set)
```

Available values for `<message-convention:string>`:

- `Manual`: same as `<message-convention>` of preset `Manual`.
- `Continuous`: same as `<message-convention>` of preset `Continuous*`.
  - Major indicator: `Falsy` (see [`<message-indicator:string>`][message-indicator-string])
  - Minor indicator: `Falsy` (see [`<message-indicator:string>`][message-indicator-string])
  - Patch indicator: `Truthy` (see [`<message-indicator:string>`][message-indicator-string])
- `ConventionalCommits`:
  - Major indicator: `ConventionalCommits` (see [`<message-indicator:string>`][message-indicator-string])
  - Minor indicator: `ConventionalCommits` (see [`<message-indicator:string>`][message-indicator-string])
  - Patch indicator: `ConventionalCommits` (see [`<message-indicator:string>`][message-indicator-string])

## Schema → `<message-convention:object>`

```yaml
# With custom message convention where indicators are short-cutted by string
VersioningMode:
  ...
  MessageConvention:
    # Use it when you want to inherit a message convention
    # and extend only a part of an already existing one.
    Base: <message-convention:string> (optional)

    # All logic of how "MajorIndicators" can be defined does also apply to
    # - "MinorIndicators" 
    # - "PatchIndicators"

    # "MajorIndicators", "MinorIndicators" or "PatchIndicators" 
    # can be either set from string OR list but not both
    # The following shows the distinction:

    # Set "MajorIndicators", "MinorIndicators" or "PatchIndicators" from string
    MajorIndicators: <message-indicator:string> (optional)

    # Or set "MajorIndicators", "MinorIndicators" or "PatchIndicators"  from list
    MajorIndicators:
      # List item can be string
      - <message-indicator:string>

      # List item can be object
      - <message-indicator:object>
      - Name: <message-indicator:string>
        <additional-data>: ... (some message indicator require addtional data)

    MinorIndicators: ... (same as above)
    PatchIndicators: ... (same as above)
```

If `MessageConvention` is defined as object its default is:

```yaml
VersioningMode:
  ...
  MessageConvention: 
    Base: None
    MajorIndicators: (inherited from "Base")
    MinorIndicators: (inherited from "Base")
    PatchIndicators: (inherited from "Base")
```

If `Base` is set in `MessageConvention`:

```yaml
VersioningMode:
  ...
  MessageConvention:
    # Base gets inherited by
    # - "MajorIndicators",
    # - "MajorIndicators" or 
    # - "MajorIndicators" if any of them are not set by user
    Base: <message-convention:string>
    MajorIndicators: (inherited from "Base")
    MinorIndicators: (inherited from "Base")
    PatchIndicators: (inherited from "Base")
```

## Schema → `<message-indicator:string>`

Available values for `<message-indicator:string>`:

- `Falsy`: indicates that every message <ins>does not increment</ins> the version
- `Truthy`: indicates that every message <ins>does increment</ins> the version
- `ConventionalCommits`
  - If used as major indicator: `^(feat)(\([\w\s-]*\))?(!:|:.*\n\n((.+\n)+\n)?BREAKING CHANGE:\s.+)`
  - If used as minor indicator: `^(feat)(\([\w\s-]*\))?:`
  - If used as patch indicator: `^(fix)(\([\w\s-]*\))?:`
- [`Regex`](#schema--message-indicator--regex): indicates a version increment if the message could be matched against the regular expression

## Schema → `<message-indicator:object>` → `Regex`

```yaml
Name: Regex # Required!
Pattern: <pattern:string> # Must be a valid regular expression.
```

### Examples

```yaml
VersioningMode:
  ...
  MessageConvention:
    ...
    PatchIndicators:
      - Name: Regex 
        Pattern: '^(fix)(\([\w\s-]*\))?:' # Imitates patch indicator of "ConventionalCommits".
```

## Schema → `<increment-mode:string>`

Available values for `<increment-mode:string>`:

- `None`: does not increment anything
- `Consecutive`: increment only the most significant version number once if a message indicates that it increments the version
- `Successive`: increment most significant version number as often as messages indicate to increment the version
  
## Schema → `<increment-flow:string>`

Available values for `<increment-flow:string>`:

- `None`: does not lead into a flow of any version part at any circumstances
  - `Condition`: `Never` (see [`<increment-flow-condition:string>`][increment-flow-condition-string])
  - `MajorFlow`: `None` (see [`<increment-flow-mode:string>`][increment-flow-mode-string])
  - `MinorFlow`: `None` (see [`<increment-flow-mode:string>`][increment-flow-mode-string])
- `ZeroMajorDownstream`: does right shift version when latest version has zero major, so next major becomes next minor, next minor becomes next patch
  - `Condition`: `ZeroMajor` (see [`<increment-flow-condition:string>`][increment-flow-condition-string])
  - `MajorFlow`: `Downstream` (see [`<increment-flow-mode:string>`][increment-flow-mode-string])
  - `MinorFlow`: `Downstream` (see [`<increment-flow-mode:string>`][increment-flow-mode-string])

## Schema → `<increment-flow:object>`

```yaml
VersioningMode:
  IncrementFlow:
    Condition: <increment-flow-condition:string> (optional)
    MajorFlow: <increment-flow-mode:string> (optional)
    MinorFlow: <increment-flow-mode:string> (optional)
```

## Schema → `<increment-flow-condition:string>`

Available values for `<increment-flow-condition:string>`:

- `Never`: a condition that does not lead into a flow of any version part at any circumstances
- `ZeroMajor`: a condition that is met when major version is `0`

## Schema → `<increment-flow-mode:string>`

Available values for `<increment-flow-mode:string>`:

- `None`: do not flow
- `Downstream`: affected version part flows one position downstream (e.g. when specified in `MajorFlow`: instead major increments, minor increments now)

## Examples

Versioning mode one-liner.

```yaml
VersioningMode: ConventionalCommitsDeployment
```

Overwrite preset.

```yaml
VersioningMode:
  Preset: ConventionalCommitsDelivery
  MessageConvention: Continuous # Overwrites "MessageConvention" of preset
  IncrementMode: Successive # Overwrites "IncrementMode" of preset

  # When a major is about to be incremented it won't, instead the minor is incremented. This does not trigger minor flow.
  # When a minor is about to be incremented it won't, instead the patch is incremented.
  IncrementFlow:
    Condition: ZeroMajor
    MajorFlow: Downstream
    MinorFlow: Downstream
```

Custom message convention in branch develop.

```yaml
Branches:
  - IfBranch: develop
    VersioningMode:
      Preset: ContinuousDelivery
      MessageConvention:
        MajorIndicators: Falsy # Ok, we want major to never increment.
        MinorIndicators:
          - Name: Regex
            Pattern: '^(feat)(\([\w\s-]*\))?:' # Let's imitate minor indicator of "ConventionalCommits".
        PatchIndicators:
          - Truthy # When major or patch didn't indicate anything already then I want at least to increment patch!
```

[message-convention-string]: #schema--message-conventionstring
[increment-mode-string]: #schema--increment-modestring
[increment-flow-string]: #schema--increment-flowstring
[message-indicator-string]: #schema--message-indicatorstring
[increment-flow-condition-string]: #schema--increment-flow-conditionstring
[increment-flow-mode-string]: #schema--increment-flow-modestring