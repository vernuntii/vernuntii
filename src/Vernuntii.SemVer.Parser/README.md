# Vernuntii.SemVer.Parser

[![Nuget][vernuntii-semver-parser-nuget-badge]][vernuntii-semver-parser-nuget]

This parser has been implemented out of the urge to parse a [SemVer-compatible version](https://semver.org/) string independently of any version class that is currently capable of to parse such a version string in a more or less correct way.

## Foreword

You should use this library when

- you want to check which part of version failed
- you want to be able to swap the implementation of how
  - the version,
  - the pre-release or
  - the build is parsed
- you want to built your OWN version class because
  - you don't want to wrap any other version class, so you want to be more independent
  - you don't want to reimplement the parsing algorithms such version classes are using
- you want to bypass the way over a version class to parse a version string because of
  - high parse frequency and therefore better performance

## Comparing to Nuget.Versioning

The most common library with a SemVer version class that parses you a version string is currently [Nuget.Versioning](https://www.nuget.org/packages/NuGet.Versioning). It provides you the `SemanticVersion` class with a static `SemanticVersion.Parse` and optionally a `SemanticVersion.TryParse` function. It does not more than it states, it simply parses a SemVer-compatible version string.

### Contra arguments

Beside of the facts Vernuntii.SemVer.Parser wants to offer you there are no issues: The parse functions and the public constructors check for SemVer rules.

<!-- The only special is that their version comparer follows the SemVer rules specified in this pull request [Semantic Versioning 2.1.0](https://github.com/semver/semver/pull/276), but has never been merged. It simply says to compare versions ordinal and case insensitively, instead of compare versions ordinal and case sensitively. -->

[vernuntii-semver-parser-nuget]: https://www.nuget.org/packages/Vernuntii.SemVer.Parser
[vernuntii-semver-parser-nuget-badge]: https://img.shields.io/nuget/v/Vernuntii.SemVer.Parser