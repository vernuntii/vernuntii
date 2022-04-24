using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration
{
    internal class FileShadowedConfigurationProviderBuilderConfigurator : IFileShadowedConfigurationProviderBuilderConfigurer
    {
        /// <inheritdoc/>
        public IConfigurationProvider RootConfigurationProvider =>
            _internalShadowedConfigurationProviderBuilding.RootConfigurationProvider;

        /// <inheritdoc/>
        public FileInfo FileInfo { get; }

        private IShadowedConfigurationProviderBuilderConfigurator _internalShadowedConfigurationProviderBuilding;

        /// <summary>
        /// Creates an instance of <see cref="FileShadowedConfigurationProviderBuilderConfigurator"/>.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="internalShadowedConfigurationProviderBuilding"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal FileShadowedConfigurationProviderBuilderConfigurator(
            FileInfo fileInfo,
            IShadowedConfigurationProviderBuilderConfigurator internalShadowedConfigurationProviderBuilding)
        {
            FileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));

            _internalShadowedConfigurationProviderBuilding = internalShadowedConfigurationProviderBuilding
                ?? throw new ArgumentNullException(nameof(internalShadowedConfigurationProviderBuilding));
        }

        /// <inheritdoc/>
        public IShadowedConfigurationProviderBuilder AddShadow(IConfigurationProvider configurationProvider) =>
            _internalShadowedConfigurationProviderBuilding.AddShadow(configurationProvider);
    }
}
