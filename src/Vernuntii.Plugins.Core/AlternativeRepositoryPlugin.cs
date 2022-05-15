using Vernuntii.Git;
using Vernuntii.Plugins.Events;
using Vernuntii.PluginSystem;
using Vernuntii.PluginSystem.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Vernuntii.Plugins
{
    /// <summary>
    /// A plugin that registers the custom repository before the official repository
    /// registration when the event <see cref="GitEvents.ConfiguringGlobalServices"/>
    /// is called.
    /// </summary>
    public class AlternativeRepositoryPlugin : Plugin
    {
        /// <summary>
        /// The alternative repository.
        /// </summary>
        protected IRepository Repository { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="repository"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public AlternativeRepositoryPlugin(IRepository repository) =>
            Repository = repository ?? throw new ArgumentNullException(nameof(repository));

        /// <inheritdoc/>
        protected override void OnEvents() =>
            Events.SubscribeOnce(GitEvents.ConfiguringGlobalServices, services => services.AddSingleton(Repository));
    }
}
