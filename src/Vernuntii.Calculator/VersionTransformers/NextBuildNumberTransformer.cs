using System.Globalization;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    internal class NextBuildNumberTransformer : IVersionTransformer
    {
        public static readonly NextBuildNumberTransformer Default = new();

        bool IVersionTransformer.DoesNotTransform => false;

        private static int GetBuildNumber(string? build)
        {
            if (string.IsNullOrWhiteSpace(build)) {
                return 1;
            }

            if (!int.TryParse(build, out var metdataNumber)) {
                throw new InvalidOperationException("Cannot parse build as number");
            }

            return metdataNumber + 1;
        }

        public ISemanticVersion TransformVersion(ISemanticVersion version) =>
            version.With().Build(GetBuildNumber(version.Build).ToString(CultureInfo.InvariantCulture)).ToVersion();
    }
}
