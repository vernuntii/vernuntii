using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration.Queueing
{
    /// <inheritdoc/>
    public class QueuedConfigurationProviderBuilder : IQueuedConfigurationProviderBuilder
    {
        /// <inheritdoc/>
        public IConfigurationProvider RootConfigurationProvider => _configurationProviders[0];

        private readonly List<IConfigurationProvider> _configurationProviders = new();

        /// <summary>
        /// Creates an instance of <see cref="QueuedConfigurationProviderBuilder"/>.
        /// </summary>
        /// <param name="configurationProvider"></param>
        public QueuedConfigurationProviderBuilder(IConfigurationProvider configurationProvider) =>
            AddConfigurationProvider(configurationProvider);

        private void AddConfigurationProvider(IConfigurationProvider configurationProvider) =>
            _configurationProviders.Add(configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider)));

        /// <inheritdoc/>
        public void Enqueue(IConfigurationProvider configurationProvider) =>
            AddConfigurationProvider(configurationProvider);

        /// <inheritdoc/>
        public IQueuedConfigurationProvider Build() =>
            new QueuedConfigurationProvider(_configurationProviders);
    }
}
