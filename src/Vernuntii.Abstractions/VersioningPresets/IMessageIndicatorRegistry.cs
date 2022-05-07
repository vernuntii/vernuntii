using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A registry for instances of <see cref="IMessageIndicator"/>.
    /// </summary>
    public interface IMessageIndicatorRegistry
    {
        /// <summary>
        /// Message indicator identifiers.
        /// </summary>
        IEnumerable<string> MessageIndicatorIdentifiers { get; }

        /// <summary>
        /// Adds a message indicator associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageIndicator"></param>
        void AddMessageIndicator(string name, IMessageIndicator messageIndicator);

        /// <summary>
        /// Gets the message indicator by name.
        /// </summary>
        /// <param name="name"></param>
        IMessageIndicator GetMessageIndicator(string name);

        /// <summary>
        /// Clears all message indicators.
        /// </summary>
        void ClearMessageConventions();
    }
}
