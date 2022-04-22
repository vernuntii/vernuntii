using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Shadowing
{
    /// <summary>
    /// Represents an shadowed configuration provider.
    /// </summary>
    public interface IShadowedConfigurationProvider : IConfigurationProvider
    {
        /// <summary>
        /// The root configuration provider.
        /// </summary>
        IConfigurationProvider RootConfigurationProvider { get; }
    }
}
