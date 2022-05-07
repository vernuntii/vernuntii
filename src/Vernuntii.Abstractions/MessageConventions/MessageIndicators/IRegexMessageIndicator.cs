using System.Text.RegularExpressions;

namespace Vernuntii.MessageConventions.MessageIndicators
{
    /// <summary>
    /// Regex message indicator.
    /// </summary>
    public interface IRegexMessageIndicator : IMessageIndicator, IEquatable<IRegexMessageIndicator>
    {
        /// <summary>
        /// Indicates major if message patches RegEx.
        /// </summary>
        Regex? MajorRegex { get; init; }
        /// <summary>
        /// Indicates minor if message patches RegEx.
        /// </summary>
        Regex? MinorRegex { get; init; }
        /// <summary>
        /// Indicates patch if message patches RegEx.
        /// </summary>
        Regex? PatchRegex { get; init; }
    }
}
