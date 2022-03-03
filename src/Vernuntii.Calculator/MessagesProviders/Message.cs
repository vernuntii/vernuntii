using Vernuntii.Logging;
using Vernuntii.MessagesProviders;

namespace Vernuntii.MessagesProviders
{
    /// <summary>
    /// Default implementation of <see cref="IMessage"/>.
    /// </summary>
    public record Message : IMessage, IMessageProvidingDebugMessage
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public string? Content { get; init; }

        /// <summary>
        /// test
        /// </summary>
        public LoggerMessageAmendmentFactory? DebugMessageFactory { get; init; }
    }
}
