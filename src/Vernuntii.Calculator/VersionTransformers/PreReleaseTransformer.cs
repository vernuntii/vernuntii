using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    /// <summary>
    /// Sets pre-release of version.
    /// </summary>
    public class PreReleaseTransformer : IPreReleaseTransformer
    {
        internal static bool IsPreRelease([NotNullWhen(true)] string? preRelease) =>
            !string.IsNullOrWhiteSpace(preRelease);

        private static int CountDots(ReadOnlySpan<char> identifier)
        {
            var enumerator = identifier.GetEnumerator();
            var dots = 0;

            while (enumerator.MoveNext()) {
                if (enumerator.Current == '.') {
                    dots++;
                }
            }

            return dots;
        }

        /// <summary>
        /// A transformer that removes the pre-release.
        /// </summary>
        public static readonly PreReleaseTransformer Release = new(preRelease: null);

        /// <inheritdoc/>
        public string? ProspectivePreRelease { get; }

        /// <inheritdoc/>
        [MemberNotNullWhen(true, nameof(ProspectivePreRelease))]
        public bool IsTransformationResultingIntoPreRelease { get; }

        bool IVersionTransformer.DoesNotTransform => false;

        /// <inheritdoc/>
        public int ProspectivePreReleaseDots { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="preRelease"></param>
        public PreReleaseTransformer(string? preRelease)
        {
            IsTransformationResultingIntoPreRelease = IsPreRelease(preRelease);
            ProspectivePreRelease = IsTransformationResultingIntoPreRelease ? preRelease : null;
            ProspectivePreReleaseDots = IsTransformationResultingIntoPreRelease ? CountDots(ProspectivePreRelease) : 0;
        }

        /// <inheritdoc/>
        public ISemanticVersion TransformVersion(ISemanticVersion version)
        {
            if (!IsTransformationResultingIntoPreRelease) {
                return version.With().PreRelease(default(string)).ToVersion();
            }

            /* We want to preserve last pre-release positions who are not affected by replacement */

            if (ProspectivePreReleaseDots + 1 >= version.PreReleaseIdentifiers.Count) {
                return version.With().PreRelease(ProspectivePreRelease).ToVersion();
            }

            var newIdentifiers = version.PreReleaseIdentifiers.ToArray();
            var replacingIdentifiers = ProspectivePreRelease.Split('.').ToArray();
            replacingIdentifiers.CopyTo(newIdentifiers.AsSpan());
            return version.With().PreRelease(newIdentifiers).ToVersion();
        }
    }
}
