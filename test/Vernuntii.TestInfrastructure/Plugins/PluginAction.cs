using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the compendium of plugin actions.
    /// </summary>
    internal static class PluginAction
    {
        /// <summary>
        /// A plugin that calls a handler with the first plugin of
        /// type <typeparamref name="TPlugin"/> after registration.
        /// </summary>
        public class WhenExecuting<TPlugin> : Plugin
            where TPlugin : IPlugin
        {
            /// <summary>
            /// Creates
            /// </summary>
            /// <param name="handler"></param>
            /// <returns></returns>
            public static PluginDescriptor CreatePluginDescriptor(Action<TPlugin> handler) =>
                PluginDescriptor.Create(sp => ActivatorUtilities.CreateInstance<WhenExecuting<TPlugin>>(sp, handler));

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
            public WhenExecuting(IPluginRegistry pluginRegistry, Action<TPlugin> handler)
            {
                _pluginRegistry = pluginRegistry;
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));
            }

            /// <inheritdoc/>
            protected override void OnExecution() =>
                _handler(_pluginRegistry.GetPlugin<TPlugin>());
        }
    }
}
