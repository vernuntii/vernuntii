using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Vernuntii.Configuration.Shadowing
{
    /// <summary>
    /// Represents an shadowed configuration provider that enables to add further
    /// providers to for example shadowing existing keys or add new keys.
    /// </summary>
    public class ShadowedConfigurationProvider : IShadowedConfigurationProvider
    {
        /// <inheritdoc/>
        public IConfigurationProvider RootConfigurationProvider => _configurationProviders[0];

        private readonly List<IConfigurationProvider> _configurationProviders;

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="configurationProviders"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public ShadowedConfigurationProvider(IEnumerable<IConfigurationProvider> configurationProviders)
        {
            _configurationProviders = new List<IConfigurationProvider>(configurationProviders ?? throw new ArgumentNullException(nameof(configurationProviders)));

            if (_configurationProviders.Count == 0) {
                throw new ArgumentException("At least on configuration provider is required");
            }
        }

        /// <inheritdoc/>
        public bool TryGet(string key, out string value)
        {
            for (var i = _configurationProviders.Count - 1; i >= 0; i--) {
                var configurationProvider = _configurationProviders[i];

                if (configurationProvider.TryGet(key, out value)) {
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }

        /// <inheritdoc/>
        public void Set(string key, string value) => RootConfigurationProvider.Set(key, value);

        /// <inheritdoc/>
        public void Load() => RootConfigurationProvider.Load();

        /// <inheritdoc/>
        public IChangeToken GetReloadToken() => RootConfigurationProvider.GetReloadToken();

        /// <inheritdoc/>
        public IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath) =>
            _configurationProviders.Aggregate(
                earlierKeys,
                (earlierKeysAccumulated, configurationProvider) =>
                    configurationProvider.GetChildKeys(earlierKeysAccumulated, parentPath));
    }
}
