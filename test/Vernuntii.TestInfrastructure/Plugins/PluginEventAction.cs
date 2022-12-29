using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.Reactive;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the compendium of plugin actions.
    /// </summary>
    internal static class PluginEventAction
    {
        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="discriminator"></param>
        /// <param name="payloadHandler"></param>
        public static WhenExecuting<TPayload> OnEveryEvent<TPayload>(EventDiscriminator<TPayload> discriminator, Action<TPayload> payloadHandler) =>
            new(discriminator, payloadHandler);

        /// <summary>
        /// A plugin that calls a handler with the event of
        /// type <typeparamref name="TPayload"/> inside <see cref="OnExecution"/>.
        /// </summary>
        public class WhenExecuting<TPayload> : Plugin
        {
            private readonly EventDiscriminator<TPayload> _eventDiscriminator;
            private readonly Action<TPayload> _payloadHandler;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="discriminator"></param>
            /// <param name="payloadHandler"></param>
            public WhenExecuting(EventDiscriminator<TPayload> discriminator, Action<TPayload> payloadHandler)
            {
                _eventDiscriminator = discriminator;
                _payloadHandler = payloadHandler;
            }

            /// <inheritdoc/>
            protected override void OnExecution()
            {
                var subscription = Events.Every(_eventDiscriminator).Subscribe(_payloadHandler);
                AddDisposable(subscription);
            }
        }
    }
}
