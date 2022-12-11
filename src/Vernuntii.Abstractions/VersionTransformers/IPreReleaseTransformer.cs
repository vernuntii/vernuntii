using System.Diagnostics.CodeAnalysis;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Transforms version with alternative pre-release.
    /// </summary>
    public interface IPreReleaseTransformer : IVersionTransformer
    {
        /// <summary>
        /// The pre-release to be used to transform version.
        /// <see langword="null"/> represents a release and
        /// non-null a pre-release.
        /// </summary>
        /// <remarks>
        /// "Prospective" means here, that the resulting pre-release may actual differ from the pre-release of
        /// the version transformed by <see cref="IVersionTransformer.TransformVersion(SemVer.ISemanticVersion)"/>.
        /// </remarks>
        string? ProspectivePreRelease { get; }

        /// <summary>
        /// The amount of dots the prospective pre-release has.
        /// </summary>
        int ProspectivePreReleaseDots { get; }

        /// <summary>
        /// If <see langword="true"/>, then <see cref="ProspectivePreRelease"/> is not null.
        /// </summary>
        [MemberNotNullWhen(true, nameof(ProspectivePreRelease))]
        bool IsTransformationResultingIntoPreRelease { get; }
    }
}
