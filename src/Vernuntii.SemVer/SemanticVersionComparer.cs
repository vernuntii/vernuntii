namespace Vernuntii.SemVer
{
    /// <summary>
    /// A comparer for <see cref="SemanticVersion"/>.
    /// </summary>
    public sealed class SemanticVersionComparer : ISemanticVersionComparer
    {
        /// <summary>
        /// A comparer that uses only the version numbers.
        /// </summary>
        public static readonly ISemanticVersionComparer Version = new SemanticVersionComparer(SemanticVersionComparisonMode.Version);

        /// <summary>
        /// Compares versions without comparing the build.
        /// </summary>
        public static readonly ISemanticVersionComparer VersionRelease = new SemanticVersionComparer(SemanticVersionComparisonMode.VersionRelease);

        /// <summary>
        /// A version comparer that follows SemVer 2.0.0 rules.
        /// </summary>
        public static readonly ISemanticVersionComparer VersionReleaseBuild = new SemanticVersionComparer(SemanticVersionComparisonMode.VersionReleaseBuild);

        #region private static methods

        /// <summary>
        /// Pre-release identifiers are compared as numbers if they are numeric, otherwise they will be compared
        /// as strings.
        /// </summary>
        private static int ComparePreReleaseIdentifier(string preRelease, string otherPreRelease)
        {
            int result;

            // Check for numeric versions
            bool isPreReleaseNumeric = int.TryParse(preRelease, out var versionNumber);
            bool isOtherPreReleaseNumeric = int.TryParse(otherPreRelease, out var otherVersionNumber);

            // If both versions are numeric we can compare as numbers
            if (isPreReleaseNumeric && isOtherPreReleaseNumeric) {
                result = versionNumber.CompareTo(otherVersionNumber);
            } else if (isPreReleaseNumeric || isOtherPreReleaseNumeric) {
                // Numeric pre-release identifiers come before string pre-release identifiers
                if (isPreReleaseNumeric) {
                    result = -1;
                } else {
                    result = 1;
                }
            } else {
                // We can ignore 2.0.0 case sensitive comparison, instead compare insensitively as specified in 2.0.1.
                result = StringComparer.OrdinalIgnoreCase.Compare(preRelease, otherPreRelease);
            }

            return result;
        }

        /// <summary>
        /// Compares pre-release identifiers.
        /// </summary>
        private static int ComparePreReleaseIdentifiers(IEnumerable<string> identifiers, IEnumerable<string> otherIdentifiers)
        {
            int result = 0;

            var identifierEnumerator = identifiers.GetEnumerator();
            var otherIdentifierEnumerator = otherIdentifiers.GetEnumerator();

            bool hasNextIdentifier = identifierEnumerator.MoveNext();
            bool hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();

            while (hasNextIdentifier || hasNextOtherIdentifier) {
                if (!hasNextIdentifier && hasNextOtherIdentifier) {
                    return -1;
                }

                if (hasNextIdentifier && !hasNextOtherIdentifier) {
                    return 1;
                }

                result = ComparePreReleaseIdentifier(identifierEnumerator.Current, otherIdentifierEnumerator.Current);

                if (result != 0) {
                    return result;
                }

                hasNextIdentifier = identifierEnumerator.MoveNext();
                hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();
            }

            return result;
        }

        /// <summary>
        /// Compares build identifiers.
        /// </summary>
        private static int CompareBuildIdentifiers(IEnumerable<string> identifiers, IEnumerable<string> otherIdentifiers)
        {
            int result = 0;

            var identifierEnumerator = identifiers.GetEnumerator();
            var otherIdentifierEnumerator = otherIdentifiers.GetEnumerator();

            bool hasNextIdentifier = identifierEnumerator.MoveNext();
            bool hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();

            while (hasNextIdentifier || hasNextOtherIdentifier) {
                if (!hasNextIdentifier && hasNextOtherIdentifier) {
                    return -1;
                }

                if (hasNextIdentifier && !hasNextOtherIdentifier) {
                    return 1;
                }

                result = StringComparer.Ordinal.Compare(identifierEnumerator.Current, otherIdentifierEnumerator.Current);

                if (result != 0) {
                    return result;
                }

                hasNextIdentifier = identifierEnumerator.MoveNext();
                hasNextOtherIdentifier = otherIdentifierEnumerator.MoveNext();
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Gets comparer via <paramref name="comparisonMode"/>.
        /// </summary>
        /// <param name="comparisonMode"></param>
        /// <exception cref="NotSupportedException"></exception>
        public static ISemanticVersionComparer GetComparer(SemanticVersionComparisonMode comparisonMode) => comparisonMode switch {
            SemanticVersionComparisonMode.Version => Version,
            SemanticVersionComparisonMode.VersionRelease => VersionRelease,
            SemanticVersionComparisonMode.VersionReleaseBuild => VersionReleaseBuild,
            _ => throw new NotSupportedException()
        };

        /// <summary>
        /// Compares the two versions using <paramref name="SemanticVersionComparisonMode"/>.
        /// </summary>
        public static int Compare(SemanticVersion? x, SemanticVersion? y, SemanticVersionComparisonMode SemanticVersionComparisonMode) =>
            GetComparer(SemanticVersionComparisonMode).Compare(x, y);

        private readonly SemanticVersionComparisonMode _comparisonMode;

        /// <summary>
        /// Creates a comparer using <see cref="SemanticVersionComparisonMode"/> as comparison mode.
        /// </summary>
        public SemanticVersionComparer() =>
            _comparisonMode = SemanticVersionComparisonMode.VersionRelease;

        /// <summary>
        /// Creates a comparer that respects the given comparison mode.
        /// </summary>
        /// <param name="SemanticVersionComparisonMode">comparison mode</param>
        public SemanticVersionComparer(SemanticVersionComparisonMode SemanticVersionComparisonMode) =>
                _comparisonMode = SemanticVersionComparisonMode;

        /// <summary>
        /// Determines if both versions are equal.
        /// </summary>
        public bool Equals(SemanticVersion? x, SemanticVersion? y) =>
            Compare(x, y) == 0;

        /// <summary>
        /// Gives a hash code based on the normalized version string.
        /// </summary>
        /// <param name="obj"></param>
        public int GetHashCode(SemanticVersion? obj)
        {
            if (obj is null) {
                return 0;
            }

            HashCode hashCode = new HashCode();
            hashCode.Add(obj.Major);
            hashCode.Add(obj.Minor);
            hashCode.Add(obj.Patch);

            if ((_comparisonMode == SemanticVersionComparisonMode.VersionRelease || _comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild)
                && obj.IsPreRelease) {
                hashCode.Add(obj.PreRelease, StringComparer.OrdinalIgnoreCase);
            }

            if (_comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild && obj.HasBuild) {
                hashCode.Add(obj.Build, StringComparer.Ordinal);
            }

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Compares two versions.
        /// </summary>
        public int Compare(SemanticVersion? x, SemanticVersion? y)
        {
            if (ReferenceEquals(x, y)) {
                return 0;
            }

            if (y is null) {
                return 1;
            }

            if (x is null) {
                return -1;
            }

            if (x is not null && y is not null) {
                /* Compare version numbers */
                var result = x.Major.CompareTo(y.Major);

                if (result != 0) {
                    return result;
                }

                result = x.Minor.CompareTo(y.Minor);

                if (result != 0) {
                    return result;
                }

                result = x.Patch.CompareTo(y.Patch);

                if (result != 0) {
                    return result;
                }

                if (_comparisonMode != SemanticVersionComparisonMode.Version) {
                    /* Compare pre-release */
                    if (x.IsPreRelease && !y.IsPreRelease) {
                        return -1;
                    }

                    if (!x.IsPreRelease && y.IsPreRelease) {
                        return 1;
                    }

                    if (x.IsPreRelease && y.IsPreRelease) {
                        result = ComparePreReleaseIdentifiers(x.PreReleaseIdentifiers, y.PreReleaseIdentifiers);

                        if (result != 0) {
                            return result;
                        }
                    }

                    // Compare build
                    if (_comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild) {
                        result = CompareBuildIdentifiers(x.BuildIdentifiers, y.BuildIdentifiers);

                        if (result != 0) {
                            return result;
                        }
                    }
                }
            }

            return 0;
        }
    }
}
