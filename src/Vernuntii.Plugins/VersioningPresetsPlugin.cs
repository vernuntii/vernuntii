using Microsoft.Extensions.DependencyInjection;
using Vernuntii.PluginSystem.Events;
using Vernuntii.VersioningPresets;

namespace Vernuntii.PluginSystem
{
    /// <summary>
    /// A plugin to manage versioning presets.
    /// </summary>
    public class VersioningPresetsPlugin : Plugin, IVersioningPresetsPlugin
    {
        /// <inheritdoc/>
        public override int? Order => -4000;

        /// <inheritdoc/>
        public IVersioningPresetCompendium Presets { get; } = VersioningPresetCompendium.CreateDefault();

        /// <summary>
        /// Creates an instance of this instance and poplulates <see cref="Presets"/> with defaults.
        /// </summary>
        public VersioningPresetsPlugin()
        {
        }

        /// <summary>
        /// <inheritdoc/> Registers <see cref="Presets"/> to global services as
        /// <br/> - <see cref="IVersioningPresetCompendium"/>
        /// <br/> - <see cref="IVersioningPresetRegistry"/>
        /// <br/> - <see cref="IMessageConventionRegistry"/>
        /// <br/> - <see cref="IHeightConventionRegistry"/>
        /// </summary>
        protected override void OnEventAggregation()
        {
            SubscribeEvent(NextVersionEvents.CreatedGlobalServices.Discriminator, services => {
                services.AddSingleton(Presets);
                services.AddSingleton<IVersioningPresetRegistry>(Presets);
                services.AddSingleton<IMessageConventionRegistry>(Presets);
                services.AddSingleton<IHeightConventionRegistry>(Presets);
                EventAggregator.GetEvent<VersioningPresetsEvents.ConfiguredGlobalServices>().Publish(services);
            });
        }
    }
}
