using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Extension methods for <see cref="RegexMessageIndicatorBuilder"/>.
    /// </summary>
    public static class RegexMessageIndicatorBuilderExtensions
    {
        /// <summary>
        /// With major RegEx pattern.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        public static RegexMessageIndicatorBuilder MajorRegex(this RegexMessageIndicatorBuilder builder, string? pattern) =>
            pattern is null
            ? builder.MajorRegex(default(Regex))
            : builder.MajorRegex(new Regex(pattern));

        /// <summary>
        /// With minor RegEx pattern.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        public static RegexMessageIndicatorBuilder MinorRegex(this RegexMessageIndicatorBuilder builder, string? pattern) =>
            pattern is null
            ? builder.MajorRegex(default(Regex))
            : builder.MajorRegex(new Regex(pattern));

        /// <summary>
        /// With patch RegEx pattern.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        public static RegexMessageIndicatorBuilder PatchRegex(this RegexMessageIndicatorBuilder builder, string? pattern) =>
            pattern is null
            ? builder.PatchRegex(default(Regex))
            : builder.PatchRegex(new Regex(pattern));

        /// <summary>
        /// Sets regex of part.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="regex"></param>
        /// <param name="part"></param>
        /// <exception cref="ArgumentException"></exception>
        public static RegexMessageIndicatorBuilder PartRegex(
            this RegexMessageIndicatorBuilder builder,
            Regex? regex,
            VersionPart part) => part switch {
                VersionPart.Major => builder.MajorRegex(regex),
                VersionPart.Minor => builder.MinorRegex(regex),
                VersionPart.Patch => builder.PatchRegex(regex),
                _ => throw new ArgumentException("Bad version part")
            };

        /// <summary>
        /// Sets regex of part.
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="pattern"></param>
        /// <param name="part"></param>
        /// <exception cref="ArgumentException"></exception>
        public static RegexMessageIndicatorBuilder PartRegex(
            this RegexMessageIndicatorBuilder builder, string? pattern, VersionPart part) =>
            pattern is null
            ? PartRegex(builder, default(Regex), part)
            : PartRegex(builder, new Regex(pattern), part);
    }
}
