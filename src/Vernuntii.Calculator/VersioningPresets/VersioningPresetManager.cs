using Vernuntii.Collections;
using Vernuntii.Configuration;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A compendium for all kind of presets.
    /// </summary>
    public class VersioningPresetManager : IVersioningPresetManager
    {
        /// <inheritdoc/>
        public IVersioningPresetRegistry VersioningPresets => _manager;
        /// <inheritdoc/>
        public IMessageConventionRegistry MessageConventions => _manager;
        /// <inheritdoc/>
        public IMessageIndicatorRegistry MessageIndicators => _manager;
        /// <inheritdoc/>
        public IConfiguredMessageIndicatorFactoryRegistry ConfiguredMessageIndicatorFactories => _manager;
        /// <inheritdoc/>
        public IHeightConventionRegistry HeightConventions => _manager;

        private Manager _manager = new Manager();

        /// <summary>
        /// Adds a preset with name and what it actually includes.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="preset"></param>
        /// <param name="mappings"></param>
        public void Add(string name, IVersioningPreset preset, VersioningPresetMappings mappings = VersioningPresetMappings.Everything) =>
            _manager.Add(name, preset, includes: mappings);

        /// <inheritdoc/>
        public void Clear()
        {
            VersioningPresets.ClearItems();
            MessageConventions.ClearItems();
            HeightConventions.ClearItems();
            MessageIndicators.ClearItems();
        }

        private class Manager :
            IVersioningPresetRegistry,
            IMessageConventionRegistry,
            IMessageIndicatorRegistry,
            IConfiguredMessageIndicatorFactoryRegistry,
            IHeightConventionRegistry
        {
            private NamedItemRegistry<IVersioningPreset> _versioningPresets = new NamedItemRegistry<IVersioningPreset>();
            private NamedItemRegistry<IMessageConvention?> _messageConventions = new NamedItemRegistry<IMessageConvention?>();
            private NamedItemRegistry<IMessageIndicator> _messageIndicators = new NamedItemRegistry<IMessageIndicator>();
            private NamedItemRegistry<IConfiguredMessageIndicatorFactory> _configuredMessageIndicatorFactories = new NamedItemRegistry<IConfiguredMessageIndicatorFactory>();
            private NamedItemRegistry<IHeightConvention?> _heightConventions = new NamedItemRegistry<IHeightConvention?>();

            IEnumerable<string> INamedItemRegistry<IVersioningPreset>.Names => _versioningPresets.Names;
            IEnumerable<string> INamedItemRegistry<IMessageConvention?>.Names => _messageConventions.Names;
            IEnumerable<string> INamedItemRegistry<IMessageIndicator>.Names => _messageIndicators.Names;
            IEnumerable<string> INamedItemRegistry<IConfiguredMessageIndicatorFactory>.Names => _configuredMessageIndicatorFactories.Names;
            IEnumerable<string> INamedItemRegistry<IHeightConvention?>.Names => _heightConventions.Names;

            IEnumerable<IVersioningPreset> INamedItemRegistry<IVersioningPreset>.Items => _versioningPresets.Items;
            IEnumerable<IMessageConvention?> INamedItemRegistry<IMessageConvention?>.Items => _messageConventions.Items;
            IEnumerable<IMessageIndicator> INamedItemRegistry<IMessageIndicator>.Items => _messageIndicators.Items;
            IEnumerable<IConfiguredMessageIndicatorFactory> INamedItemRegistry<IConfiguredMessageIndicatorFactory>.Items => _configuredMessageIndicatorFactories.Items;
            IEnumerable<IHeightConvention?> INamedItemRegistry<IHeightConvention?>.Items => _heightConventions.Items;

            private void EnsureNotReservedPresetName(string name)
            {
                if (_versioningPresets.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A preset with the name \"{name}\" already exists");
                }
            }

            private void EnsureNotReservedMessageIndicatorName(string name)
            {
                if (_messageIndicators.NamedItems.ContainsKey(name)
                    || _configuredMessageIndicatorFactories.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A message indicator or a message indicator factory with the name \"{name}\" already exists");
                }
            }

            /// <summary>
            /// Adds a preset with name and what it actually includes.
            /// </summary>
            /// <param name="name"></param>
            /// <param name="preset"></param>
            /// <param name="includes"></param>
            public void Add(string name, IVersioningPreset preset, VersioningPresetMappings includes = VersioningPresetMappings.Everything)
            {
                EnsureNotReservedPresetName(name);

                if (includes.HasFlag(VersioningPresetMappings.Everything)) {
                    ((INamedItemRegistry<IVersioningPreset>)this).AddItem(name, preset);
                }

                if (includes.HasFlag(VersioningPresetMappings.MessageConvention)) {
                    ((INamedItemRegistry<IMessageConvention?>)this).AddItem(name, preset.MessageConvention);
                }

                if (includes.HasFlag(VersioningPresetMappings.HeightConvention)) {
                    ((INamedItemRegistry<IHeightConvention?>)this).AddItem(name, preset.HeightConvention);
                }
            }

            void INamedItemRegistry<IVersioningPreset>.AddItem(string name, IVersioningPreset itme)
            {
                EnsureNotReservedPresetName(name);
                _versioningPresets.AddItem(name, itme);
            }

            void INamedItemRegistry<IMessageConvention?>.AddItem(string name, IMessageConvention? item)
            {
                EnsureNotReservedPresetName(name);
                _messageConventions.AddItem(name, item);
            }

            void INamedItemRegistry<IMessageIndicator>.AddItem(string name, IMessageIndicator item)
            {
                EnsureNotReservedMessageIndicatorName(name);
                _messageIndicators.AddItem(name, item);
            }

            void INamedItemRegistry<IConfiguredMessageIndicatorFactory>.AddItem(string name, IConfiguredMessageIndicatorFactory item)
            {
                EnsureNotReservedMessageIndicatorName(name);
                _configuredMessageIndicatorFactories.AddItem(name, item);
            }

            void INamedItemRegistry<IHeightConvention?>.AddItem(string name, IHeightConvention? item)
            {
                EnsureNotReservedPresetName(name);
                _heightConventions.AddItem(name, item);
            }

            bool INamedItemRegistry<IVersioningPreset>.ContainsName(string name) => _versioningPresets.ContainsName(name);

            bool INamedItemRegistry<IMessageConvention?>.ContainsName(string name) => _messageConventions.ContainsName(name);

            bool INamedItemRegistry<IMessageIndicator>.ContainsName(string name) => _messageIndicators.ContainsName(name);

            bool INamedItemRegistry<IConfiguredMessageIndicatorFactory>.ContainsName(string name) => _configuredMessageIndicatorFactories.ContainsName(name);

            bool INamedItemRegistry<IHeightConvention?>.ContainsName(string name) => _heightConventions.ContainsName(name);

            IVersioningPreset INamedItemRegistry<IVersioningPreset>.GetItem(string name)
            {
                if (!_versioningPresets.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Versioning preset was missing: \"{name}\"");
                }

                return value;
            }

            IMessageConvention? INamedItemRegistry<IMessageConvention?>.GetItem(string name)
            {
                if (!_messageConventions.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Message convention was missing: \"{name}\"");
                }

                return value;
            }

            IMessageIndicator INamedItemRegistry<IMessageIndicator>.GetItem(string name)
            {
                if (!_messageIndicators.NamedItems.TryGetValue(name, out var value)) {
                    var helpText = $"Message indicator was missing: \"{name}\"";

                    if (_configuredMessageIndicatorFactories.ContainsName(name)) {
                        throw new ConfigurationValidationException(helpText + " (object was expected because additional data may be required)");
                    } else {
                        throw new ItemMissingException(helpText);
                    }
                }

                return value;
            }

            IConfiguredMessageIndicatorFactory INamedItemRegistry<IConfiguredMessageIndicatorFactory>.GetItem(string name)
            {
                if (!_configuredMessageIndicatorFactories.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Message indicator factory was missing: \"{name}\"");
                }

                return value;
            }

            IHeightConvention? INamedItemRegistry<IHeightConvention?>.GetItem(string name)
            {
                if (!_heightConventions.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Height convention was missing: \"{name}\"");
                }

                return value;
            }

            void INamedItemRegistry<IVersioningPreset>.ClearItems() => _versioningPresets.ClearItems();

            void INamedItemRegistry<IMessageConvention?>.ClearItems() => _messageConventions.ClearItems();

            void INamedItemRegistry<IMessageIndicator>.ClearItems() => _messageIndicators.ClearItems();

            void INamedItemRegistry<IConfiguredMessageIndicatorFactory>.ClearItems() => _configuredMessageIndicatorFactories.ClearItems();

            void INamedItemRegistry<IHeightConvention?>.ClearItems() => _heightConventions.ClearItems();
        }
    }
}
