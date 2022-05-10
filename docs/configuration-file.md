# Configuration file

The configuration file is the heart of Vernuntii. By defining properties you change fundamentally the behvaiour of Vernuntii.

- [Configuration file](#configuration-file)
- [Auto-discovery](#auto-discovery)
- [Git plugin → Global properties](#git-plugin--global-properties)
  - [Schema → `<git-directory>`](#schema--git-directory)
    - [Defaults](#defaults)
  - [Schema → `<start-version>`](#schema--start-version)
    - [Defaults](#defaults-1)
- [Git plugin → Branches](#git-plugin--branches)
  - [Schema → `<if-branch>`](#schema--if-branch)
  - [Schema → `<branch>`](#schema--branch)
  - [Schema → `<since-commit>`](#schema--since-commit)
  - [Schema → `VersioningMode`](#schema--versioningmode)
  - [Schema → `<pre-release>`](#schema--pre-release)
  - [Schema → `<pre-release-escapes>`](#schema--pre-release-escapes)
  - [Schema → `<search-pre-release>`](#schema--search-pre-release)
  - [Schema → `<search-pre-release-escapes>`](#schema--search-pre-release-escapes)
  - [Schema → Defaults](#schema--defaults)
- [Git plugin → Branches → Versioning mode](#git-plugin--branches--versioning-mode)
  - [Schema → `<preset:string>`](#schema--presetstring)
  - [Schema -> `<preset:object>`](#schema---presetobject)
  - [Schema → `<message-convention:string>`](#schema--message-conventionstring)
  - [Schema → `<message-convention:object>`](#schema--message-conventionobject)
  - [Schema → `<message-indicator:string>`](#schema--message-indicatorstring)
  - [Schema → `<message-indicator:object>` → `Regex`](#schema--message-indicatorobject--regex)
    - [Examples](#examples)
  - [Schema → `<increment-mode>`](#schema--increment-mode)
  - [Schema → `<increment-flow:string>`](#schema--increment-flowstring)
  - [Schema → `<increment-flow:object>`](#schema--increment-flowobject)
  - [Schema → `<increment-flow-condition:string>`](#schema--increment-flow-conditionstring)
  - [Schema → `<increment-flow-mode:string>`](#schema--increment-flow-modestring)
  - [Schema → Examples](#schema--examples)

# Auto-discovery

The configuration file will be auto-discovered when the file name meets the following rules:

- Has the **file name** `vernuntii`
- Ends with `.json`, `.yaml` or `.yml`

Depending on integration you use the following working directory and therefore default location to start the searching from:

- MSBuild Integration: root directory of project that has the MSBuild integration installed
- GitHub Actions (`vernuntii/actions/execute`): current working directory at the time where the action was called

# Git plugin → Global properties

## Schema → `<git-directory>`

```yaml
# The git directory to use
GitDirectory: <git-branch:string> (optional)
```

### Defaults

```yaml
GitDirectory: (the location where this configuration file has been found)
```

## Schema → `<start-version>`

```yaml
# The start version when not a single version exist
StartVersion: <start-version:string> (optional)
```

### Defaults

```yaml
StartVersion: (latest commit version or if not found "0.1.0-main.0" where "main" is active branch name as long as not configured otherwise)
```

# Git plugin → Branches

The git plugin allows you to define branches where their rules. They do not affect each other. 

When I talk about branches, then I talk always about _branch cases_. A branch case by definition of Vernuntii is a ruleset of how Vernuntii should behave on this or that branch.

```
Branches:
  - IfBranch: ... # other branch case
```


There is <ins>always</ins> a **default branch case**. It does apply when a branch case is not found for the current branch you are active right now. The default branch case can be customized at the root of the configuration.

> As you maybe can think of, `<if-branch>` is not available in the default branch case.

```
# Set here the properties for the default branch case!

Branches:
  - IfBranch: ... # other branch case
```

## Schema → `<if-branch>`

- Can be the the full branch name or a part of it
  - main (head)
  - heads/main (head)
  - refs/heads/main (head)
  - origin/main (remote) 
  - remotes/origin/main (remote)
  - refs/remotes/origin/main (remote)
- `<if-branch>` is recognized as **regular expression** when it begins with '/' and ends with another '/'
  - `/feature-.*/`: all branches starting with `feature-`
  - If multiple branches are selected an exception is thrown, so use this feature with caution
- You can specify `HEAD` when you want to <ins>match detached HEAD</ins>.

## Schema → `<branch>`

```yaml
# The branch reading the commits from.
Branch: <branch-name:string> (optional)
```

- If not specified the branch is the one evaluated from `IfBranch`
- It must be a valid branch name (see `<if-branch>`), but
- RegEx is NOT allowed (change my mind. :D)

## Schema → `<since-commit>`

```yaml
# The since-commit where to start reading from.
SinceCommit: <since-commit:string> (optional)
```

## Schema → `VersioningMode`

The schema for [`VersioningMode`](#git-plugin--branches--versioning-mode) deserves its own section.

## Schema → `<pre-release>`

```yaml
# The pre-release is used for pre-search or post-transformation.
# Used only in pre-search if "SearchPreRelease" is
# null.
PreRelease: <pre-release:string> (optional, inheritable)
```

## Schema → `<pre-release-escapes>`

```yaml
# Pre-release escapes.
PreReleaseEscapes: <pre-release-escapes:object> (optional, inheritable)
  - Pattern: <pattern:string> (optional)
    Replacement: <replacement:string> (optional)
  - ...
```

- `<pattern>` is recognized as **regular expression** when it begins with '/' and ends with another '/'

## Schema → `<search-pre-release>`

```yaml
# The pre-release is used for pre-search.
# If null or empty non-pre-release versions are included in
# search.
# If specified then all non-release AND version with this
# pre-release are included in search.
SearchPreRelease: <search-pre-release:string> (optional)
```

## Schema → `<search-pre-release-escapes>`

```yaml
# Search pre-release escapes.
SearchPreReleaseEscapes: <search-pre-release-escapes:object> (same as <pre-release-escapes>)
```

- `<pattern>` is recognized as **regular expression** when it begins with '/' and ends with another '/'

## Schema → Defaults

These defaults are automatically applied.

```yaml
###################################################################
# Reminder: The below properties are available in "Branches" too. #
###################################################################

VersioningMode: Default # 'Default' resolves internally to 'ContinousDelivery'
SinceCommit: (latest version)

PreRelease: (short name of current branch, e.g. main)

PreReleaseEscapes:
  - Pattern: /[A-Z]/
    Replacement: (lower)
  - Pattern: ///
    Replaceent: '-'

SearchPreRelease: (if not specified then it inherits <pre-release>)
SearchPreReleaseEscapes: (if not specified then it inherits <pre-release-escapes>)
```

# Git plugin → Branches → Versioning mode

The versioning mode allows you to choose one of the many available version strategies to calculate the next version. You can define `VersioningMode` also in every branch.

```yaml
Branches:
  - IfBranch: <if-branch>
    VersioningMode: (same as above)
```

## Schema → `<preset:string>`

```yaml
# The versioning mode defined by string
VersioningMode: <preset:string> (optional)
```

Available values for `<preset:string>`:

- `Default`: resolves internally to `ContinousDelivery`
- `ContinousDelivery`
  - `<message-convention>`
    - Major indicator: `Falsy`
    - Minor indicator: `Falsy`
    - Patch indicator: `Truthy`
  - `<increment-mode>`: `Consecutive`
  - `<increment-flow>`: `None`
- `ContinousDeployment`
  - `<message-convention>`: same as `ContinousDelivery`
  - `<increment-mode>`: `Successive`
  - `<increment-flow>`: `None`
- `ConventionalCommitsDelivery`
  - `<message-convention>`: `ConventionalCommits`
  - `<increment-mode>`: `Consecutive`
  - `<increment-flow>`: `None`
- `ConventionalCommitsDeployment`
  - `<message-convention>`: same as `ConventionalCommitsDelivery`
  - `<increment-mode>`: `Successive`
  - `<increment-flow>`: `None`
- `Manual`
  - `<message-convention>`: `Falsy`
  - `<increment-mode>`: `None`
  - `<increment-flow>`: `None`

If `VersioningMode` is not set its default is:

```yaml
VersioningMode: Default # 'Default' resolves internally to 'ContinousDelivery'
```

## Schema -> `<preset:object>`

```yaml
# More detailed
VersioningMode:
  Preset: <preset:string> (optional)
  MessageConvention: <message-convention:string> (optional if <preset> is set)
  IncrementMode: <increment-mode:string> (optional if <preset> is set)
  RightShiftWhenZeroMajor: <right-shift-when-zero-major:boolean> (optional)
```

If `VersioningMode` is defined as object its default is:

```yaml
VersioningMode:
  Preset: (not set)
  MessageConvention: (not set)
  IncrementMode: (not set)
  RightShiftWhenZeroMajor: false
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
- `Continous`: same as `<message-convention>` of preset `Continous*`.
- `ConventionalCommits`: same as `<message-convention>` of preset `ConventionalCommits*`.

## Schema → `<message-convention:object>`

```yaml
# With custom message convention where indicators are short-cutted by string
VersioningMode:
  ...
  MessageConvention:
    # Use it when you want to inherit a message convention
    # and extend only a part of an already existing one.
    Base: <message-convention:string> (optional)

    # All logic of how "MajorIndicators" can be defined also applies to
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
    Base: (not set) # Preset from "VersioningMode" won't be inherited!
    MajorIndicators: (not set)
    MinorIndicators: (not set)
    PatchIndicators: (not set)
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
    Base: <message-convention>
    MajorIndicators: (Base.MajorIndicators if "MajorIndicators" is not set by user)
    MinorIndicators: (Base.MinorIndicators if "MinorIndicators" is not set by user)
    PatchIndicators: (Base.PatchIndicators if "PatchIndicators" is not set by user)
```

## Schema → `<message-indicator:string>`

Available values for `<message-indicator:string>`:

- `Falsy`: indicates that every message <ins>does not increment</ins> the version
- `Truthy`: indicates that every message <ins>does increment</ins> the version
- `ConventionalCommits`
  - If used as major indicator: `^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\\([\\w\\s-]*\\))?(!:|:.*\\n\\n((.+\\n)+\\n)?BREAKING CHANGE:\\s.+)`
  - If used as minor indicator: `^(feat)(\\([\\w\\s-]*\\))?:`
  - If used as patch indicator: `^(fix)(\\([\\w\\s-]*\\))?:`
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
        Pattern: ^(fix)(\\([\\w\\s-]*\\))?: # Imitates patch indicator of "ConventionalCommits".
```

## Schema → `<increment-mode>`

Available values for `<increment-mode:string>`:

- `None`: does not increment anything
- `Consecutive`: increment only the most significant version number once if a message indicates that it increments the version
- `Successive`: increment most significant version number as often as messages indicate to increment the version
  
## Schema → `<increment-flow:string>`

Available values for `<increment-flow:string>`:

- `None`: does not lead into a flow of any version part at any circumstances
- `ZeroMajorDownstream`: does right shift version when latest version has zero major, so next major becomes next minor, next minor becomes next patch

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

## Schema → Examples

Versioning mode one-liner.

```yaml
VersioningMode: ConventionalCommitsDeployment
```

Overwrite preset.

```yaml
VersioningMode:
  Preset: ConventionalCommitsDelivery
  MessageConvention: Continous # Overwrites "MessageConvention" of preset
  IncrementMode: Successive # Overwrites "IncrementMode" of preset
  RightShiftWhenZeroMajor: true
```

Custom message convention in branch develop.

```yaml
Branches:
  - IfBranch: develop
    VersioningMode:
      Preset: ContinousDelivery
      MessageConvention:
        MajorIndicators: Falsy # Ok, we want major to never increment.
        MinorIndicators:
          - Name: Regex
            Pattern: ^(feat)(\\([\\w\\s-]*\\))?: # Let's imitate minor indicator of "ConventionalCommits".
        PatchIndicators:
          - Truthy # When major or patch didn't indicate anything already then I want at least to increment patch!
```