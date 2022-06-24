using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vernuntii.PluginSystem;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Represents the compendium of plugin actions.
    /// </summary>
    public static class PluginAction
    {
        /// <summary>
        /// A plugin that calls a handler with the first plugin of
        /// type <typeparamref name="TPlugin"/> after registration.
        /// </summary>
        public class AfterRegistration<TPlugin> : Plugin
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
            public AfterRegistration(Action<TPlugin> handler) =>
                _handler = handler ?? throw new ArgumentNullException(nameof(handler));

            /// <inheritdoc/>
            protected override void OnAfterRegistration() =>
                _handler(Plugins.First<TPlugin>());
        }
    }
}
