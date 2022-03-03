using Vernuntii.Logging;

namespace Vernuntii.MessagesProviders
{
    /// <summary>
    /// Enabled a message to have a debug message.
    /// </summary>
    public interface IMessageProvidingDebugMessage
    {
        /// <summary>
        /// Gets a debug message.
        /// </summary>
        LoggerMessageAmendmentFactory? DebugMessageFactory { get; }
    }
}
