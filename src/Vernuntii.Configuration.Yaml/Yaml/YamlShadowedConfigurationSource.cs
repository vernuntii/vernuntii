using Microsoft.Extensions.Configuration;
using NetEscapades.Configuration.Yaml;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration.Yaml
{
    internal class YamlShadowedConfigurationSource : YamlConfigurationSource
    {
        public Action<IShadowedConfigurationProviderBuilderConfigurator>? OnBuildShadowedConfigurationProvider { get; set; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            var configurationBuilder = new ShadowedConfigurationProviderBuilder(base.Build(builder));
            OnBuildShadowedConfigurationProvider?.Invoke(configurationBuilder);
            return configurationBuilder.Build();
        }
    }
}
