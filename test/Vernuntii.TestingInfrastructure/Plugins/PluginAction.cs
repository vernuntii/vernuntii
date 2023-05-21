using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the compendium of advanced plugin actions.
    /// </summary>
    internal static class PluginAction
    {
        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="executionHandler"></param>
        public static PluginDescriptor HandleEvents(Action<IEventSystem> executionHandler) =>
            PluginDescriptor.Create(new EventsHandlerPlugin(executionHandler));

        /// <summary>
        /// Creates
        /// </summary>
        /// <param name="pluginHandler"></param>
        public static PluginDescriptor HandlePlugin<TPlugin>(Action<TPlugin> pluginHandler)
            where TPlugin : IPlugin =>
            PluginDescriptor.Create(sp => ActivatorUtilities.CreateInstance<PluginHandlerPlugin<TPlugin>>(sp, pluginHandler));

        /// <summary>
        /// A plugin that calls a handler with the first plugin of
        /// type <typeparamref name="TPlugin"/> after registration.
        /// </summary>
        private class PluginHandlerPlugin<TPlugin> : Plugin
            where TPlugin : IPlugin
        {
            private readonly IPluginRegistry _pluginRegistry;
            private Action<TPlugin> _handler { get; }

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="pluginRegistry"></param>
            /// <param name="handler">
            /// The handler that is called after registration.
            /// </param>
            /// <exception cref="ArgumentNullException"></exception>
            public PluginHandlerPlugin(IPluginRegistry pluginRegistry, Action<TPlugin> handler)
            {
                _pluginRegistry = pluginRegistry;
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            /// <inheritdoc/>
            protected override void OnExecution() =>
                _handler(_pluginRegistry.GetPlugin<TPlugin>());
        }

        private class EventsHandlerPlugin : Plugin
        {
            private readonly Action<IEventSystem> _payloadHandler;

            /// <summary>
            /// Creates an instance of this type.
            /// </summary>
            /// <param name="payloadHandler"></param>
            public EventsHandlerPlugin(Action<IEventSystem> payloadHandler)
            {
                _payloadHandler = payloadHandler;
            }

            /// <inheritdoc/>
            protected override void OnExecution() =>
                _payloadHandler(Events);
        }
    }
}
