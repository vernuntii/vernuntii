
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Provides the configuration of <see cref="IShadowedConfigurationProviderBuilderConfigurator"/> with additional informations.
    /// </summary>
    public interface IFileShadowedConfigurationProviderBuilderConfigurer : IShadowedConfigurationProviderBuilderConfigurator
    {
        /// <summary>
        /// Represents the file that you are about to add via <see cref="IShadowedConfigurationProviderBuilderConfigurator.RootConfigurationProvider"/>.
        /// </summary>
        FileInfo FileInfo { get; }
    }
}
