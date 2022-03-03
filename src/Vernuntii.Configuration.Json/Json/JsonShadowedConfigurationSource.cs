using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Vernuntii.Configuration.Shadowing;

namespace Vernuntii.Configuration.Json
{
    internal class JsonShadowedConfigurationSource : JsonConfigurationSource
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
