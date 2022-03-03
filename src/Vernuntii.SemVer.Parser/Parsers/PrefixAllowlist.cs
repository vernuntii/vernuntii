namespace Vernuntii.SemVer.Parser.Parsers
{
    /// <summary>
    /// Validator for prefix.
    /// </summary>
    public class PrefixAllowlist : IPrefixValidator
    {
        /// <summary>
        /// Creates a default prefix allowlist containing: "v"
        /// </summary>
        public readonly static PrefixAllowlist Default = Create("v");

        /// <summary>
        /// Creates an instance of type <paramref name="allowedPrefixes"/>
        /// with <paramref name="allowedPrefixes"/>.
        /// </summary>
        /// <param name="allowedPrefixes"></param>
        public static PrefixAllowlist Create(params string[] allowedPrefixes) =>
            new PrefixAllowlist(allowedPrefixes);

        /// <summary>
        /// Collection of allowed prefixes.
        /// </summary>
        public IReadOnlyCollection<string?> AllowedPrefixes { get; }

        /// <summary>
        /// Comparer used to compare prefixes. Default is <see cref="StringComparer.Ordinal"/>.
        /// </summary>
        public StringComparer StringComparer { get; init; } = StringComparer.Ordinal;

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="allowedPrefixes"></param>
        public PrefixAllowlist(IReadOnlyCollection<string?>? allowedPrefixes) =>
            AllowedPrefixes = allowedPrefixes ?? Array.Empty<string?>();

        /// <inheritdoc/>
        public bool ValidatePrefix(string? prefix) =>
            string.IsNullOrEmpty(prefix) || AllowedPrefixes.Contains(prefix, StringComparer);
    }
}
