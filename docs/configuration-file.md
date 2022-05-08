# Configuration file

The configuration file is the heart of Vernuntii. By defining properties you change fundamentally the behvaiour of Vernuntii.

- [Configuration file](#configuration-file)
- [Auto-discovery](#auto-discovery)
- [Git plugin: Branches](#git-plugin-branches)
  - [Usage](#usage)
  - [Types](#types)
  - [Defaults](#defaults)
- [Git plugin: Versioning mode](#git-plugin-versioning-mode)
  - [Usage](#usage-1)
  - [Types](#types-1)
    - [Message Indicator: `Regex`](#message-indicator-regex)
      - [Schema](#schema)
      - [Example](#example)
  - [Defaults](#defaults-1)
  - [Examples](#examples)

# Auto-discovery

The configuration file will be auto-discovered when the file name meets the following rules:

- Has the **file name** `vernuntii`
- Ends with `.json`, `.yaml` or `.yml`

Depending on integration you use the following working directory and therefore default location to start the searching from:

- MSBuild Integration: root directory of project that has the MSBuild integration installed
- GitHub Actions (`vernuntii/actions/execute`): current working directory at the time where the action was called

# Git plugin: Branches

The git plugin allows you to define branches where their rules does not affect each other.

## Usage

```yaml
# The git directory to use
GitDirectory: <git-branch:string> (optional)

# The start version when not a single version exist
StartVersion: <start-version:string> (optional)

#########################################################
# The below properties are available in "Branches" too. #
#########################################################

VersioningMode: (explained down below of this document)

# The branch reading the commits from.
Branch: <branch-name:string> (optional)

# The since-commit where to start reading from.
SinceCommit: <since-commit:string> (optional)

# The pre-release is used for pre-search or post-transformation.
# Used only in pre-search if "SearchPreRelease" is
# null.
PreRelease: <pre-release:string> (optional, inheritable)

# Pre-release escapes.
PreReleaseEscapes: <pre-release-escapes:object> (optional, inheritable)
  - Pattern: <pattern:string> (optional)
    Replacement: <replacement:string> (optional)
  - ...

# The pre-release is used for pre-search.
# If null or empty non-pre-release versions are included in
# search.
# If specified then all non-release AND version with this
# pre-release are included in search.
SearchPreRelease: <search-pre-release:string> (optional, inheritable)

# Search pre-release escapes.
SearchPreReleaseEscapes: <search-pre-release-escapes:object> (same as <pre-release-escapes>)

Branches:
  - IfBranch: <if-branch:string> (required)
    ... (same as above)
  - ...
```

## Types

- `<if-branch>`
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
- `<branch>`
  - If not specified the branch is the one evaluated from `IfBranch`
  - It must be a valid branch name (see `<if-branch>`), but
  - RegEx is NOT allowed (change my mind. :D)
- `<pre-release-escapes>`
  - `<pattern>` is recognized as **regular expression** when it begins with '/' and ends with another '/'
- `<search-pre-release-escapes>`
  - `<pattern>` is recognized as **regular expression** when it begins with '/' and ends with another '/'

## Defaults

These defaults are automatically applied.

```yaml
GitDirectory: (the location where this configuration file has been found)
StartVersion: (latest version)

# Below properties are available in "Branches" too.
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

# Git plugin: Versioning mode

The versioning mode allows you to choose one of the many available version strategies to calculate the next version.

## Usage

```yaml
# The versioning mode defined by string
VersioningMode: <preset:string> (optional)

# More detailed
VersioningMode:
  Preset: <preset:string> (optional)
  MessageConvention: <message-convention:string> (optional if <preset> is set)
  IncrementMode: <increment-mode:string> (optional if <preset> is set)
  RightShiftWhenZeroMajor: <right-shift-when-zero-major:boolean> (optional)

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
      - <message-indicator:string> (required)
      # List item can be object
      - Name: <message-indicator:string> (required)
        <additional-data>: ... (some message indicator require addtional data)

    MinorIndicators: ... (same as above)
    PatchIndicators: ... (same as above)
```

You can define `VersioningMode` also in every branch.

```yaml
Branches:
  - IfBranch: <if-branch>
    VersioningMode: (same as above)
```

## Types

- `<preset>` 
  - `Default`: same as `ContinousDelivery`
  - `ContinousDelivery` (default)
    - `<message-convention>`
      - Major indicator: `Falsy`
      - Minor indicator: `Falsy`
      - Patch indicator: `Truthy`
    - `<increment-mode>`: `Consecutive`
    - `<right-shift-when-zero-major>`: `false`
  - `ContinousDeployment`
    - `<message-convention>`
      - Major indicator: `Falsy`
      - Minor indicator: `Falsy`
      - Patch indicator: `Truthy`
    - `<increment-mode>`: `Successive`
    - `<right-shift-when-zero-major>`: `false`
  - `ConventionalCommitsDelivery`
    - `<message-convention>`
      - Major indicator: `ConventionalCommits`
      - Minor indicator: `ConventionalCommits`
      - Patch indicator: `ConventionalCommits`
    - `<increment-mode>`: `Consecutive`
  - `ConventionalCommitsDeployment`
    - `<message-convention>`: same as `ConventionalCommitsDelivery`
    - `<increment-mode>`: `Successive`
    - `<right-shift-when-zero-major>`: `false`
  - `Manual`
    - `<message-convention>`
      - Major indicator: never
      - Minor indicator: never
      - Patch indicator: never
    - `<increment-mode>`: `None`
    - `<right-shift-when-zero-major>`: `false`
- `<message-convention>`
  - `Manual`: same as `<message-convention>` of preset `Manual`.
  - `Continous`: same as `<message-convention>` of preset `Continous*`.
  - `ConventionalCommits`: same as `<message-convention>` of `ConventionalCommits*`.
- `<message-indicator>`
  - `Falsy`: always indicates that a <ins>message does not increment</ins> the version
  - `Truthy`: always indicates that a <ins>message does increment</ins> the version
  - `ConventionalCommits`
    - Major indicator: `^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\\([\\w\\s-]*\\))?(!:|:.*\\n\\n((.+\\n)+\\n)?BREAKING CHANGE:\\s.+)`
    - Minor indicator: `^(feat)(\\([\\w\\s-]*\\))?:`
    - Patch indicator: `^(fix)(\\([\\w\\s-]*\\))?:`
  - [`Regex`](#message-indicator-regex): indicates a version increment if the message could be matched against the regular expression
- `<increment-mode>`
  - `None`: does not increment anything
  - `Consecutive`: increment only the most significant version number once if a message indicates that it increments the version
  - `Successive`: increment most significant version number as often as messages indicate to increment the version
- `<right-shift-when-zero-major>`
  - `true`: does right shift version when latest version has zero major, so next major becomes next minor, next minor becomes next patch
  - `false`: does not right shift version when latest version has zero major

### Message Indicator: `Regex`

#### Schema

```yaml
Name: Regex # Required!
Pattern: <pattern:string> # A valid regular expression.
```

#### Example

```yaml
VersioningMode:
  ...
  MessageConvention:
    ...
    PatchIndicators:
      - Name: Regex 
        Pattern: `^(feat)(\\([\\w\\s-]*\\))?:` # Imitates minor indicator of "ConventionalCommits".
```

## Defaults

If `VersioningMode` is not set its default is:

```yaml
VersioningMode: Default # 'Default' resolves internally to 'ContinousDelivery'
```

If `VersioningMode` is defined as object its default is:

```yaml
VersioningMode:
  Preset: (not set)
  MessageConvention: (not set)
  IncrementMode: (not set)
  RightShiftWhenZeroMajor: false
```

If `MessageConvention` is defined as object its default is:

```yaml
VersioningMode:
  ...
  MessageConvention: 
    Base: (not set) # So preset won't be inherited!
    MajorIndicators: (not set)
    MinorIndicators: (not set)
    PatchIndicators: (not set)

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

## Examples

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