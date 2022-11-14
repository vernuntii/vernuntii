using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration
{
    /// <summary>
    /// Creates a builder for <see cref="IConfigurationRoot"/>.
    /// </summary>
    public class ConventionalConfigurationBuilder : IConventionalConfigurationBuilder
    {
        /// <inheritdoc/>
        public IDictionary<string, object> Properties => _configurationBuilder.Properties;

        /// <inheritdoc/>
        public IList<IConfigurationSource> Sources => _configurationBuilder.Sources;

        /// <inheritdoc/>
        public IFileFinder FileFinder {
            get => _fileFinder;
            set => _fileFinder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc/>
        public IList<IConventionalFileFinder> ConventionalFileFinders { get; } = new List<IConventionalFileFinder>();

        private readonly ConfigurationBuilder _configurationBuilder = new();
        private IFileFinder _fileFinder = Configuration.FileFinder.Default;

        /// <inheritdoc/>
        public IConventionalConfigurationBuilder Add(IConfigurationSource source)
        {
            _configurationBuilder.Add(source);
            return this;
        }

        /// <inheritdoc/>
        public IConventionalConfigurationBuilder Add(IConventionalFileFinder filePrber)
        {
            ConventionalFileFinders.Add(filePrber);
            return this;
        }

        IConfigurationBuilder IConfigurationBuilder.Add(IConfigurationSource source) =>
            Add(source);

        /// <inheritdoc/>
        public IConventionalConfigurationBuilder AddConventionalFileFinder(IConventionalFileFinder fileFinder)
        {
            ConventionalFileFinders.Add(fileFinder);
            return this;
        }

        /// <inheritdoc/>
        public IConfigurationRoot Build() => _configurationBuilder.Build();
    }
}
