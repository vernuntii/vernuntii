using Microsoft.Extensions.Configuration;

namespace Vernuntii.Configuration
{
    internal class DefaultConfigurationSectionProvider : IDefaultConfigurationSectionProvider
    {
        private readonly IConfiguration _configuration;
        private readonly string _defaultKey;

        public DefaultConfigurationSectionProvider(IConfiguration configuration, string defaultKey)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _defaultKey = defaultKey ?? throw new ArgumentNullException(nameof(defaultKey));
        }

        private IConfigurationSection GetSectionCore(string key) =>
            _configuration.GetSection(key);

        public IConfigurationSection GetSection() =>
            GetSectionCore(_defaultKey);

        public IConfigurationSection GetSection(string alternativeKey) =>
            GetSectionCore(alternativeKey);
    }
}
