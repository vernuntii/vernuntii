using System.Globalization;
using Vernuntii.SemVer;

namespace Vernuntii.VersionTransformers
{
    internal class NextBuildNumberTransformer : ISemanticVersionTransformer
    {
        public readonly static NextBuildNumberTransformer Default = new NextBuildNumberTransformer();

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

        public SemanticVersion TransformVersion(SemanticVersion version) =>
            version.With.Build(GetBuildNumber(version.Build).ToString(CultureInfo.InvariantCulture));
    }
}
