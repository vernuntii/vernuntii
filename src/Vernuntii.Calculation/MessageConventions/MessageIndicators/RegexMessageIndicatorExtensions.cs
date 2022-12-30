namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Extension methods for <see cref="IRegexMessageIndicator"/>.
    /// </summary>
    public static class RegexMessageIndicatorExtensions
    {
        /// <summary>
        /// Creates an builder to copy <paramref name="messageIndiactor"/>.
        /// </summary>
        /// <param name="messageIndiactor"></param>
        public static RegexMessageIndicatorBuilder With(this IRegexMessageIndicator messageIndiactor) =>
            new(messageIndiactor);
    }
}
