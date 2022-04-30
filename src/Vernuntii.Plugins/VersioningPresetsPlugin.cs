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
        public IVersioningPresetManager PresetManager { get; } = VersioningpresetManager.CreateDefault();

        /// <summary>
        /// Creates an instance of this instance and poplulates <see cref="PresetManager"/> with defaults.
        /// </summary>
        public VersioningPresetsPlugin()
        {
        }

        /// <summary>
        /// <inheritdoc/> Registers <see cref="PresetManager"/> to global services as
        /// <br/> - <see cref="IVersioningPresetManager"/>
        /// <br/> - <see cref="IVersioningPresetRegistry"/>
        /// <br/> - <see cref="IMessageConventionRegistry"/>
        /// <br/> - <see cref="IHeightConventionRegistry"/>
        /// </summary>
        protected override void OnEventAggregation()
        {
            SubscribeEvent(NextVersionEvents.CreatedGlobalServices.Discriminator, services => {
                services.AddSingleton(PresetManager);
                services.AddSingleton<IVersioningPresetRegistry>(PresetManager);
                services.AddSingleton<IMessageConventionRegistry>(PresetManager);
                services.AddSingleton<IHeightConventionRegistry>(PresetManager);
                EventAggregator.GetEvent<VersioningPresetsEvents.ConfiguredGlobalServices>().Publish(services);
            });
        }
    }
}
