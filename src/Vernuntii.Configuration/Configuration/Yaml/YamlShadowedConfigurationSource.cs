using Microsoft.Extensions.Configuration;
using NetEscapades.Configuration.Yaml;
using Vernuntii.Configuration.Queueing;

namespace Vernuntii.Configuration.Yaml
{
    internal class YamlShadowedConfigurationSource : YamlConfigurationSource
    {
        public Action<IConfigurationProviderQueue>? OnBuildShadowedConfigurationProvider { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var configurationBuilder = new QueuedConfigurationProviderBuilder(base.Build(builder));
            OnBuildShadowedConfigurationProvider?.Invoke(configurationBuilder);
            return configurationBuilder.Build();
        }
    }
}
