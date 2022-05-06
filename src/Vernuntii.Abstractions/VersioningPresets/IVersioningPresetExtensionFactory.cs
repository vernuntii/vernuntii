using Microsoft.Extensions.Configuration;

namespace Vernuntii.VersioningPresets
{
    /// <summary>
    /// A factory for <see cref="IVersioningPresetExtension"/>.
    /// </summary>
    public interface IVersioningPresetExtensionFactory
    {
        /// <summary>
        /// A factory for <see cref="IVersioningPresetExtension"/>.
        /// </summary>
        /// <param name="branchConfiguration"></param>
        IVersioningPresetExtension Create(IConfiguration branchConfiguration);
    }
}
