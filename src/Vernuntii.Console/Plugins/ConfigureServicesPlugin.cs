using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Vernuntii.PluginSystem.Reactive;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// Helper class for creating an instance of <see cref="ConfigureServicesPlugin{TServices}"/>.
    /// </summary>
    public static class ConfigureServicesPlugin
    {
        /// <summary>
        /// Creates an instance of this type from <paramref name="eventTemplate"/>.
        /// </summary>
        /// <param name="eventTemplate"></param>
        public static ConfigureServicesPlugin<TServices> FromEvent<TServices>(EventDiscriminator<TServices> eventTemplate)
            where TServices : IServiceCollection =>
            new(eventTemplate);
    }

    /// <summary>
    /// This plugin represents an extension point to an event that provides <see cref="IServiceCollection"/>.
    /// On each event the yet registered actions are applied on the service collection.
    /// </summary>
    /// <typeparam name="TServices"></typeparam>
    public sealed class ConfigureServicesPlugin<TServices> : Plugin
        where TServices : IServiceCollection
    {
        private readonly EventDiscriminator<TServices> _eventTemplate;
        private readonly List<Action<IServiceCollection>> _configureServicesActions = new();

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="eventTemplate"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConfigureServicesPlugin(EventDiscriminator<TServices> eventTemplate) =>
            _eventTemplate = eventTemplate ?? throw new ArgumentNullException(nameof(eventTemplate));

        /// <summary>
        /// Configures
        /// </summary>
        /// <param name="configureServices"></param>
        /// <returns>A disposable that removes <paramref name="configureServices"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IDisposable ConfigureServices(Action<IServiceCollection> configureServices)
        {
            _configureServicesActions.Add(configureServices ?? throw new ArgumentNullException(nameof(configureServices)));
            return DelegatingDisposable.Create(() => _configureServicesActions.Remove(configureServices));
        }

        /// <inheritdoc/>
        protected override void OnExecution()
        {
            Events
                .Every(_eventTemplate)
                .Subscribe(services => {
                    foreach (var configureServices in _configureServicesActions) {
                        configureServices(services);
                    }
                })
                .DisposeWhenDisposing(this);
        }
    }
}
