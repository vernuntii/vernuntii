using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration;
using Vernuntii.Extensions;
using Vernuntii.VersioningPresets;

namespace Vernuntii.VersionIncrementFlows
{
    /// <summary>
    /// A factory for <see cref="IVersionIncrementFlow"/>.
    /// </summary>
    public class ConfiguredVersionIncrementFlowFactory : IConfiguredVersionIncrementFlowFactory
    {
        /// <summary>
        /// The default increment flow key in configuration.
        /// </summary>
        public const string DefaultIncrementFlowKey = "IncrementFlow";

        /// <summary>
        /// The versioning preset manager.
        /// </summary>
        protected IVersioningPresetManager PresetManager { get; }

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="presetManager"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConfiguredVersionIncrementFlowFactory(IVersioningPresetManager presetManager) =>
            PresetManager = presetManager ?? throw new ArgumentNullException(nameof(presetManager));

        /// <inheritdoc/>
        public bool TryCreate(IDefaultConfigurationSectionProvider sectionProvider, [NotNullWhen(true)] out IVersionIncrementFlow? incrementFlow)
        {
            var section = sectionProvider.GetSection();

            if (section.NotExisting()) {
                incrementFlow = null;
                return false;
            }

            if (section.Value(out var sectionValue)) {
                incrementFlow = PresetManager.IncrementFlows.GetItem(sectionValue ?? nameof(InbuiltVersionIncrementFlow.None));
            } else {
                incrementFlow = section.Get<VersionIncrementFlow>();
            }

            return true;
        }
    }
}
