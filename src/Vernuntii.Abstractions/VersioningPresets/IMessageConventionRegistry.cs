using Vernuntii.MessageConventions;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Registry for message conventions.
    /// </summary>
    public interface IMessageConventionRegistry
    {
        /// <summary>
        /// Message convention identifiers.
        /// </summary>
        IEnumerable<string> MessageConventionIdentifiers { get; }

        /// <summary>
        /// Adds a message convention associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageConvention"></param>
        void AddMessageConvention(string name, IMessageConvention? messageConvention);

        /// <summary>
        /// Gets the message convention by name.
        /// </summary>
        /// <param name="name"></param>
        IMessageConvention? GetMessageConvention(string name);

        /// <summary>
        /// Clears all message conventions.
        /// </summary>
        void ClearMessageConventions();
    }
}
