using Vernuntii.Configuration;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A factory for <see cref="IVersioningPreset"/>.
    /// </summary>
    public interface IConfiguredVersioningPresetFactory
    {
        /// <summary>
        /// Creates a versioning preset from configuration section.
        /// </summary>
        /// <param name="sectionProvider"></param>
        IVersioningPreset Create(IDefaultConfigurationSectionProvider sectionProvider);
    }
}
