using Vernuntii.Collections;
using Vernuntii.Configuration;
using Vernuntii.HeightConventions;
using Vernuntii.MessageConventions;
using Vernuntii.MessageConventions.MessageIndicators;
using Vernuntii.VersionIncrementFlows;

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
        public IVersionIncrementFlowRegistry IncrementFlows => _manager;
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
            _manager.Add(name, preset, mappings: mappings);

        /// <inheritdoc/>
        public void Clear()
        {
            VersioningPresets.ClearItems();
            IncrementFlows.ClearItems();
            MessageConventions.ClearItems();
            HeightConventions.ClearItems();
            MessageIndicators.ClearItems();
        }

        private class Manager :
            IVersioningPresetRegistry,
            IVersionIncrementFlowRegistry,
            IMessageConventionRegistry,
            IMessageIndicatorRegistry,
            IConfiguredMessageIndicatorFactoryRegistry,
            IHeightConventionRegistry
        {
            private NamedItemRegistry<IVersioningPreset> _versioningPresets = new NamedItemRegistry<IVersioningPreset>();
            private NamedItemRegistry<IVersionIncrementFlow> _incrementFlows = new NamedItemRegistry<IVersionIncrementFlow>();
            private NamedItemRegistry<IMessageConvention> _messageConventions = new NamedItemRegistry<IMessageConvention>();
            private NamedItemRegistry<IMessageIndicator> _messageIndicators = new NamedItemRegistry<IMessageIndicator>();
            private NamedItemRegistry<IConfiguredMessageIndicatorFactory> _configuredMessageIndicatorFactories = new NamedItemRegistry<IConfiguredMessageIndicatorFactory>();
            private NamedItemRegistry<IHeightConvention> _heightConventions = new NamedItemRegistry<IHeightConvention>();

            IEnumerable<string> INamedItemRegistry<IVersioningPreset>.Names => _versioningPresets.Names;
            IEnumerable<string> INamedItemRegistry<IVersionIncrementFlow>.Names => _incrementFlows.Names;
            IEnumerable<string> INamedItemRegistry<IMessageConvention>.Names => _messageConventions.Names;
            IEnumerable<string> INamedItemRegistry<IMessageIndicator>.Names => _messageIndicators.Names;
            IEnumerable<string> INamedItemRegistry<IConfiguredMessageIndicatorFactory>.Names => _configuredMessageIndicatorFactories.Names;
            IEnumerable<string> INamedItemRegistry<IHeightConvention>.Names => _heightConventions.Names;

            IEnumerable<IVersioningPreset> INamedItemRegistry<IVersioningPreset>.Items => _versioningPresets.Items;
            IEnumerable<IVersionIncrementFlow> INamedItemRegistry<IVersionIncrementFlow>.Items => _incrementFlows.Items;
            IEnumerable<IMessageConvention> INamedItemRegistry<IMessageConvention>.Items => _messageConventions.Items;
            IEnumerable<IMessageIndicator> INamedItemRegistry<IMessageIndicator>.Items => _messageIndicators.Items;
            IEnumerable<IConfiguredMessageIndicatorFactory> INamedItemRegistry<IConfiguredMessageIndicatorFactory>.Items => _configuredMessageIndicatorFactories.Items;
            IEnumerable<IHeightConvention> INamedItemRegistry<IHeightConvention>.Items => _heightConventions.Items;

            private void EnsureNotReservedPresetName(string name)
            {
                if (_versioningPresets.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A preset with the name \"{name}\" already exists");
                }
            }

            private void EnsureNotReservedIncrementFlowName(string name)
            {
                if (_incrementFlows.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A increment flow with the name \"{name}\" already exists");
                }
            }

            private void EnsureNotReservedMessageConventionName(string name)
            {
                if (_messageConventions.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A message convention with the name \"{name}\" already exists");
                }
            }

            private void EnsureNotReservedHeightConventionName(string name)
            {
                if (_heightConventions.NamedItems.ContainsKey(name)) {
                    throw new ArgumentException($"A height convention with the name \"{name}\" already exists");
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
            /// <param name="mappings"></param>
            public void Add(string name, IVersioningPreset preset, VersioningPresetMappings mappings = VersioningPresetMappings.Everything)
            {
                var mapVersioningPreset = mappings.HasFlag(VersioningPresetMappings.VersioningPreset);
                var mapIncrementFlow = mappings.HasFlag(VersioningPresetMappings.IncrementFlow);
                var mapMessageConvention = mappings.HasFlag(VersioningPresetMappings.MessageConvention);
                var mapHeightConvention = mappings.HasFlag(VersioningPresetMappings.HeightConvention);

                if (mapVersioningPreset) {
                    EnsureNotReservedPresetName(name);
                }

                if (mapIncrementFlow) {
                    EnsureNotReservedIncrementFlowName(name);
                }

                if (mapMessageConvention) {
                    EnsureNotReservedMessageConventionName(name);
                }

                if (mapHeightConvention) {
                    EnsureNotReservedHeightConventionName(name);
                }

                if (mapVersioningPreset) {
                    _versioningPresets.AddItem(name, preset);
                }

                if (mapIncrementFlow) {
                    _incrementFlows.AddItem(name, preset.IncrementFlow);
                }

                if (mapMessageConvention) {
                    _messageConventions.AddItem(name, preset.MessageConvention);
                }

                if (mapHeightConvention) {
                    _heightConventions.AddItem(name, preset.HeightConvention);
                }
            }

            void INamedItemRegistry<IVersioningPreset>.AddItem(string name, IVersioningPreset item)
            {
                EnsureNotReservedPresetName(name);
                _versioningPresets.AddItem(name, item);
            }

            void INamedItemRegistry<IVersionIncrementFlow>.AddItem(string name, IVersionIncrementFlow item)
            {
                EnsureNotReservedPresetName(name);
                EnsureNotReservedIncrementFlowName(name);
                _incrementFlows.AddItem(name, item);
            }

            void INamedItemRegistry<IMessageConvention>.AddItem(string name, IMessageConvention item)
            {
                EnsureNotReservedPresetName(name);
                EnsureNotReservedMessageConventionName(name);
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

            void INamedItemRegistry<IHeightConvention>.AddItem(string name, IHeightConvention item)
            {
                EnsureNotReservedPresetName(name);
                EnsureNotReservedHeightConventionName(name);
                _heightConventions.AddItem(name, item);
            }

            bool INamedItemRegistry<IVersioningPreset>.ContainsName(string name) => _versioningPresets.ContainsName(name);

            bool INamedItemRegistry<IVersionIncrementFlow>.ContainsName(string name) => _incrementFlows.ContainsName(name);

            bool INamedItemRegistry<IMessageConvention>.ContainsName(string name) => _messageConventions.ContainsName(name);

            bool INamedItemRegistry<IMessageIndicator>.ContainsName(string name) => _messageIndicators.ContainsName(name);

            bool INamedItemRegistry<IConfiguredMessageIndicatorFactory>.ContainsName(string name) => _configuredMessageIndicatorFactories.ContainsName(name);

            bool INamedItemRegistry<IHeightConvention>.ContainsName(string name) => _heightConventions.ContainsName(name);

            IVersioningPreset INamedItemRegistry<IVersioningPreset>.GetItem(string name)
            {
                if (!_versioningPresets.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Versioning preset was missing: \"{name}\"");
                }

                return value;
            }

            IVersionIncrementFlow INamedItemRegistry<IVersionIncrementFlow>.GetItem(string name)
            {
                if (!_incrementFlows.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Increment flow was missing: \"{name}\"");
                }

                return value;
            }

            IMessageConvention INamedItemRegistry<IMessageConvention>.GetItem(string name)
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

            IHeightConvention INamedItemRegistry<IHeightConvention>.GetItem(string name)
            {
                if (!_heightConventions.NamedItems.TryGetValue(name, out var value)) {
                    throw new ItemMissingException($"Height convention was missing: \"{name}\"");
                }

                return value;
            }

            void INamedItemRegistry<IVersioningPreset>.ClearItems() => _versioningPresets.ClearItems();

            void INamedItemRegistry<IVersionIncrementFlow>.ClearItems() => _incrementFlows.ClearItems();

            void INamedItemRegistry<IMessageConvention>.ClearItems() => _messageConventions.ClearItems();

            void INamedItemRegistry<IMessageIndicator>.ClearItems() => _messageIndicators.ClearItems();

            void INamedItemRegistry<IConfiguredMessageIndicatorFactory>.ClearItems() => _configuredMessageIndicatorFactories.ClearItems();

            void INamedItemRegistry<IHeightConvention>.ClearItems() => _heightConventions.ClearItems();
        }
    }
}
