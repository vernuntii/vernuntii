using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// A plugin that calls a handler with the first plugin of
    /// type <typeparamref name="TPlugin"/> after registration.
    /// </summary>
    public class PluginActionAfterRegistrationPlugin<TPlugin> : Plugin
        where TPlugin : IPlugin
    {
        private Action<TPlugin> _handler { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="handler">
        /// The handler that is called after registration.
        /// </param>
        /// <exception cref="ArgumentNullException"></exception>
        public PluginActionAfterRegistrationPlugin(Action<TPlugin> handler) =>
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));

        /// <inheritdoc/>
        protected override void OnAfterRegistration() =>
            _handler(Plugins.First<TPlugin>());
    }
}
