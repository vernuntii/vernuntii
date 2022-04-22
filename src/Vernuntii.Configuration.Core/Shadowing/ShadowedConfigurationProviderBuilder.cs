using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Shadowing
{
    /// <inheritdoc/>
    public class ShadowedConfigurationProviderBuilder : IShadowedConfigurationProviderBuilder
    {
        /// <inheritdoc/>
        public IConfigurationProvider RootConfigurationProvider => _configurationProviders[0];

        private List<IConfigurationProvider> _configurationProviders = new List<IConfigurationProvider>();

        /// <summary>
        /// Creates an instance of <see cref="ShadowedConfigurationProviderBuilder"/>.
        /// </summary>
        /// <param name="configurationProvider"></param>
        public ShadowedConfigurationProviderBuilder(IConfigurationProvider configurationProvider) =>
            AddConfigurationProvider(configurationProvider);

        private void AddConfigurationProvider(IConfigurationProvider configurationProvider) =>
            _configurationProviders.Add(configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider)));

        /// <inheritdoc/>
        public IShadowedConfigurationProviderBuilder AddShadow(IConfigurationProvider configurationProvider)
        {
            AddConfigurationProvider(configurationProvider);
            return this;
        }

        /// <inheritdoc/>
        public IShadowedConfigurationProvider Build() =>
            new ShadowedConfigurationProvider(_configurationProviders);
    }
}
