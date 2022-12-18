using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;

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
        /// <param name="uniqueEvent"></param>
        /// <param name="payloadHandler"></param>
        public static WhenExecuting<TPayload> OnEveryEvent<TPayload>(SubjectEvent<TPayload> uniqueEvent, Action<TPayload> payloadHandler) =>
            new(uniqueEvent, payloadHandler);

        /// <summary>
        /// A plugin that calls a handler with the event of
        /// type <typeparamref name="TPayload"/> inside <see cref="OnExecution"/>.
        /// </summary>
        public class WhenExecuting<TPayload> : Plugin
        {
            private readonly SubjectEvent<TPayload> _uniqueEvent;
            private readonly Action<TPayload> _payloadHandler;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="uniqueEvent"></param>
            /// <param name="payloadHandler"></param>
            public WhenExecuting(SubjectEvent<TPayload> uniqueEvent, Action<TPayload> payloadHandler)
            {
                _uniqueEvent = uniqueEvent;
                _payloadHandler = payloadHandler;
            }

            /// <inheritdoc/>
            protected override void OnExecution()
            {
                var subscription = Events.OnEveryEvent(_uniqueEvent, _payloadHandler);
                AddDisposable(subscription);
            }
        }
    }
}
