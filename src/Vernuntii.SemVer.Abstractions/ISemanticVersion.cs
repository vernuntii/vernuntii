namespace Vernuntii.SemVer
{
    /// <summary>
    /// A strict semantic version implementation.
    /// </summary>
    public interface ISemanticVersion : IComparable, IComparable<ISemanticVersion>, IEquatable<ISemanticVersion>
    {
        /// <summary>
        /// Prefix of the version.
        /// </summary>
        string Prefix { get; }

        /// <summary>
        /// Checks whether a prefix exists.
        /// </summary>
        bool HasPrefix { get; }

        /// <summary>
        /// Major version.
        /// </summary>
        uint Major { get; }

        /// <summary>
        /// Minor version.
        /// </summary>
        uint Minor { get; }

        /// <summary>
        /// Patch version.
        /// </summary>
        uint Patch { get; }

        /// <summary>
        /// Dot-splitted pre-release identifiers.
        /// </summary>
        IReadOnlyList<string> PreReleaseIdentifiers { get; }

        /// <summary>
        /// Dot-separated pre-release.
        /// </summary>
        string PreRelease { get; }

        /// <summary>
        /// True if pre-release exists for the version.
        /// </summary>
        bool IsPreRelease { get; }

        /// <summary>
        /// Dot-splitted build identifiers.
        /// </summary>
        IReadOnlyList<string> BuildIdentifiers { get; }

        /// <summary>
        /// Dot-separated build.
        /// </summary>
        string Build { get; }

        /// <summary>
        /// True if build exists for the version.
        /// </summary>
        bool HasBuild { get; }
    }
}
