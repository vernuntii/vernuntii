using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// A defualt configuration section provider.
    /// </summary>
    public interface IDefaultConfigurationSectionProvider
    {
        /// <summary>
        /// Gets the default section.
        /// </summary>
        IConfigurationSection GetSection();

        /// <summary>
        /// Gets the alternative section.
        /// </summary>
        /// <param name="alternativeKey"></param>
        IConfigurationSection GetSection(string alternativeKey);
    }
}
