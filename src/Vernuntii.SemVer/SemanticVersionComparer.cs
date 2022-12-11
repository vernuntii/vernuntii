namespace Vernuntii.SemVer
{
    /// <summary>
    /// A comparer for <see cref="ISemanticVersion"/>.
    /// </summary>
    public sealed class SemanticVersionComparer : ISemanticVersionComparer
    {
        /// <summary>
        /// A comparer that uses only the version core for comparison.
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
        public static int Compare(ISemanticVersion? x, ISemanticVersion? y, SemanticVersionComparisonMode SemanticVersionComparisonMode) =>
            GetComparer(SemanticVersionComparisonMode).Compare(x, y);

        /// <summary>
        /// The pre-release comparer. Is by default <see cref="PreReleaseIdentifierComparer.Default"/>.
        /// </summary>
        public IDotSplittedIdentifierComparer PreReleaseComparer { get; init; } = PreReleaseIdentifierComparer.Default;
        /// <summary>
        /// The pre-release comparer. Is by default <see cref="PreReleaseIdentifierComparer.Default"/>.
        /// </summary>
        public IDotSplittedIdentifierComparer BuildComparer { get; init; } = BuildIdentifierComparer.Default;

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
        public bool Equals(ISemanticVersion? x, ISemanticVersion? y) =>
            Compare(x, y) == 0;

        /// <summary>
        /// Gives a hash code based on the normalized version string.
        /// </summary>
        /// <param name="obj"></param>
        public int GetHashCode(ISemanticVersion? obj)
        {
            if (obj is null) {
                return 0;
            }

            var hashCode = new HashCode();
            hashCode.Add(obj.Major);
            hashCode.Add(obj.Minor);
            hashCode.Add(obj.Patch);

            if ((_comparisonMode == SemanticVersionComparisonMode.VersionRelease || _comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild)
                && obj.IsPreRelease) {
                hashCode.Add(PreReleaseComparer.GetHashCode(obj.PreRelease));
            }

            if (_comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild && obj.HasBuild) {
                hashCode.Add(BuildComparer.GetHashCode(obj.Build));
            }

            return hashCode.ToHashCode();
        }

        /// <summary>
        /// Compares two versions.
        /// </summary>
        public int Compare(ISemanticVersion? x, ISemanticVersion? y)
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
                        result = PreReleaseComparer.Compare(x.PreReleaseIdentifiers, y.PreReleaseIdentifiers);

                        if (result != 0) {
                            return result;
                        }
                    }

                    // Compare build
                    if (_comparisonMode == SemanticVersionComparisonMode.VersionReleaseBuild) {
                        result = BuildComparer.Compare(x.BuildIdentifiers, y.BuildIdentifiers);

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
