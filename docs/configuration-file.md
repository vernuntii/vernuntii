# Configuration file

The configuration file is the heart of Vernuntii. By defining properties you change fundamentally the processes.

- [Configuration file](#configuration-file)
  - [Auto-discovery](#auto-discovery)
  - [Git Plugin](#git-plugin)
    - [Branches](#branches)
      - [Usage](#usage)
      - [Explanations](#explanations)
      - [Defaults](#defaults)
    - [Versioning Mode](#versioning-mode)
      - [Usage](#usage-1)
      - [Structure](#structure)

## Auto-discovery

The configuration file will be auto-discovered when the file name meets one of the following conventions:

- Has the **file name** `vernuntii` AND
- Ends with `.json`, `.yaml` or `.yml`

Depending on integration you use the following default start locations are assumed:

- MSBuild Integration: root of project or in one of the directories above
- GitHub Actions (`vernuntii/actions/execute`): current directory where the action has been called

## Git Plugin

The git plugin adds the following features.

### Branches

Branches allows you to define rules for one or more branches.

#### Usage

```yaml
# The git directory to use
GitDirectory: <git-branch:string> (optional)

# Below properties are available in "Branches" too.
# If no <if-branch>-branch matches the current branch then the properties of this root level is taken.

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

#### Explanations

- `<if-branch>`
  - Can be the the full branch name or a part of it
    - main (head)
    - heads/main (head)
    - refs/heads/main (head)
    - origin/main (remote) 
    - remotes/origin/main (remote)
    - refs/remotes/origin/main (remote)
  - Can be RegEx by surrounding the pattern with '/' and '/'
    - `/feature-.*/`: all branches starting with `feature-`
    - If multiple branches are selected an exception is thrown, so use this feature with caution
  - You can specify `HEAD` when you want to match detached HEAD.
- `<branch>`
  - If not specified the branch is the one evaluated from `IfBranch`
  - It must be a valid branch name (see `<if-branch>`), but
  - RegEx is NOT allowed (change my mind. :D)

#### Defaults

These defaults are automatically applied.

```yaml
GitDirectory: (the location where this configuration file has been found)
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

### Versioning Mode

The versioning mode allows you to choose one of the many available version strategies to calculate the next version.

#### Usage

```yaml
VersioningMode: <preset>
# or more detailed
VersioningMode:
  Preset: <preset:string>
  MessageConvention: <message-convention:string>
  IncrementMode: <increment-mode:string>
```

```yaml
Branches:
  - IfBranch: <if-branch>
    VersioningMode: (same as above)
```

#### Structure

- `<preset>` 
  - `ContinousDelivery` (default)
    - `<message-convention>`
      - Major indicator: never
      - Minor indicator: never
      - Patch indicator: always
    - `<increment-mode>`: `Consecutive`
  - `ContinousDeployment`
    - `<message-convention>`
      - Major indicator: never
      - Minor indicator: never
      - Patch indicator: always
    - `<increment-mode>`: `Successive`
  - `ConventionalCommitsDelivery`
    - `<message-convention>`
      - Major indicator: `^(build|chore|ci|docs|feat|fix|perf|refactor|revert|style|test)(\\([\\w\\s-]*\\))?(!:|:.*\\n\\n((.+\\n)+\\n)?BREAKING CHANGE:\\s.+)`
      - Minor indicator: `^(feat)(\\([\\w\\s-]*\\))?:`
      - Patch indicator: `^(fix)(\\([\\w\\s-]*\\))?:`
    - `<increment-mode>`: `Consecutive`
  - `ConventionalCommitsDeployment`
    - `<message-convention>`: same as `ConventionalCommitsDelivery`
    - `<increment-mode>`: `Successive`
  - `Manual`
    - `<message-convention>`
      - Major indicator: never
      - Minor indicator: never
      - Patch indicator: never
    - `<increment-mode>`: `None`
- `<message-convention>`
  - `Manual`: same as `Manual`.`<message-convention>`
  - `Continous`: same as `Continous*`.`<message-convention>`
  - `ConventionalCommits`: same as `ConventionalCommits*`.`<message-convention>`
- `<increment-mode>`
  - `None`: does not increment anything
  - `Consecutive`: increment only the most significant version number once
  - `Successive`: increment most significant version number as often as indicated