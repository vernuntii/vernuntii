using System.Diagnostics.CodeAnalysis;
using System.Text;
using Vernuntii.SemVer.Parser;

namespace Vernuntii.SemVer
{
    /// <summary>
    /// Extension methods for <see cref="ISemanticVersion"/>
    /// </summary>
    public static class SemanticVersionExtensions
    {
        /// <summary>
        /// Creates a builder with template.
        /// </summary>
        /// <param name="version"></param>
        public static SemanticVersionBuilder With(this ISemanticVersion version) =>
            new SemanticVersionBuilder(version);

        /// <summary>
        /// Gets Parser or default.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="parser"></param>
        [return: NotNullIfNotNull("parser")]
        public static ISemanticVersionParser? GetParserOrDefault(this ISemanticVersion version, ISemanticVersionParser? parser)
        {
            if (version is ISemanticVersionParserProvider parserProvider) {
                return parserProvider.Parser;
            } else {
                return parser;
            }
        }

        /// <summary>
        /// Gets Parser or default.
        /// </summary>
        /// <param name="version"></param>
        public static ISemanticVersionParser GetParserOrStrict(this ISemanticVersion version) =>
            GetParserOrDefault(version, SemanticVersionParser.Strict);

        /// <summary>
        /// Gets the version in a custom format.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="format"></param>
        public static string Format(this ISemanticVersion version, SemanticVersionFormat format)
        {
            var stringBuilder = new StringBuilder();

            if (format.HasFlag(SemanticVersionFormat.Prefix)) {
                AppendMaybePrefix(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.Version)) {
                AppendVersion(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.PreRelease)) {
                AppendMaybePreRelease(stringBuilder);
            }

            if (format.HasFlag(SemanticVersionFormat.Build)) {
                AppendMaybeBuild(stringBuilder);
            }

            return stringBuilder.ToString();

            void AppendMaybePrefix(StringBuilder stringBuilder)
            {
                if (version.HasPrefix) {
                    stringBuilder.Append(version.Prefix);
                }
            }

            void AppendVersion(StringBuilder stringBuilder)
            {
                stringBuilder.Append(version.Major);
                stringBuilder.Append('.');
                stringBuilder.Append(version.Minor);
                stringBuilder.Append('.');
                stringBuilder.Append(version.Patch);
            }

            void AppendMaybePreRelease(StringBuilder stringBuilder)
            {
                if (version.IsPreRelease) {
                    stringBuilder.Append('-');
                    stringBuilder.Append(version.PreRelease);
                }
            }

            void AppendMaybeBuild(StringBuilder stringBuilder)
            {
                if (version.HasBuild) {
                    stringBuilder.Append('+');
                    stringBuilder.Append(version.Build);
                }
            }
        }
    }
}
