using Microsoft.Extensions.Configuration;
using Vernuntii.Configuration.Queueing;

namespace Vernuntii.Configuration
{
    internal class FileDerivedConfigurationProviderQueue : IFileDerivedConfigurationProviderQueue
    {
        /// <inheritdoc/>
        public IConfigurationProvider RootConfigurationProvider =>
            _configurationProviderQueue.RootConfigurationProvider;

        /// <inheritdoc/>
        public FileInfo FileInfo { get; }

        private readonly IConfigurationProviderQueue _configurationProviderQueue;

        /// <summary>
        /// Creates an instance of <see cref="FileDerivedConfigurationProviderQueue"/>.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="configurationProviderQueue"></param>
        /// <exception cref="ArgumentNullException"></exception>
        internal FileDerivedConfigurationProviderQueue(
            FileInfo fileInfo,
            IConfigurationProviderQueue configurationProviderQueue)
        {
            FileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));

            _configurationProviderQueue = configurationProviderQueue
                ?? throw new ArgumentNullException(nameof(configurationProviderQueue));
        }

        /// <inheritdoc/>
        public void Enqueue(IConfigurationProvider configurationProvider) =>
            _configurationProviderQueue.Enqueue(configurationProvider);
    }
}
