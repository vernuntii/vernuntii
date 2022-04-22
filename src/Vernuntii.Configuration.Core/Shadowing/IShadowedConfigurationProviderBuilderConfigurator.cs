using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Shadowing
{
    /// <summary>
    /// Represents the building process of <see cref="IShadowedConfigurationProvider"/>.
    /// </summary>
    public interface IShadowedConfigurationProviderBuilderConfigurator
    {
        /// <summary>
        /// The root configuration provider.
        /// </summary>
        IConfigurationProvider RootConfigurationProvider { get; }

        /// <summary>
        /// Adds another <paramref name="configurationProvider"/>.
        /// </summary>
        /// <param name="configurationProvider"></param>
        IShadowedConfigurationProviderBuilder AddShadow(IConfigurationProvider configurationProvider);
    }
}
