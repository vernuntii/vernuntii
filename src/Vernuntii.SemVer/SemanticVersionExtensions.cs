using System.Diagnostics.CodeAnalysis;
using System.Text;

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
            new(version);

        /// <summary>
        /// Gets Parser or default.
        /// </summary>
        /// <param name="version"></param>
        /// <param name="context"></param>
        [return: NotNullIfNotNull(nameof(context))]
        public static ISemanticVersionContext? GetContextOrDefault(this ISemanticVersion version, ISemanticVersionContext? context)
        {
            if (version is ISemanticVersionContextProvider parserProvider) {
                return parserProvider.Context;
            } else {
                return context;
            }
        }

        /// <summary>
        /// Gets Parser or default.
        /// </summary>
        /// <param name="version"></param>
        public static ISemanticVersionContext GetContextOrStrict(this ISemanticVersion version) =>
            GetContextOrDefault(version, SemanticVersionContext.Strict);

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

            if (format.HasFlag(SemanticVersionFormat.VersionCore)) {
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
