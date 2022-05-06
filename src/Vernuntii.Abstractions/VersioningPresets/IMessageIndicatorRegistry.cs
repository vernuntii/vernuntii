using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A registry for instances of <see cref="IMessageIndicator"/>.
    /// </summary>
    public interface IMessageIndicatorRegistry
    {
        /// <summary>
        /// Data source of message indicators.
        /// </summary>
        IReadOnlyDictionary<string, IMessageIndicator> MessageIndicators { get; }

        /// <summary>
        /// Adds a message indicator associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageIndicator"></param>
        void AddMessageIndicator(string name, IMessageIndicator messageIndicator);

        /// <summary>
        /// Clears all message indicators.
        /// </summary>
        void ClearMessageConventions();
    }
}
