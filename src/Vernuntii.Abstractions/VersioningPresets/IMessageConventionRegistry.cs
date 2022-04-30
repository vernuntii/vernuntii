using Vernuntii.MessageConventions;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// Registry for message conventions.
    /// </summary>
    public interface IMessageConventionRegistry
    {
        /// <summary>
        /// Data source of message conventions.
        /// </summary>
        IReadOnlyDictionary<string, IMessageConvention?> MessageConventions { get; }

        /// <summary>
        /// Adds a message convention associated to a name.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="messageConvention"></param>
        void AddMessageConvention(string name, IMessageConvention? messageConvention);

        /// <summary>
        /// Clears all message conventions.
        /// </summary>
        void ClearMessageConventions();
    }
}
